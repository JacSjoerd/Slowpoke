using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flintstones;

namespace Slowpoke.Skills
{
  internal class UseSkillContext
  {
    internal Client Client;
    internal int SkillSlot;
    internal Skill ComboID;

    internal static UseSkillContext FromPacket(Client client, ClientPacket msg)
    {
      int slot = msg.ReadByte();

      Skill skill = null;
      foreach (Skill skill1 in client.FakeSkills.Values)
      {
        if (skill1 != null && slot == skill1.SkillSlot)
        {
          skill = skill1;
          break;
        }
      }

      if (skill == null) 
        Console.WriteLine($"[UseSkillContext] Skill not found for slot {slot}");

      return new UseSkillContext
      {
        Client = client,
        SkillSlot = slot,
        ComboID = skill,
      };
    }
  }
}
