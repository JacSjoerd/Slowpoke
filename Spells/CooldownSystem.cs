using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Spells
{
  public static class CooldownSystem
  {
    public static bool HandleCooldown(Client client, ServerPacket msg)
    {
      var context = CooldownContext.FromPacket(client, msg);

      int type = context.Type;
      int slot = context.Slot;
      int duration = context.Duration;

      client.ImCasting = false;

      if (type == 0)
        setSpellCooldown(client, slot, duration);
      else
        setSkillCooldown(client, slot, duration);

      Logger.Debug($"CD: [{context.Client.Name}] type: {type}, slot: {slot}, duration: {duration}");

      return true;
    }

    private static void setSpellCooldown(Client client, int slot, int duration)
    {
      if (slot > 0)
      {
        Spell spell = client.SpellBook[slot - 1];
        if (spell != null)
        {
          client.GlobalSpellCD = DateTime.UtcNow;
          if (duration == 0)
            spell.NextUse = DateTime.UtcNow.AddMilliseconds(335);
          else
            spell.NextUse = DateTime.UtcNow.AddMilliseconds(duration * 1000);
        }
      }
    }

    private static void setSkillCooldown(Client client, int slot, int duration)
    {
      if (slot > 0)
      {
        Skill skill = client.SkillBook[slot - 1];
        if (skill != null)
        {
          if (duration == 0)
            skill.NextUse = DateTime.UtcNow.AddMilliseconds(335);
          else
            skill.NextUse = DateTime.UtcNow.AddMilliseconds(duration * 1000);
        }
      }
    }
  }
}
