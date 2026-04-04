using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slowpoke.Spells;

namespace Flintstones
{
  public static class SpellBookSystem
  {
    public static bool HandleAddSpell(Client client, ServerPacket msg)
    {
      var context = SpellAddContext.FromPacket(client, msg);

      AddSpell(context);
      SpellAddUI.Handle(context);
      Logger.Debug($"Spell added: {context.Name}");

      return true;
    }

    public static bool HandleRemoveSpell(Client client, ServerPacket msg)
    {
      var context = SpellRemoveContext.FromPacket(client, msg);

      RemoveSpell(context);
      SpellRemoveUI.Handle(context);
      Logger.Debug($"Spell removed from slot: {context.Slot}");

      return true;
    }

    public static void AddSpell(SpellAddContext context)
    {
      var client = context.Client;
      var spell = new Spell
      {
        Name = context.Name,
        CastLines = context.CastLines,
        SpellSlot = context.Slot,
        Captions = context.Captions,
        NextUse = DateTime.UtcNow,
        CurrentLevel = context.CurrentLevel,
        MaximumLevel = context.MaximumLevel,
        Prompt = context.Prompt,
        Type = context.Type,
        Icon = context.Icon,
      };

      client.SpellBook[context.Slot - 1] = spell;

      if (spell.Name.Contains("Prayer"))
        client.PrayerSpell = spell.Name;
    }

    public static void RemoveSpell(SpellRemoveContext context)
    {
      var client = context.Client;

      client.SpellBook[context.Slot - 1] = null;
    }
  }
}
