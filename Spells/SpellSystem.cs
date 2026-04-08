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

      Spell spell = context.Spell;
      int bodyDataLength = context.BodyDataLength;

      if (spell != null)
        client.LastSpell = spell.Name;

      SpellTargeting.Handle(context);
      SpellCasting.ResetState(context);

      Logger.Debug($"[{client.Name}:HandleUseSpell]: Casts spell {spell.Name}.");

      return true;
    }

    public static bool HandleSpellLines(Client client, ClientPacket msg)
    {
      var context = SpellLineContext.FromPacket(client, msg);

      Logger.Debug($"[{client.Name}:HandleSpellLines]: Casts spell with {context.Lines} lines");

      if (context.Lines != 1)
        return true;

      QueueSpell(context);
      return false;
    }


    private static void QueueSpell(SpellLineContext context)
    {
      Client client = context.Client;

      client.mancastdelay = DateTime.UtcNow;

      if (client.Tab.halfcast.Checked)
      {
        client.StartCast(context.Lines);
      }
      else
      {
        Task.Run(() =>
        {
          client.StartCast(context.Lines);
        });
      }
    }
  }
}
