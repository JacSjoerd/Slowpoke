using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Inventory
{
  internal class UnequipContext
  {
    internal Client Client;
    internal int RemoveType;
    internal bool RemovedArmorManually;
    internal static UnequipContext FromPacket(Client client, ClientPacket msg)
    {
      bool removedManually = false;
      int removeType = msg.ReadByte();

      if (removeType == (int)GearSlot.Armor)
        removedManually = true;

      var context = new UnequipContext
      {
        Client = client,
        RemoveType = removeType,
        RemovedArmorManually = removedManually
      };

      return context;
    }
  }
}
