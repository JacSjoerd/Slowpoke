using Flintstones;
using Slowpoke.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Inventory
{
  internal class InventorySystem
  {

    public static bool HandleDrop(Client client, ClientPacket msg)
    {
      var context = DropContext.FromPacket(client, msg);

      bool canDrop = DropFilter.ShouldAllow(context);

      Logger.Debug($"Client {client.Name} attempted to drop item in slot {context.Slot}. Allowed: {canDrop}");

      return canDrop;
    }

    public static bool HandleUseItem(Client client, ClientPacket msg)
    {
      var context = UseItemContext.FromPacket(client, msg);

      Item item = client.Inventory[context.Slot - 1];
      if (item != null) 
        UseItemGameplay.Handle(client, item);

      Logger.Debug($"Client {client.Name} used item in slot {context.Slot}");

      return true;
    }

    public static bool HandleSwapSlot(Client client, ClientPacket msg) 
    { 
      var context = SwapSlotContext.FromPacket(client, msg);
      
      Logger.Debug($"Client {client.Name} swapped item type {context.Type} from slot {context.FromSlot} to slot {context.ToSlot}");

      return true;
    }

  }
}
