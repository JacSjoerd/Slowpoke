using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Slowpoke.Inventory
{
  public static class DropFilter
  {

    /// <summary>
    /// Determines whether the drop action is allowed based on the client's inventory and the specified drop location.
    /// </summary>
    /// <remarks>If the client possesses a "Warranty Bag" and attempts to drop a Succubus's Hair at the Mileth Altar
    /// the action is denied and a message is sent to the client. In all other cases, the drop is allowed.</remarks>
    /// <param name="context">The context containing information about the client and the drop location to evaluate.</param>
    /// <returns>true if the drop action is permitted; otherwise, false.</returns>
    public static bool ShouldAllow(DropContext context)
    {
      Client client = context.Client;
      var slot = context.Slot;
      var xLocation = context.DropLocationX;
      var yLocation = context.DropLocationY;

      if (client.Inventory[slot - 1].Name == "Succubus's Hair"
        && client.HasItem("Warranty Bag")
        && client.MapInfo.Name.Equals("Mileth Village")
        && xLocation == 31
        && (yLocation == 52 || yLocation == 53))
      {
        client.SendMessage("Deposit your Warranty Bag first.", "red");
        return false;
      }

      return true;
    }
  }
}
