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

      Logger.Debug($"[{client.Name}:HandleWalking] Walking {context.Direction.ToString()} to ({client.ClientLocation.X}, {client.ClientLocation.Y})");

      return true;
    }

    public static bool HandleLocation(Client client, ServerPacket msg)
    {
      var context = LocationContext.FromPacket(client, msg);

      client.ServerLocation.X = context.X;
      client.ServerLocation.Y = context.Y;
      client.checkedtiles.Add($"{context.X},{context.Y}");

      Logger.Debug($"[{client.Name}:HandleLocation] location updated to ({context.X}, {context.Y})");

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
