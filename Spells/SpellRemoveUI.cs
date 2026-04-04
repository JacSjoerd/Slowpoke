using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flintstones;

namespace Slowpoke.Spells
{
  public class SpellRemoveUI
  {

    public static void Handle(SpellRemoveContext context)
    {
      var client = context.Client;
      var spellSlot = context.Slot;
      var slotString = context.Slot.ToString();

      var spellItemView = client.Tab.SkillSwap.spelltemlist;
      if (spellItemView.Items.ContainsKey(slotString))
      {
        if (spellItemView.Items[slotString].SubItems.Count > 1)
          spellItemView.Items[slotString].SubItems[1].Text = "";
      }

      var medSpellListView = client.Tab.SkillSwap.spellmedlist;
      if (spellSlot > 36)
      {
        string medSlotString = (context.Slot - 36).ToString();

        if (medSpellListView.Items.ContainsKey(medSlotString))
        {
          if (medSpellListView.Items[medSlotString].SubItems.Count > 1)
            medSpellListView.Items[medSlotString].SubItems[1].Text = "";
        }
      }
    }
  }
}
