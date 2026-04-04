using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Walking
{
  public static class WalkingSystem
  {
    public static bool HandleWalking(Client client, ClientPacket msg)
    {
      var context = WalkingContext.FromPacket(client, msg);

      UpdateWalkingState(context);

      WalkingContext.ToPacket(client, msg);

      return true;
    }

    private static void UpdateWalkingState(WalkingContext context)
    {
      Client client = context.Client;

      switch (context.Direction) {
        case Direction.North:
          client.ClientLocation.Y--;
          break;
        case Direction.South:
          client.ClientLocation.Y++;
          break;
        case Direction.West:
          client.ClientLocation.X--;
          break;
        case Direction.East:
          client.ClientLocation.X++;
          break;
      }

      if (client.LastMapLocation.ContainsKey(client.MapInfo.Number))
        client.LastMapLocation[client.MapInfo.Number] = new Location(client.ClientLocation.X, client.ClientLocation.Y);
      else
        client.LastMapLocation.Add(client.MapInfo.Number, new Location(client.ClientLocation.X, client.ClientLocation.Y));

      client.laststep = DateTime.UtcNow;
    }
  }
}
