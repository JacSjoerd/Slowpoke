using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flintstones;

namespace Slowpoke.Inventory
{
  internal class SwapSlotContext
  {
    public Client Client;
    public int Type;
    public int FromSlot;
    public int ToSlot;

    public static SwapSlotContext FromPacket(Client client, ClientPacket msg)
    {
      int type = msg.ReadByte();
      int fromSlot = msg.ReadByte();
      int toSlot = msg.ReadByte();
      return new SwapSlotContext
      {
        Client = client,
        Type = type,
        FromSlot = fromSlot,
        ToSlot = toSlot,
      };
    }
  }
}
