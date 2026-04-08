using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Inventory
{
  internal static class UseItemGameplay
  {
    public static void Handle(Client client, Item item)
    {
      string itemName = item.Name;


      switch (itemName)
      {
        case "Lucky Clover":
          client.ateclover = true;
          break;

        case "Golden Starfish":
          client.ategsf = true;
          break;

        case "Sprint Potion":
          item.NextUse = DateTime.UtcNow.AddMilliseconds(16000);
          break;

        case "Grime Scent":
          item.NextUse = DateTime.UtcNow.AddMilliseconds(11000);
          break;

        case "Damage Scroll":
          item.NextUse = DateTime.UtcNow.AddMilliseconds(31000);
          break;

        case "Experience Gem":
          item.NextUse = DateTime.UtcNow.AddMilliseconds(30000);
          break;

        case "Two Move Combo":
        case "Three Move Combo":
          if (!client.comboscrollused)
            ++client.comboscrolluse;
          if (client.comboscrolluse >= 2 && item.Name.Equals("Two Move Combo")      // Cooldown starts after 2 uses
            || client.comboscrolluse >= 3 && item.Name.Equals("Three Move Combo"))  // Cooldown starts after 3 uses
          {
            item.NextUse = DateTime.UtcNow.AddMilliseconds(121000);
            client.ComboScrollTimer.Start();
            client.comboscrolluse = 0;
            client.comboscrollused = true;
          }
          break;

        default:
          item.NextUse = DateTime.UtcNow.AddMilliseconds(325);
          break;
      }
    }
  }
}
