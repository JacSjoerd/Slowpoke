using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flintstones;

namespace Slowpoke.Inventory
{
  public static class GearSystem
  {
    public static bool HandleUnequip(Client client, ClientPacket msg)
    {
      var context = UnequipContext.FromPacket(client, msg);

      client.manualremovedarmor = context.RemovedArmorManually;

      Logger.Debug($"Client {client.Name} unequipped item in slot {context.RemoveType}.");

      return true;
    }
  }
}
