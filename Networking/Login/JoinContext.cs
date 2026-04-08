using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flintstones;

namespace Slowpoke.Networking.Login
{
  internal class JoinContext
  {
    public Client Client;
    public byte Seed;
    public byte[] Key;
    public string Name;

    public static JoinContext FromPacket(Client client, ClientPacket msg)
    {

      byte seed = msg.ReadByte();
      byte[] key = msg.Read((int)msg.ReadByte());
      string name = msg.ReadString((int)msg.ReadByte());
      msg.ReadUInt32(); // skip 4 bytes

      return new JoinContext
      {
        Client = client,
        Seed = seed,
        Key = key,
        Name = name,
      };
    }
  }
}
