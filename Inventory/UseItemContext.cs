using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flintstones;

namespace Slowpoke.Inventory
{
  internal class UseItemContext
  {
    public Client Client;
    public int Slot;

    public static UseItemContext FromPacket(Client client, ClientPacket msg)
    {
      int slot = msg.ReadByte();

      return new UseItemContext
      {
        Client = client,
        Slot = slot
      }; ;
    }
  }
}
