using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Skills
{
  public static class SkillSystem
  {
    public static bool HandleAssail(Client client, ClientPacket msg)
    {
      Logger.Debug($"[{client.Name}:HandleAssail] used Assail skill.");
      return true;
    }

    public static bool HandleUseSkill(Client client, ClientPacket msg)
    {
      var context = UseSkillContext.FromPacket(client, msg);
      Skill skill = context.ComboID;
      if (skill != null && client.Combos.Count() > 0 && client.Combos.ContainsKey(skill.Name))
      {
        QueueComboSkills(context);
        Logger.Debug($"[{client.Name}:HandleUseSkill] used skill {context.ComboID.Name} in slot {context.SkillSlot}.");
      }
      else
      {
        Logger.Debug($"[{client.Name}:HandleUseSkill] used skill in slot {context.SkillSlot}, but no combo found.");
      }

      return true;
    }

    private static void QueueComboSkills(UseSkillContext context)
    {
      Client client = context.Client;
      Skill skill = context.ComboID;
      Task.Run(() =>
      {
        string combo = client.Combos[skill.Name];
        foreach (string part in combo.Split('|'))
        {
          string skillName = part.Trim();
          if (skillName.Equals("space", StringComparison.CurrentCultureIgnoreCase)
            || skillName.Equals("assail", StringComparison.CurrentCultureIgnoreCase))
          {
            client.Assail();
          }
          else if (!client.UseSkill(skillName)
            && !client.UseMedSkill(skillName)
            && !client.UseItem(skillName))
          {
            client.Cast(skillName, null);
          }
        }
      });
    }
  }
}
