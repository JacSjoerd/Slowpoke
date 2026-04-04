using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flintstones;

namespace Slowpoke.Inventory
{
  public class DropContext
  {
    public Client Client;
    public int Slot;
    public int DropLocationX;
    public int DropLocationY;

    public static DropContext FromPacket(Client client, ClientPacket msg)
    {
      int slot = msg.ReadByte();
      int dropLocationX = msg.ReadUInt16();
      int dropLocationY = msg.ReadUInt16();
      msg.ReadUInt32(); // skip the unknown uint32

      return new DropContext
      {
        Client = client,
        Slot = slot,
        DropLocationX = dropLocationX,
        DropLocationY = dropLocationY,
      };
    }
  }
}
