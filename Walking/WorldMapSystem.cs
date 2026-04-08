using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flintstones;  

namespace Slowpoke.Walking
{
  public static class WorldMapSystem
  {

    public static bool HandleWorldMap(Client client, ClientPacket msg)
    {
      client.Towns.Clear();

      Logger.Debug($"Client {client.Name} opened world map.");
      return true;
    }
  }
}
