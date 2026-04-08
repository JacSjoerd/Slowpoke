using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Slowpoke.Networking.Login
{
  internal static class LoginSystem
  {
    public static bool HandleLogin(Client client, ClientPacket msg)
    {
      var context = LoginContext.FromPacket(client, msg);
      Logger.Debug($"[{client.Name}:HandleLogin] logged in.");

      return true;
    }

    public static bool HandleRedirect(Client client, ServerPacket msg)
    {
      var context = RedirectContext.FromPacket(client, msg);

      client.Name = context.Name;
      client.Redirected = true;
      if (Server.DAServer.ContainsKey(context.Name))
        Server.DAServer[context.Name] = context.Port;
      else
        Server.DAServer.Add(context.Name, context.Port);
      Array.Reverse(context.Address);

      RedirectContext.ToPacket(context, msg);

      Logger.Debug($"[{client.Name}:HandleRedirect] redirecting {string.Join(".", context.Address)}:{context.Port} to 127.0.0.1:{context.Port}.");

      return true;
    }

    public static bool HandleClientJoin(Client client, ClientPacket msg)
    {
      var context = JoinContext.FromPacket(client, msg);
      var name = context.Name;

      client.Name = name;
      client.Tab.Text = name;
      client.Seed = context.Seed;
      client.Key = context.Key;
      client.KeyTable = GetHashString(name);

      Logger.Debug($"[{client.Name}:HandleClientJoin] joined.");

      return true;
    }

    public static bool HandleLogout(Client client, ClientPacket msg)
    {
      var context = LogoutContext.FromPacket(client, msg);

      client.logoff = context.LoggingOff;

      RemoveClientFromLists(client);

      Logger.Debug($"Client {client.Name} logged out. Logoff: {client.logoff}");

      return true;
    }

    private static void RemoveClientFromLists(Client client)
    {
      // Remove client from servers alts list
      if (Server.Alts.ContainsKey(client.Name.ToLower()))
        Server.Alts.Remove(client.Name.ToLower());

      // Remove client from alt target lists
      foreach (Client client1 in Server.Alts.Values.ToArray<Client>())
      {
        if (client1 != null && client1.targetplayer != null)
        {
          foreach (targetPlayer targetPlayer in client1.targetplayer)
            targetPlayer?.updatePlayerTargets();
        }
      }
    }

    private static byte[] GetHashString(string input)
    {
      string hashString = Program.GetHashString(Program.GetHashString(input));
      for (int index = 0; index < 31; ++index)
        hashString += Program.GetHashString(hashString);
      return Encoding.ASCII.GetBytes(hashString);
    }
  }
}
