using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Walking
{
  internal class LocationContext
  {
    internal Client Client;
    internal int X;
    internal int Y;
    internal static LocationContext FromPacket(Client client, ServerPacket msg)
    {
      return new LocationContext {
        Client = client,
        X = msg.ReadUInt16(),
        Y = msg.ReadUInt16()
      };
    }
  }
}
