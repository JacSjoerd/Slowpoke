using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flintstones;

namespace Slowpoke.Inventory
{
  public static class DropSystem
  {

    public static bool HandleDrop(Client client, ClientPacket msg)
    {
      var context = DropContext.FromPacket(client, msg);

      bool canDrop = DropFilter.ShouldAllow(context);

      return canDrop;
    }
  }
}
