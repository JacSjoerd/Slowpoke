using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Spells
{
  public static class SpellSystem
  {
    public static bool HandleUseSpell(Client client, ClientPacket msg)
    {
      var context = SpellContext.FromPacket(client, msg);

      if (context.Spell == null)
        return false;

      SpellState.UpdateLastSpell(context);
      SpellTargeting.Handle(context);
      SpellCasting.ResetState(context);

      return true;
    }
  }
}
