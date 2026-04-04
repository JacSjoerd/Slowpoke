using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Spells
{
  public static class SpellState
  {
    public static void UpdateLastSpell(SpellContext ctx)
    {
      ctx.Client.LastSpell = ctx.Spell.Name;
    }
  }
}
