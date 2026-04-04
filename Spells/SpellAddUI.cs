using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flintstones;

namespace Slowpoke.Spells
{
  public static class SpellAddUI
  {
    public static void Handle(SpellAddContext context)
    {
      Client client = context.Client;
      string spellName = context.Name;
      int spellSlot = context.Slot;
      string slotString = context.Slot.ToString();

      int currentLevel = context.CurrentLevel;
      int maximumLevel = context.MaximumLevel;

      if (spellName == "nis" || spellName == "Learning Spell")
        return;

      var spellListView = client.Tab.MacroOptions.macrospellslistview;
      if (spellListView.Items.ContainsKey(spellName))
      {
        if (currentLevel < maximumLevel)
          spellListView.Items[spellName].SubItems[1].Text = currentLevel.ToString();

        if (currentLevel == maximumLevel)
          spellListView.Items[spellName].Remove();

        client.SaveMacroList();
      }

      var spellItemView = client.Tab.SkillSwap.spelltemlist;
      if (spellItemView.Items.ContainsKey(slotString))
      {
        if (spellItemView.Items[slotString].SubItems.Count > 1)
          spellItemView.Items[slotString].SubItems[1].Text = spellName;
        else
          spellItemView.Items[slotString].SubItems.Add(spellName);
      }
      
      var medSpellListView = client.Tab.SkillSwap.spellmedlist;
      if (spellSlot > 36) 
      {
        string medSlotString = (context.Slot - 36).ToString();
        if (medSpellListView.Items.ContainsKey(medSlotString))
        {
          if (medSpellListView.Items[medSlotString].SubItems.Count > 1)
            medSpellListView.Items[medSlotString].SubItems[1].Text = spellName;
          else
            medSpellListView.Items[medSlotString].SubItems.Add(spellName);
        }
      }
    }
  }
}
