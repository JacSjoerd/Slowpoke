using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Networking.Login
{
  internal class LoginContext
  {
    public Client Client;
    public string Name;
    public string Password;

    public static LoginContext FromPacket(Client client, ClientPacket msg)
    {
      string name = msg.ReadString((int)msg.ReadByte());
      string password = msg.ReadString((int)msg.ReadByte());


      return new LoginContext
      {
        Client = client,
        Name = name,
        Password = password,
      };
    }
  }
}
