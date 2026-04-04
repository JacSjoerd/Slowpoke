using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Spells
{
  public static class SpellCasting
  {
    public static void ResetState(SpellContext context)
    {
      var client = context.Client;

      client.ImCasting = false;
      client.castingoneline = false;
    }
  }
}
