using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Networking.Login
{
  internal static class LoginSystem
  {
    public static bool HandleLogin(Client client, ClientPacket msg)
    {
      var context = LoginContext.FromPacket(client, msg);

      return true;
    }
  }
}
