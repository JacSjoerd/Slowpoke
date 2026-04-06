using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Networking.Login
{
  internal class LogoutContext
  {
    public Client Client;
    public bool LoggingOff;

    public static LogoutContext FromPacket(Client client, ClientPacket msg)
    {
      bool loggingOff = (msg.ReadByte() == 0);

      return new LogoutContext
      {
        Client = client,
        LoggingOff = loggingOff,
      };
    }
  }
}
