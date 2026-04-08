using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Networking.Login
{
  internal class RedirectContext
  {
    internal Client Client;
    internal byte[] Address;
    internal int Port;
    internal string Name;

    internal static RedirectContext FromPacket(Client client, ServerPacket msg)
    {
      byte[] address = msg.Read(4);
      ushort port = msg.ReadUInt16();
      msg.ReadByte();
      msg.ReadByte();
      msg.Read(msg.ReadByte());
      string name = msg.ReadString(msg.ReadByte());
      msg.ReadUInt32();

      return new RedirectContext
      {
        Client = client,
        Address = address,
        Port = port,
        Name = name,
      };
    }

    internal static void ToPacket(RedirectContext context, ServerPacket msg)
    {
      context.Client.Server.RemoteEndPoint = new IPEndPoint(new IPAddress(context.Address), context.Port);

      msg.BodyData[0] = (byte)1;
      msg.BodyData[1] = (byte)0;
      msg.BodyData[2] = (byte)0;
      msg.BodyData[3] = (byte)127;
      msg.BodyData[4] = (byte)10;
      msg.BodyData[5] = (byte)50;
    }
  }


}
