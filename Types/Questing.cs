using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flintstones
{
  internal class Questing
  {
    private Client _client;

    private Dictionary<string, Dictionary<DugonColor, Action>> dugonMeditations;

    public Questing(Client client)
    {
      _client = client;
    }

    public void Run(CancellationToken token)
    {
      while (!token.IsCancellationRequested)
      {
        try
        {
          if (!_client.pause)
          {
            if (_client.needsrepaired && _client.Currentnpctext == "")
            {
              Npc[] npcArray1 = _client.NearbyNpcs(Npc.NpcType.Mundane);
              if (npcArray1 != null)
              {
                Npc[] npcArray2 = npcArray1;
                int index = 0;
                if (index < npcArray2.Length)
                {
                  Npc npc = npcArray2[index];
                  _client.repairmode = true;
                  _client.goldbefore = _client.Statistics.Gold;
                  _client.DialogueRespond(new uint?(npc.ID), "Fix All");
                  _client.needsrepaired = false;
                }
              }
            }
            _client.InsectEventAssail();
            if (!_client.pausecast && !_client.autowalkon)
            {
              if ((_client.Tab.useskillshidden.Checked ? (_client.SpellBar.Contains((ushort)10) ? 1 : 0) : (!_client.SpellBar.Contains((ushort)10) ? 1 : 0)) != 0)
              {
                _client.SetMainTarget();
                if (_client.MainTarget != null && (!_client.Tab.asrs.Checked || !_client.asready && !_client.rsready || (_client.IgnoreHP(_client.MainTarget.Image) ? (_client.MainTarget.HpAmount <= 20.0 ? 1 : 0) : 0) != 0 || !_client.MainTarget.Lured || _client.HasInfiniteMR(_client.MainTarget.Image)))
                {
                  _client.ParalyzeForce();
                  if (!_client.Tab.equipweapon.Checked || _client.BestWeapon() == string.Empty || _client.staffnow.StartsWith("Staff of "))
                    _client.UseSkills();
                }
              }
              if (_client.Tab.throwtotems.Checked && (int)_client.ClientForm - 16384 > 0 && _client.MonsterInFront() != null && !_client.SpellBar.Contains((ushort)10) && _client.CanSkill("Tail Slam"))
                _client.UseSkill("Tail Slam");
            }
            if (_client.Tab.requestaite.Checked && _client.askaite && !_client.SpellBar.Contains((ushort)11))
            {
              if (_client.appendand)
                _client.Speak("and " + _client.Tab.requesttextaite.Text);
              else
                _client.Speak(_client.Tab.requesttextaite.Text);
              _client.SpeakAiteTimer.Start();
              _client.askaite = false;
              _client.appendand = true;
            }
            if (_client.Tab.requestfas.Checked && _client.askfas && !_client.SpellBar.Contains((ushort)119))
            {
              if (_client.appendand)
                _client.Speak("and " + _client.Tab.requesttextfas.Text);
              else
                _client.Speak(_client.Tab.requesttextfas.Text);
              _client.SpeakFasTimer.Start();
              _client.askfas = false;
              _client.appendand = true;
            }
            if (_client.Tab.requestred.Checked && _client.askred && (_client.IsSkulled || _client.Statistics.CurrentHP == 0U))
            {
              if (_client.appendand)
                _client.Speak("and " + _client.Tab.requesttextred.Text);
              else
                _client.Speak(_client.Tab.requesttextred.Text);
              _client.SpeakRedTimer.Start();
              _client.askred = false;
              _client.appendand = true;
            }
            if (_client.Tab.requestflower.Checked && _client.askflower && (Decimal)_client.Statistics.CurrentMP < _client.Tab.requestflowercond.Value)
            {
              if (_client.appendand)
                _client.Speak("and " + _client.Tab.requesttextflower.Text);
              else
                _client.Speak(_client.Tab.requesttextflower.Text);
              _client.SpeakFlowerTimer.Start();
              _client.askflower = false;
              _client.appendand = true;
            }
            if (_client.staffnow == "Fishing Rod" && _client.HasItem("Fishing Bait"))
            {
              foreach (Npc npc in (IEnumerable<Npc>)_client.NearbyNpcs(Npc.NpcType.Mundane).OrderBy(n => n.DistanceFrom(_client.ServerLocation)))
              {
                if (npc != null && npc.Image == 583 && npc.DistanceFrom(_client.ServerLocation) <= 5)
                {
                  _client.ClickEntity(npc.ID);
                  _client.ClickEntity(npc.ID);
                  Thread.Sleep(100);
                }
              }
              if (_client.MapInfo.IsLoaded)
              {
                Point[,] tiles = _client.MapInfo.Tiles;
                int upperBound1 = tiles.GetUpperBound(0);
                int upperBound2 = tiles.GetUpperBound(1);
                for (int lowerBound1 = tiles.GetLowerBound(0); lowerBound1 <= upperBound1; ++lowerBound1)
                {
                  for (int lowerBound2 = tiles.GetLowerBound(1); lowerBound2 <= upperBound2; ++lowerBound2)
                  {
                    Point point = tiles[lowerBound1, lowerBound2];
                    if (point != null && point.DistanceFrom(_client.ServerLocation) <= 5)
                    {
                      _client.FakeSpellAni(point.X, point.Y, 362);
                      Thread.Sleep(10);
                    }
                  }
                }
              }
            }
          }
          if (_client.portalani == DateTime.MinValue || DateTime.UtcNow.Subtract(_client.portalani).TotalMilliseconds > 500.0)
          {
            foreach (RootNpc rootNpc in Server.gamenpcs.Values)
            {
              if (int.Parse(rootNpc.mapnum) == _client.MapInfo.Number)
                _client.FakeSpellAni(int.Parse(rootNpc.x), int.Parse(rootNpc.y), 231);
            }
            if (Server.gamemaps.ContainsKey(_client.MapInfo.Number) && int.Parse(Server.gamemaps[_client.MapInfo.Number].mapnum) == _client.MapInfo.Number)
            {
              foreach (To to in Server.gamemaps[_client.MapInfo.Number].to)
              {
                string str = to.ports.Substring(0, to.ports.IndexOf('>'));
                string s1 = str.Substring(0, str.IndexOf(','));
                string s2 = str.Substring(str.IndexOf(',') + 1);
                _client.FakeSpellAni(int.Parse(s1), int.Parse(s2), 19);
              }
            }
            _client.portalani = DateTime.UtcNow;
          }
          if (_client.Tab.studycreaturetxt.Checked)
          {
            foreach (Npc nearbyNormalMonster in _client.NearbyNormalMonsters())
            {
              if (nearbyNormalMonster != null && nearbyNormalMonster.IsOnScreen && nearbyNormalMonster.sensed && (nearbyNormalMonster.senseanimationdelay == DateTime.MinValue || DateTime.UtcNow.Subtract(nearbyNormalMonster.senseanimationdelay).TotalMilliseconds > 500.0))
              {
                _client.FakeSpellAniTarget(nearbyNormalMonster.ID, 0U, (ushort)19, (ushort)0);
                nearbyNormalMonster.senseanimationdelay = DateTime.UtcNow;
              }
            }
          }
          if (!_client.safemode && (_client.Tab.vmonitorspells || _client.Tab.vmonitorcurses || _client.Tab.vmonitordion))
          {
            foreach (Player player in _client.NearbyPlayer())
            {
              if (player != null && player != null && (int)player.ID != (int)_client.PlayerID && Server.StaticCharacters.ContainsKey(player.ID))
              {
                if (_client.Tab.vmonitordion && Server.StaticCharacters[player.ID].hasdion && player.DisplayName.Contains(")") && int.Parse(player.DisplayName.Substring(1, player.DisplayName.IndexOf(")") - 1)) != 20 - (int)DateTime.UtcNow.Subtract(Server.StaticCharacters[player.ID].SpellAnimationHistory[244]).TotalSeconds)
                  _client.UpdatePlayerImage(player);
                else if (_client.Tab.vmonitordion && Server.StaticCharacters[player.ID].hasironskin && player.DisplayName.Contains(")") && int.Parse(player.DisplayName.Substring(1, player.DisplayName.IndexOf(")") - 1)) != 19 - (int)DateTime.UtcNow.Subtract(Server.StaticCharacters[player.ID].SpellAnimationHistory[89]).TotalSeconds)
                  _client.UpdatePlayerImage(player);
                else if (_client.Tab.vmonitordion && Server.StaticCharacters[player.ID].hasdioncomlha && player.DisplayName.Contains(")") && int.Parse(player.DisplayName.Substring(1, player.DisplayName.IndexOf(")") - 1)) != 20 - (int)DateTime.UtcNow.Subtract(Server.StaticCharacters[player.ID].SpellAnimationHistory[93]).TotalSeconds)
                  _client.UpdatePlayerImage(player);
                else if (_client.Tab.vmonitordion && Server.StaticCharacters[player.ID].haswingsofprot && player.DisplayName.Contains(")") && int.Parse(player.DisplayName.Substring(1, player.DisplayName.IndexOf(")") - 1)) != 14 - (int)DateTime.UtcNow.Subtract(Server.StaticCharacters[player.ID].SpellAnimationHistory[86]).TotalSeconds)
                  _client.UpdatePlayerImage(player);
                else if (_client.Tab.vmonitordion && Server.StaticCharacters[player.ID].hasasgall && player.DisplayName.Contains(")") && int.Parse(player.DisplayName.Substring(1, player.DisplayName.IndexOf(")") - 1)) != 13 - (int)DateTime.UtcNow.Subtract(Server.StaticCharacters[player.ID].SpellAnimationHistory[66]).TotalSeconds)
                  _client.UpdatePlayerImage(player);
                else if (_client.Tab.vmonitordion && player.DisplayName.Contains(")") && !Server.StaticCharacters[player.ID].hasasgall && !Server.StaticCharacters[player.ID].hasdion && !Server.StaticCharacters[player.ID].hasironskin && !Server.StaticCharacters[player.ID].hasdioncomlha && !Server.StaticCharacters[player.ID].haswingsofprot)
                  _client.UpdatePlayerImage(player);
                else if (_client.Tab.vmonitorspells && (player.DisplayName.Contains("[fas]") || player.DisplayName.Contains("[aite/fas]")) && !Server.StaticCharacters[player.ID].hasfas)
                  _client.UpdatePlayerImage(player);
                else if (_client.Tab.vmonitorspells && (player.DisplayName.Contains("[aite]") || player.DisplayName.Contains("[aite/fas]")) && !Server.StaticCharacters[player.ID].hasaite)
                  _client.UpdatePlayerImage(player);
                else if (_client.Tab.monitords.Checked && Server.StaticCharacters[player.ID].NameIsRed && !Server.StaticCharacters[player.ID].hasdemonseal && !Server.StaticCharacters[player.ID].hasdemise && !Server.StaticCharacters[player.ID].hasdarkerseal && !Server.StaticCharacters[player.ID].hasdarkseal)
                  _client.UpdatePlayerImage(player);
                else if (_client.Tab.vmonitorcurses && Server.StaticCharacters[player.ID].NameIsRed && !Server.StaticCharacters[player.ID].hasardcradh)
                  _client.UpdatePlayerImage(player);
              }
            }
          }
          if (!_client.safemode)
          {
            foreach (Npc allNearbyMonster in _client.AllNearbyMonsters())
            {
              if (allNearbyMonster != null && Server.StaticCharacters.ContainsKey(allNearbyMonster.ID))
              {
                if (!_client.MapInfo.Name.Contains("Andor") && Server.StaticCharacters[allNearbyMonster.ID].hasmonsterdion)
                {
                  int diontime = allNearbyMonster.diontime;
                  TimeSpan timeSpan = DateTime.UtcNow.Subtract(Server.StaticCharacters[allNearbyMonster.ID].SpellAnimationHistory[271]);
                  int num1 = 20 - (int)timeSpan.TotalSeconds;
                  if (diontime != num1)
                  {
                    Npc npc = allNearbyMonster;
                    timeSpan = DateTime.UtcNow.Subtract(Server.StaticCharacters[allNearbyMonster.ID].SpellAnimationHistory[271]);
                    int num2 = 20 - (int)timeSpan.TotalSeconds;
                    npc.diontime = num2;
                    _client.FakeChat("(" + allNearbyMonster.diontime.ToString() + ")", allNearbyMonster.ID);
                    continue;
                  }
                }
                if (!_client.MapInfo.Name.Contains("Andor") && Server.StaticCharacters[allNearbyMonster.ID].hasdion)
                {
                  int diontime = allNearbyMonster.diontime;
                  TimeSpan timeSpan = DateTime.UtcNow.Subtract(Server.StaticCharacters[allNearbyMonster.ID].SpellAnimationHistory[244]);
                  int num3 = 20 - (int)timeSpan.TotalSeconds;
                  if (diontime != num3)
                  {
                    Npc npc = allNearbyMonster;
                    timeSpan = DateTime.UtcNow.Subtract(Server.StaticCharacters[allNearbyMonster.ID].SpellAnimationHistory[244]);
                    int num4 = 20 - (int)timeSpan.TotalSeconds;
                    npc.diontime = num4;
                    _client.FakeChat("(" + allNearbyMonster.diontime.ToString() + ")", allNearbyMonster.ID);
                  }
                }
              }
            }
          }
          if (_client.Tab.uncheckloot.Checked && _client.Tab.looton.Checked && !_client.SafeToWalkFast)
          {
            _client.Tab.looton.Checked = false;
            _client.loot = false;
            _client.SendMessage("Loot option was unchecked", "red");
          }
          if (!_client.pause)
          {
            if (_client.loot && _client.lootbefore && !_client.castingoneline)
              _client.LootItems();
            if (_client.Tab.vdropitemson && _client.dropbefore && _client.Tab.dropitemslist.Items.Count > 0)
            {
              foreach (object obj in _client.Tab.dropitemslist.Items)
              {
                if (obj != null && obj.ToString() != string.Empty)
                {
                  if (_client.HasItem(obj.ToString()))
                    _client.DropItems(obj.ToString());
                  if (obj.ToString().ToLower() == "count")
                  {
                    if (_client.HasItem("Abomination Mask"))
                      _client.DropItems("Abomination Mask");
                    if (_client.HasItem("Spectre Mask"))
                      _client.DropItems("Spectre Mask");
                    if (_client.HasItem("Fiend Mask"))
                      _client.DropItems("Fiend Mask");
                    if (_client.HasItem("Dubhaim Helm"))
                      _client.DropItems("Dubhaim Helm");
                    if (_client.HasItem("Undead Hand"))
                      _client.DropItems("Undead Hand");
                    if (_client.HasItem("Swamp Witch Pet"))
                      _client.DropItems("Swamp Witch Pet");
                    if (_client.HasItem("Pumpkin Cap"))
                      _client.DropItems("Pumpkin Cap");
                    if (_client.HasItem("Pumpkin Slippers"))
                      _client.DropItems("Pumpkin Slippers");
                    if (_client.HasItem("Pumpkin Costume"))
                      _client.DropItems("Pumpkin Costume");
                    if (_client.HasItem("Macabre Shoes"))
                      _client.DropItems("Macabre Shoes");
                    if (_client.HasItem("Macabre Hexed Hat"))
                      _client.DropItems("Macabre Hexed Hat");
                    if (_client.HasItem("Macabre Hexed Robes"))
                      _client.DropItems("Macabre Hexed Robes");
                    if (_client.HasItem("Macabre Hexed Dress"))
                      _client.DropItems("Macabre Hexed Dress");
                    if (_client.HasItem("Macabre Battle Armor"))
                      _client.DropItems("Macabre Battle Armor");
                    if (_client.HasItem("Macabre Battle Helm"))
                      _client.DropItems("Macabre Battle Helm");
                    if (_client.HasItem("Macabre Shadow Cloak"))
                      _client.DropItems("Macabre Shadow Cloak");
                    if (_client.HasItem("Macabre Shadow Sheath"))
                      _client.DropItems("Macabre Shadow Sheath");
                    if (_client.HasItem("Blue Shadow Hair"))
                      _client.DropItems("Blue Shadow Hair");
                    if (_client.HasItem("Red Shadow Hair"))
                      _client.DropItems("Red Shadow Hair");
                    if (_client.HasItem("Macabre Virtue Coat"))
                      _client.DropItems("Macabre Virtue Coat");
                    if (_client.HasItem("Macabre Virtue Blouse"))
                      _client.DropItems("Macabre Virtue Blouse");
                    if (_client.HasItem("Virtue Hood"))
                      _client.DropItems("Virtue Hood");
                    if (_client.HasItem("Virtue Cap"))
                      _client.DropItems("Virtue Cap");
                    if (_client.HasItem("Macabre Divine Robe"))
                      _client.DropItems("Macabre Divine Robe");
                    if (_client.HasItem("Macabre Divine Gown"))
                      _client.DropItems("Macabre Divine Gown");
                    if (_client.HasItem("Macabre Holy Hat"))
                      _client.DropItems("Macabre Holy Hat");
                    if (_client.HasItem("Macabre Bewitched Hat"))
                      _client.DropItems("Macabre Bewitched Hat");
                  }
                }
              }
            }
            if (_client.Tab.iditems.Checked || _client.Tab.recorditemdata.Checked)
            {
              if (!_client.waitingonlore && !_client.swappingitem && _client.firstitemslot != "" && _client.ItemSlot(_client.firstitemslot) != 1 && _client.SlotHasItem(1))
              {
                foreach (Item obj in _client.Inventory)
                {
                  if (obj != null && obj.InventorySlot == 1)
                  {
                    if (!_client.NeedsIdentified(obj))
                    {
                      _client.swappingitem = true;
                      _client.SwitchSlots((byte)0, _client.ItemSlot(_client.firstitemslot), 1);
                      break;
                    }
                    break;
                  }
                }
              }
              if (_client.CanSkill("Analyze Item") || _client.CanSkill("Perish Lore") || _client.CanSkill("Wise Touch") || _client.CanSkill("Evaluate Item") || _client.CanSkill("Appraise") || _client.CanSkill("Armor Lore"))
                _client.waitingonlore = false;
              if (!_client.waitingonlore && !_client.swappingitem && (_client.CanSkill("Analyze Item") || _client.CanSkill("Perish Lore") || _client.CanSkill("Wise Touch") || _client.CanSkill("Evaluate Item") || _client.CanSkill("Appraise") || _client.CanSkill("Armor Lore")))
              {
                foreach (Item obj in _client.Inventory)
                {
                  if (obj != null && !_client.waitingonlore && !_client.swappingitem && (_client.CanSkill("Analyze Item") || _client.CanSkill("Perish Lore") || _client.CanSkill("Wise Touch") || _client.CanSkill("Evaluate Item") && !_client.IsArmor(obj.Name) || _client.CanSkill("Appraise") && _client.IsGem(obj.Name) || _client.CanSkill("Armor Lore") && _client.IsArmor(obj.Name)))
                  {
                    if (_client.NeedsIdentified(obj))
                    {
                      if (obj.InventorySlot == 1)
                      {
                        if (_client.CanSkill("Analyze Item"))
                        {
                          _client.waitingonlore = true;
                          _client.UseSkill("Analyze Item");
                        }
                        else if (_client.CanSkill("Perish Lore"))
                        {
                          _client.waitingonlore = true;
                          _client.UseSkill("Perish Lore");
                        }
                        else if (_client.CanSkill("Wise Touch"))
                        {
                          _client.waitingonlore = true;
                          _client.UseSkill("Wise Touch");
                        }
                        else if (_client.CanSkill("Evaluate Item") && !_client.IsArmor(obj.Name))
                        {
                          _client.waitingonlore = true;
                          _client.UseSkill("Evaluate Item");
                        }
                        else if (_client.CanSkill("Armor Lore") && _client.IsArmor(obj.Name))
                        {
                          _client.waitingonlore = true;
                          _client.UseSkill("Armor Lore");
                        }
                        else if (_client.CanSkill("Appraise") && _client.IsGem(obj.Name))
                        {
                          _client.waitingonlore = true;
                          _client.UseSkill("Appraise");
                        }
                      }
                      else
                      {
                        if (_client.firstitemslot != "" && _client.ItemSlot(_client.firstitemslot) == 1)
                        {
                          _client.swappingitem = true;
                          _client.SwitchSlots((byte)0, obj.InventorySlot, 1);
                          break;
                        }
                        if (_client.firstitemslot == "")
                          _client.SendMessage("firstitemslot is blank, manually move an item");
                      }
                    }
                    if (_client.firstitemslot != "" && !_client.SlotHasItem(1))
                    {
                      _client.swappingitem = true;
                      _client.SwitchSlots((byte)0, _client.ItemSlot(_client.firstitemslot), 1);
                      break;
                    }
                  }
                }
              }
            }
            if (_client.autodeposit && _client.Tab.autodepositlistbox.Items.Count > 0)
            {
              foreach (object obj in _client.Tab.autodepositlistbox.Items)
              {
                if (obj != null && obj.ToString() != string.Empty && _client.HasItem(obj.ToString()))
                  _client.AutoDeposit(obj.ToString());
              }
            }
            if (_client.Tab.trashorbs.Checked && _client.MapInfo.Number == 6138 && _client.HasItem("Demon Orb"))
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Table");
              if (npcByName != null && npcByName.IsOnScreen && npcByName.DistanceFrom(_client.ServerLocation) < 3)
              {
                _client.DropInMonster(npcByName.ID, _client.ItemSlot("Demon Orb"), 1);
                Thread.Sleep(800);
              }
            }
            if (_client.Tab.tradeincostumes.Checked && (_client.MapInfo.Name == "Mileth Storage" || _client.MapInfo.Name == "Abel Storage" || _client.MapInfo.Name == "Rucesion Storage") && !_client.autowalkon)
            {
              if (_client.Overhat == "Monkey Head" || _client.Overhat == "Dino Head" || _client.Overhat == "Panda Head" || _client.Overhat == "Lizard Head" || _client.Overhat == "Bunny Head" || _client.Overhat == "Yeti Head" || _client.Overhat == "Cat Head" || _client.Overhat == "Dog Head" || _client.Overhat == "Sheep Head")
              {
                Npc npcByName = _client.FindNpcByName<Npc>("Cassidy");
                if (_client.MapInfo.Name == "Abel Storage")
                  npcByName = _client.FindNpcByName<Npc>("Lamont");
                else if (_client.MapInfo.Name == "Rucesion Storage")
                  npcByName = _client.FindNpcByName<Npc>("Antonio");
                if (npcByName != null)
                {
                  _client.DialogueRespond(npcByName.ID, (byte)10, (byte)11);
                  _client.PopupNext(new uint?(npcByName.ID));
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
                  _client.PopupNext(new uint?(npcByName.ID));
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
                  _client.PopupClose(new uint?(npcByName.ID));
                  Thread.Sleep(1000);
                }
              }
              else if (_client.Overcoat == "Male Beach Attire" || _client.Overcoat == "Female Beach Attire" || _client.Overcoat == "Monkey Body" || _client.Overcoat == "Shredded Cape" || _client.Overcoat == "Caveman" || _client.Overcoat == "Dino Body" || _client.Overcoat == "Panda Body" || _client.Overcoat == "Lizard Body" || _client.Overcoat == "Bunny Body" || _client.Overcoat == "Yeti Body" || _client.Overcoat == "Cat Body" || _client.Overcoat == "Dog Body" || _client.Overcoat == "Sheep Body")
              {
                Npc npcByName = _client.FindNpcByName<Npc>("Cassidy");
                if (_client.MapInfo.Name == "Abel Storage")
                  npcByName = _client.FindNpcByName<Npc>("Lamont");
                else if (_client.MapInfo.Name == "Rucesion Storage")
                  npcByName = _client.FindNpcByName<Npc>("Antonio");
                if (npcByName != null)
                {
                  _client.DialogueRespond(npcByName.ID, (byte)10, (byte)11);
                  _client.PopupNext(new uint?(npcByName.ID));
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)2);
                  _client.PopupNext(new uint?(npcByName.ID));
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
                  _client.PopupClose(new uint?(npcByName.ID));
                  Thread.Sleep(1000);
                }
              }
              else
              {
                if (_client.HasItem("Male Beach Attire"))
                {
                  _client.UseItem("Male Beach Attire");
                  Thread.Sleep(1000);
                }
                if (_client.HasItem("Female Beach Attire"))
                {
                  _client.UseItem("Female Beach Attire");
                  Thread.Sleep(1000);
                }
                if (_client.HasItem("Monkey Head"))
                {
                  _client.UseItem("Monkey Head");
                  Thread.Sleep(1000);
                }
                else if (_client.HasItem("Cat Head"))
                {
                  _client.UseItem("Cat Head");
                  Thread.Sleep(1000);
                }
                else if (_client.HasItem("Lizard Head"))
                {
                  _client.UseItem("Lizard Head");
                  Thread.Sleep(1000);
                }
                else if (_client.HasItem("Bunny Head"))
                {
                  _client.UseItem("Bunny Head");
                  Thread.Sleep(1000);
                }
                else if (_client.HasItem("Dog Head"))
                {
                  _client.UseItem("Dog Head");
                  Thread.Sleep(1000);
                }
                else if (_client.HasItem("Dino Head"))
                {
                  _client.UseItem("Dino Head");
                  Thread.Sleep(1000);
                }
                else if (_client.HasItem("Panda Head"))
                {
                  _client.UseItem("Panda Head");
                  Thread.Sleep(1000);
                }
                else if (_client.HasItem("Sheep Head"))
                {
                  _client.UseItem("Sheep Head");
                  Thread.Sleep(1000);
                }
                else if (_client.HasItem("Yeti Head"))
                {
                  _client.UseItem("Yeti Head");
                  Thread.Sleep(1000);
                }
                if (_client.HasItem("Monkey Body"))
                {
                  _client.UseItem("Monkey Body");
                  Thread.Sleep(1000);
                }
                else if (_client.HasItem("Cat Body"))
                {
                  _client.UseItem("Cat Body");
                  Thread.Sleep(1000);
                }
                else if (_client.HasItem("Lizard Body"))
                {
                  _client.UseItem("Lizard Body");
                  Thread.Sleep(1000);
                }
                else if (_client.HasItem("Bunny Body"))
                {
                  _client.UseItem("Bunny Body");
                  Thread.Sleep(1000);
                }
                else if (_client.HasItem("Dog Body"))
                {
                  _client.UseItem("Dog Body");
                  Thread.Sleep(1000);
                }
                else if (_client.HasItem("Dino Body"))
                {
                  _client.UseItem("Dino Body");
                  Thread.Sleep(1000);
                }
                else if (_client.HasItem("Panda Body"))
                {
                  _client.UseItem("Panda Body");
                  Thread.Sleep(1000);
                }
                else if (_client.HasItem("Sheep Body"))
                {
                  _client.UseItem("Sheep Body");
                  Thread.Sleep(1000);
                }
                else if (_client.HasItem("Yeti Body"))
                {
                  _client.UseItem("Yeti Body");
                  Thread.Sleep(1000);
                }
                else if (_client.HasItem("Caveman"))
                {
                  _client.UseItem("Caveman");
                  Thread.Sleep(1000);
                }
                else if (_client.HasItem("Shredded Cape"))
                {
                  _client.UseItem("Shredded Cape");
                  Thread.Sleep(1000);
                }
              }
            }
          }
          if (_client.throwss)
          {
            _client.throwss = false;
            _client.SendMessage(_client.throwername + " threw me.");
            _client.throwername = string.Empty;
          }
          if (!_client.pause && (!_client.Tab.dojo.Checked || _client.MapInfo.Name.Contains("Training Dojo")) && !_client.autowalkon)
          {
            if (_client.Tab.MacroOptions.macroskill.Checked)
            {
              if (_client.MainTarget != null && _client.MainTarget != _client.MonsterInFront() && _client.MainTarget.DistanceFrom(_client.ServerLocation) == 1 && !_client.ImFacingMonster && !_client.ImFacingAnything)
                _client.FaceTarget(_client.MainTarget.Location);
              foreach (Skill skill in _client.SkillBook)
              {
                if (skill != null && _client.Tab.MacroOptions.macroskillslistview.Items.ContainsKey(skill.Name) && _client.Tab.MacroOptions.macroskillslistview.Items[skill.Name].Checked)
                  _client.UseSkill(skill.Name);
              }
            }
            if (_client.Tab.MacroOptions.macropoisoncrasher.Checked && (_client.CanSkill("Crasher") || _client.CanSkill("Execute") || _client.CanSkill("Animal Feast")) && (double)_client.Statistics.CurrentHP / (double)_client.Statistics.MaximumHP * 100.0 <= 2.0)
            {
              _client.UseSkill("Crasher");
              _client.UseSkill("Execute");
              if (_client.HasItem("Damage Scroll"))
                _client.UseItem("Damage Scroll");
              _client.UseSkill("Animal Feast");
            }
            else if (_client.Tab.MacroOptions.macrohemcrasher.Checked && (_client.CanSkill("Crasher") || _client.CanSkill("Execute") || _client.CanSkill("Animal Feast")) && (double)_client.Statistics.CurrentHP / (double)_client.Statistics.MaximumHP * 100.0 > 2.0)
            {
              if (_client.HasItem("Hemloch"))
              {
                _client.UseItem("Hemloch");
                _client.UseSkill("Crasher");
                _client.UseSkill("Execute");
                _client.UseSkill("Animal Feast");
                Thread.Sleep(1000);
              }
              else if (_client.CanSkill("Auto Hemloch"))
              {
                _client.UseSkill("Auto Hemloch");
                _client.UseSkill("Crasher");
                _client.UseSkill("Execute");
                _client.UseSkill("Animal Feast");
                Thread.Sleep(1000);
              }
            }
            if (_client.Tab.MacroOptions.macropoisoncrasher.Checked && !_client.SpellBar.Contains((ushort)35) && !_client.SpellBar.Contains((ushort)1))
              _client.SkillSpellCaption("Poison");
          }
          if (_client.Statistics.CurrentHP != 0U && !_client.IsSkulled && _client.Tab.MacroOptions.macromend.Checked && _client.FirstItemHasNoDurability)
          {
            foreach (Skill skill in _client.SkillBook)
            {
              if (skill != null && skill.CurrentLevel < skill.MaximumLevel && skill.MaximumLevel != 0 && (skill.Name == "Lucky Hand" || skill.Name.Contains("Mend") || skill.Name == "Tailoring"))
                _client.UseSkill(skill.Name);
            }
          }
          if (!_client.pause && _client.Tab.dojo.Checked)
          {
            if (_client.MapInfo.Name.StartsWith("Training Dojo"))
            {
              if (_client.dojowalk && _client.Tab.autowalker_button.Text == "Stop")
              {
                _client.Tab.autowalker_button.Text = "Start";
                _client.autowalkon = false;
              }
              if (_client.dojowalk && _client.SurroundedCount != 4 && _client.MainTarget != null && _client.MainTarget.IsOnScreen)
                _client._walkCommands.WalkToTarget();
              if (_client.ImFacingMonster && _client.MonsterInFront() != null)
                _client.dojowalk = false;
            }
            else if (_client.MapInfo.Number == 3071)
            {
              if (_client.Statistics.Gold < 25000U)
              {
                _client.pause = true;
                _client.Tab.btnPlay.Enabled = true;
                _client.Tab.btnStop.Enabled = false;
                _client.SendMessage("Stopped because you're a broke ass, get some gold");
              }
              if (_client.Tab.autowalker_button.Text == "Stop")
              {
                _client.Tab.autowalker_button.Text = "Start";
                _client.autowalkon = false;
              }
              _client.dojowalk = true;
              Npc npcByName = _client.FindNpcByName<Npc>("Niomope");
              if (npcByName != null && npcByName.IsOnScreen)
              {
                if (!_client.niomope)
                {
                  if (_client.dojodelay == DateTime.MinValue)
                    _client.dojodelay = DateTime.UtcNow;
                  if (DateTime.UtcNow.Subtract(_client.dojodelay).TotalMilliseconds > 5000.0)
                  {
                    _client.dojodelay = DateTime.MinValue;
                    _client.DialogueRespond(npcByName.ID, (byte)9, (byte)74);
                    _client.niomope = true;
                  }
                }
                if (_client.Currentnpctext.StartsWith("Do you want to enter"))
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
              }
            }
          }
          if (!_client.pause && !_client.needsrepaired && !_client.repairmode && _client.Tab.enterbugs.Checked)
          {
            if (_client.MapInfo.Number == 133)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Torrance");
              if (npcByName != null)
              {
                _client.ClickNpc(npcByName.ID);
                if (_client.Currentnpctext.StartsWith("Hello.  What can I do for you?"))
                  _client.PopupOption1();
                if (_client.Currentnpctext.StartsWith("Would you like to enter the Insect Hunting ground?"))
                  _client.PopupOption2();
              }
            }
            if (_client.MapInfo.Number == 6513)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Octavio");
              if (npcByName != null)
              {
                _client.ClickNpc(npcByName.ID);
                if (_client.Currentnpctext.StartsWith("Hello.  What can I do for you?"))
                  _client.PopupOption1();
                if (_client.Currentnpctext.StartsWith("Would you like to enter the Insect Hunting ground?"))
                  _client.PopupOption2();
              }
            }
            if (_client.MapInfo.Number == 10265)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Mank");
              if (npcByName != null)
              {
                _client.ClickNpc(npcByName.ID);
                if (_client.Currentnpctext.StartsWith("Hello.  What can I do for you?"))
                  _client.PopupOption1();
                if (_client.Currentnpctext.StartsWith("Would you like to enter the Insect Hunting ground?"))
                  _client.PopupOption2();
              }
            }
            if (_client.MapInfo.Number == 10001)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Akum");
              if (npcByName != null)
              {
                _client.ClickNpc(npcByName.ID);
                if (_client.Currentnpctext.StartsWith("Hello.  What can I do for you?"))
                  _client.PopupOption1();
                if (_client.Currentnpctext.StartsWith("Would you like to enter the Insect Hunting ground?"))
                  _client.PopupOption2();
              }
            }
          }
          if (!_client.pause && _client.autowalkon)
          {
            if (_client.towerfinish && _client.MapInfo.Number == 706 && _client.Tab.vautowalker_locales.Equals("Nobis") && _client.Tab.vwalklocaleslist.Equals("Tower Maze"))
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Filippo");
              if (npcByName != null && npcByName.IsOnScreen)
              {
                _client.DialogueRespond(npcByName.ID, (byte)7, (byte)236);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)3, (byte)236, (byte)0, (byte)47);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)3, (byte)236, (byte)0, (byte)48);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)3, (byte)236, (byte)0, (byte)49);
                _client.randomdest = (Location)null;
                _client.Tab.autowalker_button.Text = "Start";
                _client.autowalkon = false;
                _client.towerfinish = false;
                Thread.Sleep(1000);
              }
            }
            if (_client.MapInfo.Number == 393 && _client.Tab.vautowalker_locales.Equals("Mileth") && (_client.Tab.vwalklocaleslist.Equals("ToC Warrior") || _client.Tab.vwalklocaleslist.Equals("ToC Monk") || _client.Tab.vwalklocaleslist.Equals("ToC Rogue") || _client.Tab.vwalklocaleslist.Equals("ToC Priest") || _client.Tab.vwalklocaleslist.Equals("ToC Wizard")))
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Aoife");
              if (npcByName != null && npcByName.IsOnScreen)
              {
                _client.DialogueRespond(npcByName.ID, (byte)4, (byte)147);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)147, (byte)0, (byte)1);
                if (_client.Gender == (byte)1)
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)147, (byte)0, (byte)3);
                else
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)147, (byte)0, (byte)6);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)147, (byte)0, (byte)8, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)147, (byte)0, (byte)15, (byte)1, (byte)1);
                Thread.Sleep(1000);
              }
            }
            if (_client.MapInfo.Number == 509)
            {
              foreach (Npc nearbyNpc in _client.NearbyNpcs(Npc.NpcType.Mundane))
              {
                if (nearbyNpc != null && nearbyNpc.IsOnScreen)
                {
                  _client.SkillSpellCaption("repair all");
                  _client.DialogueRespond(nearbyNpc.ID, (byte)7, (byte)153);
                  _client.PopupRespond(new uint?(nearbyNpc.ID), (byte)3, (byte)153, (byte)0, (byte)1, (byte)1, (byte)2);
                  Thread.Sleep(1000);
                  break;
                }
              }
            }
            if (_client.MapInfo.Number == 6525 && _client.WithinRange(29, 18, 21))
            {
              _client.Speak("welcome aisling", 2);
              Thread.Sleep(1000);
            }
            if (_client.MapInfo.Number == 6926)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Quard");
              if (npcByName != null)
              {
                _client.DialogueRespond(npcByName.ID, (byte)9, (byte)23);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
                _client.PopupNext(new uint?(npcByName.ID));
                Thread.Sleep(800);
              }
            }
            if (_client.MapInfo.Number == 10265 && !_client.Tab.vautowalker_locales.Equals("Hwarone") && !_client.Tab.vautowalker_locales.Equals("Veltain Mines"))
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Mank");
              if (npcByName != null)
              {
                _client.DialogueRespond(npcByName.ID, (byte)9, (byte)25);
                _client.PopupNext(new uint?(npcByName.ID));
                _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
                _client.PopupNext(new uint?(npcByName.ID));
                Thread.Sleep(800);
              }
            }
            if ((_client.Tab.vautowalker_locales.Equals("Tavaly Village") || _client.Tab.vautowalker_locales.Equals("Plamit Village") || _client.Tab.vautowalker_locales.Equals("Veltain Mines") || _client.Tab.vautowalker_locales.Equals("Aman Jungle") || _client.Tab.vautowalker_locales.Equals("Lost Ruins") || _client.Tab.vautowalker_locales.Equals("Gladiator Arena") || _client.Tab.vautowalker_locales.Equals("Water Dungeon") || _client.Tab.vautowalker_locales.Equals("Hwarone") || _client.Tab.vautowalker_locales.Equals("Andor") || _client.Tab.vautowalker_locales.Equals("Desert Dunes")) && _client.MapInfo.Number == 5232)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("ColiseumTir");
              if (npcByName != null)
              {
                _client.DialogueRespond(npcByName.ID, (byte)7, (byte)88);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)3, (byte)88, (byte)0, (byte)1, (byte)1, (byte)2);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)3, (byte)88, (byte)0, (byte)12, (byte)1, (byte)2);
                Thread.Sleep(800);
              }
            }
            if (_client.Tab.vwalklocaleslist.Equals("Balanced Arena") && _client.MapInfo.Number == 5232)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("ColiseumTir");
              if (npcByName != null)
              {
                _client.ClickNpc(npcByName.ID);
                if (_client.Currentnpctext.StartsWith("Hello.  What can I do for you?"))
                  _client.PopupOption5();
                _client.PopupNext(new uint?(npcByName.ID));
              }
            }
            if (_client.Tab.vautowalker_locales.Equals("Andor") && _client.Tab.vwalklocaleslist.Equals("Andor Lobby") && Server.DARegged.ContainsKey(_client.Name) && Server.DARegged[_client.Name] && _client.MapInfo.Number != 10038 && !_client.MapInfo.Name.Contains("Andor"))
            {
              if (_client.Currentnpctext.Contains("Map of Ant"))
                _client.PopupNext(new uint?(_client.CurrentnpcpopupID), (byte)2);
              else if (_client.Currentnpctext.StartsWith("Where would you like to go?"))
                _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)3, (byte)2);
              else if (_client.Currentnpctext.StartsWith("You will be transported to"))
                _client.PopupNext(new uint?(_client.CurrentnpcpopupID), (byte)2);
              if (_client.Currentnpctext.StartsWith("((You can only use this once every "))
              {
                if (_client.HasItem("Map of Ant Tunnels"))
                {
                  _client.anttunnel = DateTime.Now;
                  _client.SaveTimedStuff(23);
                }
                else if (_client.HasItem("Map of Ant Guardian Tunnels"))
                {
                  _client.guardiananttunnel = DateTime.Now;
                  _client.SaveTimedStuff(24);
                }
                _client.usetunneldelay = DateTime.UtcNow;
                _client.PopupClose(new uint?(_client.CurrentnpcpopupID));
              }
              if (_client.HasItem("Map of Ant Tunnels") && (_client.anttunnel == DateTime.MinValue || DateTime.Now.Subtract(_client.anttunnel).TotalMinutes > 360.0) && (_client.usetunneldelay == DateTime.MinValue || DateTime.UtcNow.Subtract(_client.usetunneldelay).TotalSeconds > 5.0))
              {
                _client.UseItem("Map of Ant Tunnels");
                _client.usetunneldelay = DateTime.UtcNow;
              }
              else if (_client.HasItem("Map of Ant Guardian Tunnels") && (_client.guardiananttunnel == DateTime.MinValue || DateTime.Now.Subtract(_client.guardiananttunnel).TotalMinutes > 240.0) && (_client.usetunneldelay == DateTime.MinValue || DateTime.UtcNow.Subtract(_client.usetunneldelay).TotalSeconds > 5.0))
              {
                _client.UseItem("Map of Ant Guardian Tunnels");
                _client.usetunneldelay = DateTime.UtcNow;
              }
            }
            if (_client.MapInfo.Number == 10000 && _client.Statistics.Gold > 10000U && _client.Tab.vautowalker_locales != "Asilon")
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Lenoa");
              if (npcByName != null)
              {
                _client.DialogueRespond(npcByName.ID, (byte)9, (byte)25);
                _client.PopupNext(new uint?(npcByName.ID));
                _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
                _client.PopupNext(new uint?(npcByName.ID));
                Thread.Sleep(800);
              }
            }
            if (_client.MapInfo.Number == 503 && !_client.Tab.vautowalker_locales.Equals("Suomi") && !_client.Tab.vautowalker_locales.Equals("Undine") && !_client.Tab.vautowalker_locales.Equals("Mount Giragan") && !_client.Tab.vautowalker_locales.Equals("Astrid") && !_client.Tab.vautowalker_locales.Equals("Sapphire Stream"))
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Keane");
              if (npcByName != null)
              {
                _client.ClickNpc(npcByName.ID);
                if (_client.Currentnpctext.StartsWith("Hello.  What can I do for you?"))
                  _client.PopupOption2();
                if (_client.Currentnpctext.StartsWith("Good day, Aisling. Can I be of some assistance to you?"))
                  _client.PopupOption3();
                _client.PopupNext(new uint?(npcByName.ID));
                Thread.Sleep(800);
              }
            }
            if (!_client.Tab.vautowalker_locales.Equals("Andor") && !_client.Tab.vautowalker_locales.Equals("Desert Dunes") && !_client.Tab.vautowalker_locales.Equals("Noam") && !_client.Tab.vautowalker_locales.Equals("Mt Merry") && _client.MapInfo.Number == 10055 && _client.Statistics.Gold > 10000U)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Habab");
              if (npcByName != null)
              {
                if (_client.carpetdelay == DateTime.MinValue)
                {
                  _client.donotwalk = true;
                  _client.carpetdelay = DateTime.UtcNow;
                }
                if (_client.SafeToWalkFast || DateTime.UtcNow.Subtract(_client.carpetdelay).TotalMilliseconds > 2000.0)
                {
                  _client.carpetdelay = DateTime.MinValue;
                  if (_client.Tab.vautowalker_locales.Equals("Tavaly Village") || _client.Tab.vautowalker_locales.Equals("Plamit Village") || _client.Tab.vautowalker_locales.Equals("Veltain Mines") || _client.Tab.vautowalker_locales.Equals("Aman Jungle") || _client.Tab.vautowalker_locales.Equals("Lost Ruins") || _client.Tab.vautowalker_locales.Equals("Gladiator Arena") || _client.Tab.vautowalker_locales.Equals("Water Dungeon") || _client.Tab.vautowalker_locales.Equals("Hwarone") || _client.Tab.vautowalker_locales.Equals("Asilon"))
                  {
                    _client.DialogueRespond(npcByName.ID, (byte)9, (byte)26);
                    _client.PopupNext(new uint?(npcByName.ID));
                  }
                  else
                  {
                    _client.DialogueRespond(npcByName.ID, (byte)9, (byte)27);
                    _client.PopupNext(new uint?(npcByName.ID));
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
                  }
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
                  _client.PopupNext(new uint?(npcByName.ID));
                  Thread.Sleep(800);
                  _client.donotwalk = false;
                }
              }
            }
            if ((_client.Tab.vautowalker_locales.Equals("Nearest Bank") || _client.Tab.pigwalk.Checked || _client.frostygift || _client.Tab.vautowalker_locales.Equals("Rucesion") && _client.Tab.vwalklocaleslist == "Armor Shop") && _client.Currentnpctext.Contains("Would you like to go "))
            {
              if (_client.Tab.vautowalker_locales.Equals("Nearest Bank") && _client.Nation == (byte)4)
                _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
              else if (_client.Nation == (byte)7 || _client.Nation == (byte)4)
                _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)2);
              else if (_client.Nation < (byte)7)
                _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
            }
            if (_client.MapInfo.Number == 5271 && _client.Tab.dojo.Checked)
              _client.AutoWalkWithinRange(5, 8, 3);
            Npc npcByName1 = _client.FindNpcByName<Npc>("Mionope");
            if (npcByName1 != null)
              _client.ClickNpc(npcByName1.ID);
            if (_client.MapInfo.Number == 3634 && (_client.Tab.vwalklocaleslist.StartsWith("Chadul's Realm") || _client.Tab.vwalklocaleslist.Equals("Chadul Army Invasion")))
            {
              Npc npcByName2 = _client.FindNpcByName<Npc>("Fallen Soldier");
              if (npcByName2 != null && npcByName2.IsOnScreen)
              {
                _client.ClickNpc(npcByName2.ID);
                if (_client.Currentnpctext.StartsWith("Hello.  What can I do for you?"))
                  _client.PopupOption3();
                if (_client.Currentnpctext.StartsWith("I can show you the entrance to the realm of Chadul.  Do you want to go?"))
                  _client.PopupOption2();
              }
            }
            if (_client.MapInfo.Number == 8420 && _client.Currentnpctext.StartsWith("Do you wish to go back to Chaos 1?"))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
            if (_client.Currentnpctext.StartsWith("You fall though a portal and wake up in temuair full of disarray."))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)0, (byte)0, (byte)0, (byte)1, (byte)1, (byte)1);
            if (_client.MapInfo.Number == 3210 && _client.Currentnpctext.StartsWith("WARNING: You already defeated 'Shade of Ealagad'"))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
            if (_client.MapInfo.Number == 2900 && _client.Currentnpctext.StartsWith("Take the same path you used to get here?"))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
            if (_client.Tab.vwalklocaleslist == "Team Arena Lobby" && _client.Currentnpctext.StartsWith("There is a small fee of 100 gold to enter."))
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID), (byte)2);
            if (_client.Tab.vwalklocaleslist == "Coliseum Gauntlet" && _client.MapInfo.Number == 5232)
            {
              Npc npcByName3 = _client.FindNpcByName<Npc>("ColiseumTir");
              if (npcByName3 != null)
              {
                _client.ClickNpc(npcByName3.ID);
                while (_client.Currentnpctext == string.Empty)
                  Thread.Sleep(100);
                if (_client.distracted)
                {
                  _client.Refresh();
                  _client.distracted = false;
                }
                if (_client.Currentnpctext.StartsWith("Hello.  What can I do for you?"))
                  _client.PopupOption6();
                Thread.Sleep(100);
                if (_client.Currentnpctext.StartsWith("Enter as a spectator or combatant?"))
                  _client.PopupOption3();
                Thread.Sleep(150);
                if (_client.Currentnpctext.StartsWith("Do you accept the Arena Host's settings?"))
                  _client.PopupOption2();
                Thread.Sleep(500);
              }
            }
            if (_client.MapInfo.Number == 5232 && !_client.Tab.vwalklocaleslist.Equals("Coliseum Gauntlet"))
              _client.AutoWalker(11, 2);
            if (_client.MapInfo.Number == 5220 && _client.Currentnpctext.StartsWith("Will you be respectful of the monastery"))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)195, (byte)0, (byte)6, (byte)1, (byte)2);
            if (_client.MapInfo.Number == 5210 && (_client.Tab.vwalklocaleslist.Equals("White Grove") || _client.Tab.vwalklocaleslist.Equals("Green Grove") || _client.Tab.vwalklocaleslist.Equals("Blue Grove") || _client.Tab.vwalklocaleslist.Equals("Yellow Grove") || _client.Tab.vwalklocaleslist.Equals("Purple Grove") || _client.Tab.vwalklocaleslist.Equals("Brown Grove") || _client.Tab.vwalklocaleslist.Equals("Red Grove") || _client.Tab.vwalklocaleslist.Equals("Black Grove")))
              _client.AutoWalker(0, 9);
            if (_client.Tab.vwalklocaleslist.Equals("White Grove"))
              _client.PopupOption8();
            if (_client.Tab.vwalklocaleslist.Equals("Green Grove"))
              _client.PopupOption7();
            if (_client.Tab.vwalklocaleslist.Equals("Blue Grove"))
              _client.PopupOption6();
            if (_client.Tab.vwalklocaleslist.Equals("Yellow Grove"))
              _client.PopupOption5();
            if (_client.Tab.vwalklocaleslist.Equals("Purple Grove"))
              _client.PopupOption4();
            if (_client.Tab.vwalklocaleslist.Equals("Brown Grove"))
              _client.PopupOption3();
            if (_client.Tab.vwalklocaleslist.Equals("Red Grove"))
              _client.PopupOption2();
            if (_client.Tab.vwalklocaleslist.Equals("Black Grove"))
              _client.PopupOption1();
            if (_client.MapInfo.Number == 662 && _client.Tab.vwalklocaleslist.Equals("Warrior Blacksmith") && _client.ServerLocation.X == 77 && _client.ServerLocation.Y == 69)
              _client.AutoWalker(76, 69);
            if (_client.MapInfo.Number == 662 && (_client.Tab.vwalklocaleslist.Equals("Warrior Trainer") || _client.Tab.vwalklocaleslist.Equals("Warrior Trainer 2")) && _client.ServerLocation.X == 83 && _client.ServerLocation.Y == 65)
              _client.AutoWalker(83, 64);
            if (!_client.Tab.vautowalker_locales.Equals("Mt Merry"))
            {
              if (_client.MapInfo.Number == 7071)
                _client.AutoWalker(43, 48);
              if (_client.MapInfo.Name.StartsWith("Mother Erbie 5-"))
                _client.AutoWalker(44, 17);
              if (_client.MapInfo.Number == 7050)
                _client.AutoWalker(20, 8);
              else if (_client.MapInfo.Number == 7055 && !_client.WithinRange(5, 6, 2))
                _client.AutoWalkWithinRange(5, 6, 2);
              Npc npcByName4 = _client.FindNpcByName<Npc>("Helper");
              if (npcByName4 != null && npcByName4.IsOnScreen)
              {
                _client.ClickNpc(npcByName4.ID);
                if (_client.Currentnpctext.StartsWith("Hi, I'm the lift operator."))
                  _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
                if (_client.Currentnpctext.StartsWith("Bored already?"))
                  _client.PopupNext(new uint?(npcByName4.ID));
                if (_client.Currentnpctext.StartsWith("((This will take you away from the North Pole,"))
                  _client.PopupNext(new uint?(npcByName4.ID));
                Thread.Sleep(400);
                _client.donotwalk = false;
              }
            }
          }
          if (_client.Currentnpctext != string.Empty)
          {
            if (_client.Currentnpctext.Contains("Hydele") && _client.Currentnpctext.Contains("repare "))
            {
              Location location = new Location(_client.ServerLocation.X, _client.ServerLocation.Y);
              string key1 = location.X.ToString() + "," + location.Y.ToString() + "," + _client.MapInfo.Number.ToString();
              bool flag = false;
              foreach (string key2 in Server.HydeleNodes.Keys)
              {
                if (key2 == key1)
                  flag = true;
              }
              if (!flag)
              {
                HerbNode herbNode = new HerbNode();
                herbNode.Type = "Hydele";
                herbNode.Map = _client.MapInfo.Number;
                herbNode.Location.X = location.X;
                herbNode.Location.Y = location.Y;
                herbNode.Active = true;
                Server.HydeleNodes.Add(key1, herbNode);
                Server.HerbNodes.Add(key1, herbNode);
                _client.SaveHerbNodes();
                _client.SendMessage(location.X.ToString() + "," + location.Y.ToString() + " added to Hydele Nodes");
              }
            }
            else if (_client.Currentnpctext.Contains("Betony") && _client.Currentnpctext.Contains("repare "))
            {
              Location location = new Location(_client.ServerLocation.X, _client.ServerLocation.Y);
              string key3 = location.X.ToString() + "," + location.Y.ToString() + "," + _client.MapInfo.Number.ToString();
              bool flag = false;
              foreach (string key4 in Server.BetonyNodes.Keys)
              {
                if (key4 == key3)
                  flag = true;
              }
              if (!flag)
              {
                HerbNode herbNode = new HerbNode();
                herbNode.Type = "Betony";
                herbNode.Map = _client.MapInfo.Number;
                herbNode.Location.X = location.X;
                herbNode.Location.Y = location.Y;
                herbNode.Active = true;
                Server.BetonyNodes.Add(key3, herbNode);
                Server.HerbNodes.Add(key3, herbNode);
                _client.SaveHerbNodes();
                _client.SendMessage(location.X.ToString() + "," + location.Y.ToString() + " added to Betony Nodes");
              }
            }
            else if (_client.Currentnpctext.Contains("Personaca") && _client.Currentnpctext.Contains("repare "))
            {
              Location location = new Location(_client.ServerLocation.X, _client.ServerLocation.Y);
              string key5 = location.X.ToString() + "," + location.Y.ToString() + "," + _client.MapInfo.Number.ToString();
              bool flag = false;
              foreach (string key6 in Server.PersonacaNodes.Keys)
              {
                if (key6 == key5)
                  flag = true;
              }
              if (!flag)
              {
                HerbNode herbNode = new HerbNode();
                herbNode.Type = "Personaca";
                herbNode.Map = _client.MapInfo.Number;
                herbNode.Location.X = location.X;
                herbNode.Location.Y = location.Y;
                herbNode.Active = true;
                Server.PersonacaNodes.Add(key5, herbNode);
                Server.HerbNodes.Add(key5, herbNode);
                _client.SaveHerbNodes();
                _client.SendMessage(location.X.ToString() + "," + location.Y.ToString() + " added to Personaca Nodes");
              }
            }
            else if (_client.Currentnpctext.Contains("Ancusa") && _client.Currentnpctext.Contains("repare "))
            {
              Location location = new Location(_client.ServerLocation.X, _client.ServerLocation.Y);
              string key7 = location.X.ToString() + "," + location.Y.ToString() + "," + _client.MapInfo.Number.ToString();
              bool flag = false;
              foreach (string key8 in Server.PersonacaNodes.Keys)
              {
                if (key8 == key7)
                  flag = true;
              }
              if (!flag)
              {
                Server.HerbNodes.Add(key7, new HerbNode()
                {
                  Type = "Ancusa",
                  Map = _client.MapInfo.Number,
                  Location = {
                  X = location.X,
                  Y = location.Y
                },
                  Active = true
                });
                _client.SaveHerbNodes();
                _client.SendMessage(location.X.ToString() + "," + location.Y.ToString() + " added to Ancusa Nodes");
              }
            }
            else if (_client.Currentnpctext.Contains("Hemloch") && _client.Currentnpctext.Contains("repare "))
            {
              Location location = new Location(_client.ServerLocation.X, _client.ServerLocation.Y);
              string key9 = location.X.ToString() + "," + location.Y.ToString() + "," + _client.MapInfo.Number.ToString();
              bool flag = false;
              foreach (string key10 in Server.PersonacaNodes.Keys)
              {
                if (key10 == key9)
                  flag = true;
              }
              if (!flag)
              {
                Server.HerbNodes.Add(key9, new HerbNode()
                {
                  Type = "Hemloch",
                  Map = _client.MapInfo.Number,
                  Location = {
                  X = location.X,
                  Y = location.Y
                },
                  Active = true
                });
                _client.SaveHerbNodes();
                _client.SendMessage(location.X.ToString() + "," + location.Y.ToString() + " added to Hemloch Nodes");
              }
            }
            else if (_client.Currentnpctext.Contains("Fifleaf") && _client.Currentnpctext.Contains("repare "))
            {
              Location location = new Location(_client.ServerLocation.X, _client.ServerLocation.Y);
              string key11 = location.X.ToString() + "," + location.Y.ToString() + "," + _client.MapInfo.Number.ToString();
              bool flag = false;
              foreach (string key12 in Server.PersonacaNodes.Keys)
              {
                if (key12 == key11)
                  flag = true;
              }
              if (!flag)
              {
                Server.HerbNodes.Add(key11, new HerbNode()
                {
                  Type = "Fifleaf",
                  Map = _client.MapInfo.Number,
                  Location = {
                  X = location.X,
                  Y = location.Y
                },
                  Active = true
                });
                _client.SaveHerbNodes();
                _client.SendMessage(location.X.ToString() + "," + location.Y.ToString() + " added to Fifleaf Nodes");
              }
            }
            else if (_client.Currentnpctext.Contains("Collect fior sal"))
            {
              Location location = new Location(_client.ServerLocation.X, _client.ServerLocation.Y);
              string key13 = location.X.ToString() + "," + location.Y.ToString() + "," + _client.MapInfo.Number.ToString();
              bool flag = false;
              foreach (string key14 in Server.PersonacaNodes.Keys)
              {
                if (key14 == key13)
                  flag = true;
              }
              if (!flag)
              {
                Server.HerbNodes.Add(key13, new HerbNode()
                {
                  Type = "sal",
                  Map = _client.MapInfo.Number,
                  Location = {
                  X = location.X,
                  Y = location.Y
                },
                  Active = true
                });
                _client.SaveHerbNodes();
                _client.SendMessage(location.X.ToString() + "," + location.Y.ToString() + " added to sal Nodes");
              }
            }
            else if (_client.Currentnpctext.Contains("Collect fior srad"))
            {
              Location location = new Location(_client.ServerLocation.X, _client.ServerLocation.Y);
              string key15 = location.X.ToString() + "," + location.Y.ToString() + "," + _client.MapInfo.Number.ToString();
              bool flag = false;
              foreach (string key16 in Server.PersonacaNodes.Keys)
              {
                if (key16 == key15)
                  flag = true;
              }
              if (!flag)
              {
                Server.HerbNodes.Add(key15, new HerbNode()
                {
                  Type = "srad",
                  Map = _client.MapInfo.Number,
                  Location = {
                  X = location.X,
                  Y = location.Y
                },
                  Active = true
                });
                _client.SaveHerbNodes();
                _client.SendMessage(location.X.ToString() + "," + location.Y.ToString() + " added to srad Nodes");
              }
            }
            else if (_client.Currentnpctext.Contains("Collect fior athar"))
            {
              Location location = new Location(_client.ServerLocation.X, _client.ServerLocation.Y);
              string key17 = location.X.ToString() + "," + location.Y.ToString() + "," + _client.MapInfo.Number.ToString();
              bool flag = false;
              foreach (string key18 in Server.PersonacaNodes.Keys)
              {
                if (key18 == key17)
                  flag = true;
              }
              if (!flag)
              {
                Server.HerbNodes.Add(key17, new HerbNode()
                {
                  Type = "athar",
                  Map = _client.MapInfo.Number,
                  Location = {
                  X = location.X,
                  Y = location.Y
                },
                  Active = true
                });
                _client.SaveHerbNodes();
                _client.SendMessage(location.X.ToString() + "," + location.Y.ToString() + " added to athar Nodes");
              }
            }
            else if (_client.Currentnpctext.Contains("Collect fior creag"))
            {
              Location location = new Location(_client.ServerLocation.X, _client.ServerLocation.Y);
              string key19 = location.X.ToString() + "," + location.Y.ToString() + "," + _client.MapInfo.Number.ToString();
              bool flag = false;
              foreach (string key20 in Server.PersonacaNodes.Keys)
              {
                if (key20 == key19)
                  flag = true;
              }
              if (!flag)
              {
                Server.HerbNodes.Add(key19, new HerbNode()
                {
                  Type = "creag",
                  Map = _client.MapInfo.Number,
                  Location = {
                  X = location.X,
                  Y = location.Y
                },
                  Active = true
                });
                _client.SaveHerbNodes();
                _client.SendMessage(location.X.ToString() + "," + location.Y.ToString() + " added to creag Nodes");
              }
            }
          }
          if (!_client.pause && _client.Tab.useexpgem.Checked)
          {
            if (_client.Currentnpctext.StartsWith("Using this item will allow you to sell exp"))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupNext(new uint?(currentnpcpopupId), (byte)2);
              if (!_client.Tab.expgemmp.Checked)
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1, (byte)2);
              else
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)2, (byte)2);
              _client.PopupNext(new uint?(currentnpcpopupId), (byte)2);
              _client.PopupNext(new uint?(currentnpcpopupId), (byte)2);
              _client.PopupNext(new uint?(currentnpcpopupId), (byte)2);
            }
            if (_client.Currentnpctext.StartsWith("Remember you CANNOT GAIN BACK"))
            {
              _client.beforeascend = _client.Statistics.Experience;
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupNext(new uint?(currentnpcpopupId));
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)2);
              _client.PopupClose(new uint?(currentnpcpopupId));
              _client.ascendexp = false;
              _client.expgemtimer = DateTime.MinValue;
            }
          }
          if (!_client.pause && _client.Currentnpctext.StartsWith("The full Red Moon"))
            _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)0, (byte)0, (byte)0, (byte)1, (byte)4);
          if (_client.MapInfo.Number == 1960 && _client.Currentnpctext.StartsWith("Welcome to the fair town of Tagor"))
          {
            if (!_client.tagorcitpopup)
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)191, (byte)0, (byte)16, (byte)1, (byte)2);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)191, (byte)0, (byte)75);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)191, (byte)0, (byte)76, (byte)1, (byte)1);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)191, (byte)0, (byte)81);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)191, (byte)0, (byte)89);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)191, (byte)0, (byte)90);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)191, (byte)0, (byte)91);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)191, (byte)0, (byte)91);
              _client.tagorcitpopup = true;
              Thread.Sleep(1000);
              _client.Tab.fastwalk.Checked = true;
              _client.Tab.autowalker_locales.SelectedItem = (object)"Tagor";
              _client.Tab.walklocaleslist.SelectedItem = (object)"Lost Path";
              _client.Tab.autowalker_button.Text = "Stop";
              _client.autowalkon = true;
            }
            else
            {
              _client.tagorcitpopup = false;
              Thread.Sleep(1000);
            }
          }
          if (_client.MapInfo.Name != null && _client.MapInfo.Name.StartsWith("Path Temple ") && !string.IsNullOrEmpty(_client.Currentnpctext))
          {
            _client.tocpopup = true;
            _client.PopupClose(new uint?(_client.CurrentnpcpopupID));
          }
          if (_client.Currentnpctext.StartsWith("You unlock the large door") && (_client.MapInfo.Number == 6000 || _client.MapInfo.Number == 6002))
            _client.PopupClose(new uint?(_client.CurrentnpcpopupID));
          if (!_client.pause && _client.MapInfo.Number == 2051 && !_client.autowalkon)
          {
            if (_client.Currentnpctext == string.Empty && !_client.gotmanorkey)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Dreval");
              if (npcByName != null)
              {
                _client.DialogueRespond(npcByName.ID, (byte)9, (byte)216);
                uint id = npcByName.ID;
                _client.PopupNext(new uint?(id));
                _client.PopupNext(new uint?(id));
                _client.PopupNext(new uint?(id));
                _client.PopupNext(new uint?(id));
                _client.PopupNext(new uint?(id));
                _client.PopupClose(new uint?(id));
                Thread.Sleep(1000);
                _client.gotmanorkey = true;
                _client.Tab.fastwalk.Checked = true;
                _client.Tab.autowalker_locales.SelectedItem = (object)"Eingren Manor";
                _client.Tab.autowalker_button.Text = "Stop";
                _client.autowalkon = true;
              }
            }
            if (_client.Currentnpctext.StartsWith("I just gave you the key,"))
            {
              _client.gotmanorkey = true;
              _client.PopupClose(new uint?(_client.CurrentnpcpopupID));
              _client.SendMessage("You can't get another key yet.");
            }
          }
          if (!_client.pause && _client.MapInfo.Number == 662 && _client.Currentnpctext.StartsWith("You see strange dark fog"))
            _client.PopupNext(new uint?(_client.CurrentnpcpopupID));
          if (!_client.pause && _client.MapInfo.Number == 4009 && _client.Currentnpctext.StartsWith("You feel a pulse"))
            _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)107, (byte)0, (byte)1, (byte)1, (byte)1);
          if (_client.MapInfo.Number == 8989 && _client.Currentnpctext.StartsWith("This must be where the Slabs"))
            _client.PopupNext(new uint?(_client.CurrentnpcpopupID));
          if (!_client.pause && _client.MapInfo.Name == "North Pole" && !_client.autowalkon && _client.megprize)
          {
            Npc npcByName = _client.FindNpcByName<Npc>("MotherErbie");
            if (npcByName != null)
            {
              _client.DialogueRespond(new uint?(npcByName.ID), "MotherErbie");
              _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)0, (byte)0, (byte)2);
              _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)0, (byte)0, (byte)2);
              Thread.Sleep(800);
              _client.megprize = false;
              _client.LogOff();
            }
          }
          if (!_client.pause && _client.MapInfo.Number == 115 && !_client.autowalkon && _client.HasMPig() && _client.HasFPig())
          {
            Npc npcByName = _client.FindNpcByName<Npc>("Jay");
            if (npcByName != null)
            {
              _client.DialogueRespond(new uint?(npcByName.ID), "Jay");
              _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)0, (byte)0, (byte)2);
              _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)0, (byte)0, (byte)2);
              Thread.Sleep(1000);
              if (_client.HasItem("Pig Hairpin"))
                _client.DropItems("Pig Hairpin");
              if (_client.HasItem("Pig Head"))
                _client.DropItems("Pig Head");
              if (_client.HasItem("Pig Body"))
                _client.DropItems("Pig Body");
              if (_client.HasItem("Pet Male Pig"))
                _client.DropItems("Pet Male Pig");
              if (_client.HasItem("Pet Female Pig"))
                _client.DropItems("Pet Female Pig");
              if (_client.HasItem("Event Star Medal"))
                _client.DropItems("Event Star Medal");
              Thread.Sleep(1000);
              _client.LogOff();
            }
          }
          if (_client.Currentnpctext != string.Empty)
          {
            if (!_client.lawquest && _client.MapInfo.Number == 8988)
              _client.PopupClose(new uint?(_client.CurrentnpcpopupID), (byte)4);
            if (!_client.pause && _client.Tab.openveltchest.Checked && _client.Tab.openveltchestgold.Text != string.Empty && !_client.Currentnpcname.StartsWith("Heavy") && _client.Currentnpcname.Equals("Veltain Treasure Chest") && _client.Currentnpctext.StartsWith("You are about to pull an item out of the chest,"))
            {
              _client.chestfee = _client.Tab.openveltchestgold.Text;
              _client.veltainchestopen = true;
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID), (byte)2);
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID), (byte)2);
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)2, _client.Tab.openveltchestgold.Text, (byte)2);
            }
            if (!_client.pause && _client.Currentnpcname.StartsWith("LA Raffle") && _client.Currentnpctext.StartsWith("This LA Raffle is good for one try in obtaining the Lumen Amulet"))
            {
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID), (byte)2);
            }

            if (_client.Currentnpctext.StartsWith("You are about to enter a hostile area."))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1, (byte)4);
            if (_client.Currentnpctext.StartsWith("You fall into a deep sleep"))
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID), (byte)4);
            else if (_client.Currentnpctext.StartsWith("((You are about to be taken"))
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID), (byte)4);
            else if (_client.Currentnpctext.StartsWith("Lying in bed, "))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1, (byte)4);
            else if (!_client.pause && _client.Currentnpctext.StartsWith("Ask a friend to show you"))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)0, (byte)108, (byte)0, (byte)6, (byte)4);
            else if (_client.Currentnpctext.StartsWith("((This powerful scent"))
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID), (byte)2);
            else if (_client.Currentnpctext.StartsWith("((You are entering a role-playing"))
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID), (byte)4);
            else if (_client.Currentnpctext.StartsWith("You can't bring weapons here!"))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)0, (byte)204, (byte)1, (byte)43, (byte)4);
            else if (_client.Currentnpctext.StartsWith("Come back when you have money."))
            {
              _client.SendMessage(_client.Name + " couldn't afford repairs!", "red", true);
              _client.PopupClose(new uint?(_client.CurrentnpcpopupID));
            }
            else if (_client.withdrawmode == 1 && _client.Currentnpctext != string.Empty && !_client.Currentnpctext.StartsWith("Here is what you have deposited"))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupClose(new uint?(currentnpcpopupId));
              if (_client.SafeToWalkFast)
                _client.DialogueRespond(new uint?(currentnpcpopupId), "Withdraw");
              _client.withdrawmode = 2;
            }
            else if (_client.depositmode == 1 && _client.Currentnpctext != string.Empty && !_client.Currentnpctext.StartsWith("I can only take new or fully repaired"))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupClose(new uint?(currentnpcpopupId));
              if (_client.SafeToWalkFast)
                _client.DialogueRespond(new uint?(currentnpcpopupId), "Deposit");
              _client.depositmode = 2;
            }
            else if (_client.sendmode == 1 && _client.Currentnpctext.Contains("It will be done."))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupClose(new uint?(currentnpcpopupId));
              _client.DialogueRespond(new uint?(currentnpcpopupId), "Send Parcel");
              _client.sendmode = 2;
            }
            else if (_client.repairmode && _client.Currentnpctext.Contains("Your items have been repaired."))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              if (_client.goldbefore > _client.Statistics.Gold)
                _client.SendMessage("repair bill: " + Math.Abs((long)(_client.goldbefore - _client.Statistics.Gold)).ToString() + " coins.");
              else
                _client.SendMessage("Cannot afford repairs or something.");
              _client.PopupClose(new uint?(currentnpcpopupId));
              _client.repairmode = false;
              _client.needsrepaired = false;
            }
            else if (_client.banklist && _client.Currentnpctext.StartsWith("Here is what you have deposited"))
            {
              _client.banklist = false;
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)0, (byte)69, (byte)0, (byte)1);
            }
          }
          if (!_client.pause && _client.MapInfo.Number == 421)
          {
            if (_client.Currentnpctext.Contains(" wishes to be your mentor. If you "))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)1, (byte)56, (byte)0, (byte)80);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)1, (byte)56, (byte)0, (byte)81, (byte)1, (byte)2);
            }
            else if (_client.Currentnpctext.Contains(" has accepted you as mentor. Teach"))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)1, (byte)56, (byte)0, (byte)118);
          }
          if (!_client.pause && _client.Tab.AscendOptions.vascendbutton)
          {
            if (_client.HasItem("Warranty Bag"))
            {
              _client.SendMessage("Deposit your Warranty Bag!");
              _client.Tab.AscendOptions.ascendbutton.Text = "Start";
            }
            if (_client.Tab.AscendOptions.vascendhp)
            {
              if (_client.MapInfo.Number == 435)
              {
                if (_client.ServerLocation.Y < 27 && _client.ServerLocation.Y > 17 && _client.ServerLocation.X < 6)
                  _client.WalkToExact(4, 20);
                else
                  _client.WalkToExact(6, 23);
              }
              if (_client.MapInfo.Number == 3085)
                _client.WalkToExact(10, 13);
              if (_client.MapInfo.Number == 3086)
              {
                if (_client.ServerLocation.Y == 5 && (_client.ServerLocation.X == 7 || _client.ServerLocation.X == 6))
                  _client.WalkWithinRange(6, 2, 2);
                else if (_client.ServerLocation.Y == 6 && (_client.ServerLocation.X == 7 || _client.ServerLocation.X == 6))
                {
                  _client.WalkWithinRange(6, 9, 2);
                }
                else
                {
                  Npc npcByName = _client.FindNpcByName<Npc>("Deoch");
                  if (npcByName != null)
                  {
                    if (!_client.deoch)
                    {
                      _client.DialogueRespond(new uint?(npcByName.ID), "Default");
                      _client.deoch = true;
                    }
                    if (_client.Tab.AscendOptions.instantascend.Checked && _client.SafeToWalkFast)
                    {
                      if (_client.Currentnpctext.StartsWith("Aisling, do."))
                        _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)114, (byte)0, (byte)2, (byte)1, (byte)1);
                      if (_client.Currentnpctext.StartsWith("You will transform your Work"))
                        _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)114, (byte)0, (byte)28);
                      if (_client.Currentnpctext.StartsWith("((It costs "))
                      {
                        uint num5 = uint.Parse(_client.Currentnpctext.Split(' ')[2].ToString());
                        _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)114, (byte)0, (byte)30);
                        uint num6 = 0;
                        ulong num7 = 0;
                        while (true)
                        {
                          num7 += (ulong)(num5 * 500U);
                          if (num7 <= (ulong)_client.Statistics.Experience)
                          {
                            num5 += 50U;
                            ++num6;
                          }
                          else
                            break;
                        }
                        for (uint index = 0; index < num6; ++index)
                        {
                          _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)114, (byte)0, (byte)51, (byte)1, (byte)2);
                          _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)114, (byte)0, (byte)80, (byte)1, (byte)2);
                          Thread.Sleep(10);
                        }
                        Thread.Sleep(1000);
                        _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)114, (byte)0, (byte)47);
                        _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)114, (byte)0, (byte)85, (byte)1, (byte)3);
                        _client.Tab.AscendOptions.ascendbutton.Text = "Start";
                        Thread.Sleep(1000);
                      }
                    }
                    else
                    {
                      if (_client.Currentnpctext.StartsWith("Aisling, do."))
                        _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)114, (byte)0, (byte)2, (byte)1, (byte)1);
                      if (_client.Currentnpctext.StartsWith("You will transform your Work"))
                        _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)114, (byte)0, (byte)28);
                      if (_client.Currentnpctext.StartsWith("((It costs "))
                        _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)114, (byte)0, (byte)30);
                      if (_client.Currentnpctext.StartsWith("You remember that you cannot"))
                        _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)114, (byte)0, (byte)51, (byte)1, (byte)2);
                      if (_client.Currentnpctext.StartsWith("Do you prostrate yourself"))
                        _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)114, (byte)0, (byte)80, (byte)1, (byte)2);
                      if (_client.Currentnpctext.StartsWith("You lack experience"))
                        _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)114, (byte)0, (byte)47);
                      if (_client.Currentnpctext.StartsWith("Do you now descend"))
                      {
                        _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)114, (byte)0, (byte)85, (byte)1, (byte)3);
                        _client.Tab.AscendOptions.ascendbutton.Text = "Start";
                      }
                    }
                  }
                }
              }
            }
            else if (_client.Tab.AscendOptions.buyto.Checked)
            {
              if (_client.MapInfo.Number == 435)
              {
                if (_client.ServerLocation.Y < 27 && _client.ServerLocation.Y > 17 && _client.ServerLocation.X < 6)
                  _client.WalkToExact(4, 20);
                else
                  _client.WalkToExact(6, 23);
              }
              if (_client.MapInfo.Number == 3085)
              {
                if (_client.Tab.AscendOptions.buytohpvalue.Value > 0M && _client.Tab.AscendOptions.buytohpvalue.Value > (Decimal)_client.Statistics.MaximumHP)
                  _client.WalkToExact(10, 13);
                else if (_client.Tab.AscendOptions.buytompvalue.Value > 0M && _client.Tab.AscendOptions.buytompvalue.Value > (Decimal)_client.Statistics.MaximumMP)
                  _client.WalkToExact(10, 6);
              }
              if (_client.MapInfo.Number == 3086)
              {
                if (_client.Tab.AscendOptions.buytohpvalue.Value > 0M && _client.Tab.AscendOptions.buytohpvalue.Value > (Decimal)_client.Statistics.MaximumHP)
                {
                  if (_client.ServerLocation.Y == 5 && (_client.ServerLocation.X == 7 || _client.ServerLocation.X == 6))
                    _client.WalkWithinRange(6, 2, 2);
                  else if (_client.ServerLocation.Y == 6 && (_client.ServerLocation.X == 7 || _client.ServerLocation.X == 6))
                  {
                    _client.WalkWithinRange(6, 9, 2);
                  }
                  else
                  {
                    Npc npcByName = _client.FindNpcByName<Npc>("Deoch");
                    if (npcByName != null)
                    {
                      if (!_client.deoch)
                      {
                        _client.DialogueRespond(new uint?(npcByName.ID), "Default");
                        _client.deoch = true;
                      }
                      if (_client.Tab.AscendOptions.instantascend.Checked && _client.SafeToWalkFast)
                      {
                        if (_client.Currentnpctext.StartsWith("Aisling, do."))
                        {
                          _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)114, (byte)0, (byte)2, (byte)1, (byte)1);
                          _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)114, (byte)0, (byte)28);
                          _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)114, (byte)0, (byte)30);
                          Thread.Sleep(200);
                          int num = (int)Math.Ceiling(((double)_client.Tab.AscendOptions.buytohpvalue.Value - (double)_client.Statistics.MaximumHP) / 50.0);
                          for (int index = 0; index < num; ++index)
                          {
                            _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)114, (byte)0, (byte)51, (byte)1, (byte)2);
                            _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)114, (byte)0, (byte)80, (byte)1, (byte)2);
                            Thread.Sleep(10);
                          }
                          Thread.Sleep(1000);
                        }
                      }
                      else if (_client.Currentnpctext.StartsWith("Aisling, do."))
                      {
                        _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)114, (byte)0, (byte)2, (byte)1, (byte)1);
                        Thread.Sleep(1000);
                        _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)114, (byte)0, (byte)28);
                        Thread.Sleep(1000);
                        _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)114, (byte)0, (byte)30);
                        Thread.Sleep(1000);
                        int num = (int)Math.Ceiling(((double)_client.Tab.AscendOptions.buytohpvalue.Value - (double)_client.Statistics.MaximumHP) / 50.0);
                        for (int index = 0; index < num; ++index)
                        {
                          _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)114, (byte)0, (byte)51, (byte)1, (byte)2);
                          _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)114, (byte)0, (byte)80, (byte)1, (byte)2);
                          Thread.Sleep(2000);
                        }
                      }
                    }
                  }
                }
                else if (_client.Tab.AscendOptions.buytompvalue.Value > 0M && _client.Tab.AscendOptions.buytompvalue.Value > (Decimal)_client.Statistics.MaximumMP)
                {
                  if (_client.Currentnpctext != string.Empty)
                    _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)114, (byte)0, (byte)50);
                  _client.WalkToExact(8, 5);
                }
                else if (_client.Currentnpctext.StartsWith("You remember that you cannot"))
                {
                  Npc npcByName = _client.FindNpcByName<Npc>("Deoch");
                  if (npcByName != null)
                  {
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)114, (byte)0, (byte)51, (byte)1, (byte)1);
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)114, (byte)0, (byte)85, (byte)1, (byte)3);
                    if (_client.Tab.AscendOptions.buytompvalue.Value == 0M || _client.Tab.AscendOptions.buytompvalue.Value <= (Decimal)_client.Statistics.MaximumMP)
                      _client.Tab.AscendOptions.ascendbutton.Text = "Start";
                    Thread.Sleep(1000);
                  }
                }
              }
              if (_client.MapInfo.Number == 3087)
              {
                if (_client.Tab.AscendOptions.buytompvalue.Value > 0M && _client.Tab.AscendOptions.buytompvalue.Value > (Decimal)_client.Statistics.MaximumMP)
                {
                  if (_client.ServerLocation.Y == 5 && (_client.ServerLocation.X == 7 || _client.ServerLocation.X == 6))
                    _client.WalkWithinRange(6, 2, 2);
                  else if (_client.ServerLocation.Y == 6 && (_client.ServerLocation.X == 7 || _client.ServerLocation.X == 6))
                  {
                    _client.WalkWithinRange(6, 9, 2);
                  }
                  else
                  {
                    Npc npcByName = _client.FindNpcByName<Npc>("Gramail");
                    if (npcByName != null)
                    {
                      if (!_client.gramail)
                      {
                        _client.DialogueRespond(new uint?(npcByName.ID), "Default");
                        _client.gramail = true;
                      }
                      if (_client.Tab.AscendOptions.instantascend.Checked && _client.SafeToWalkFast)
                      {
                        if (_client.Currentnpctext.StartsWith("Aisling, live true."))
                        {
                          _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)115, (byte)0, (byte)2, (byte)1, (byte)1);
                          _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)115, (byte)0, (byte)28);
                          _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)115, (byte)0, (byte)30);
                          Thread.Sleep(200);
                          int num = (int)Math.Ceiling(((double)_client.Tab.AscendOptions.buytompvalue.Value - (double)_client.Statistics.MaximumMP) / 25.0);
                          for (int index = 0; index < num; ++index)
                          {
                            _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)115, (byte)0, (byte)47, (byte)1, (byte)2);
                            _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)115, (byte)0, (byte)73, (byte)1, (byte)2);
                            Thread.Sleep(10);
                          }
                          Thread.Sleep(1000);
                        }
                      }
                      else if (_client.Currentnpctext.StartsWith("Aisling, live true."))
                      {
                        _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)115, (byte)0, (byte)2, (byte)1, (byte)1);
                        Thread.Sleep(1000);
                        _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)115, (byte)0, (byte)28);
                        Thread.Sleep(1000);
                        _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)115, (byte)0, (byte)30);
                        Thread.Sleep(1000);
                        int num = (int)Math.Ceiling(((double)_client.Tab.AscendOptions.buytompvalue.Value - (double)_client.Statistics.MaximumMP) / 25.0);
                        for (int index = 0; index < num; ++index)
                        {
                          _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)115, (byte)0, (byte)47, (byte)1, (byte)2);
                          _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)115, (byte)0, (byte)73, (byte)1, (byte)2);
                          Thread.Sleep(2000);
                        }
                      }
                    }
                  }
                }
                else if (_client.Tab.AscendOptions.buytohpvalue.Value > 0M && _client.Tab.AscendOptions.buytohpvalue.Value > (Decimal)_client.Statistics.MaximumHP)
                  _client.WalkToExact(8, 6);
                else if (_client.Currentnpctext.StartsWith("You remember that you cannot"))
                {
                  Npc npcByName = _client.FindNpcByName<Npc>("Gramail");
                  if (npcByName != null)
                  {
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)115, (byte)0, (byte)47, (byte)1, (byte)1);
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)115, (byte)0, (byte)78, (byte)1, (byte)3);
                    if (_client.Tab.AscendOptions.buytohpvalue.Value == 0M || _client.Tab.AscendOptions.buytohpvalue.Value <= (Decimal)_client.Statistics.MaximumHP)
                      _client.Tab.AscendOptions.ascendbutton.Text = "Start";
                    Thread.Sleep(1000);
                  }
                }
              }
            }
            else if (_client.Tab.AscendOptions.vascendmp)
            {
              if (_client.MapInfo.Number == 435)
              {
                if (_client.ServerLocation.Y < 27 && _client.ServerLocation.Y > 17 && _client.ServerLocation.X < 6)
                  _client.WalkToExact(4, 20);
                else
                  _client.WalkToExact(6, 23);
              }
              if (_client.MapInfo.Number == 3085)
                _client.WalkToExact(10, 6);
              if (_client.MapInfo.Number == 3087)
              {
                if (_client.ServerLocation.Y == 5 && (_client.ServerLocation.X == 7 || _client.ServerLocation.X == 6))
                  _client.WalkWithinRange(6, 2, 2);
                else if (_client.ServerLocation.Y == 6 && (_client.ServerLocation.X == 7 || _client.ServerLocation.X == 6))
                {
                  _client.WalkWithinRange(6, 9, 2);
                }
                else
                {
                  Npc npcByName = _client.FindNpcByName<Npc>("Gramail");
                  if (npcByName != null)
                  {
                    if (!_client.gramail)
                    {
                      _client.DialogueRespond(new uint?(npcByName.ID), "Default");
                      _client.gramail = true;
                    }
                    if (_client.Tab.AscendOptions.instantascend.Checked && _client.SafeToWalkFast)
                    {
                      if (_client.Currentnpctext.StartsWith("Aisling, live true."))
                        _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)115, (byte)0, (byte)2, (byte)1, (byte)1);
                      if (_client.Currentnpctext.StartsWith("You will transform your Work"))
                        _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)115, (byte)0, (byte)28);
                      if (_client.Currentnpctext.StartsWith("((It costs "))
                      {
                        uint num8 = uint.Parse(_client.Currentnpctext.Split(' ')[2].ToString());
                        _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)115, (byte)0, (byte)30);
                        uint num9 = 0;
                        ulong num10 = 0;
                        while (true)
                        {
                          num10 += (ulong)(num8 * 500U);
                          if (num10 <= (ulong)_client.Statistics.Experience)
                          {
                            num8 += 25U;
                            ++num9;
                          }
                          else
                            break;
                        }
                        for (uint index = 0; index < num9; ++index)
                        {
                          _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)115, (byte)0, (byte)47, (byte)1, (byte)2);
                          _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)115, (byte)0, (byte)73, (byte)1, (byte)2);
                          Thread.Sleep(10);
                        }
                        Thread.Sleep(1000);
                        _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)115, (byte)0, (byte)43);
                        _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)115, (byte)0, (byte)78, (byte)1, (byte)3);
                        _client.Tab.AscendOptions.ascendbutton.Text = "Start";
                        Thread.Sleep(1000);
                      }
                    }
                    else
                    {
                      if (_client.Currentnpctext.StartsWith("Aisling, live true."))
                        _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)115, (byte)0, (byte)2, (byte)1, (byte)1);
                      if (_client.Currentnpctext.StartsWith("You will transform your Work"))
                        _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)115, (byte)0, (byte)28);
                      if (_client.Currentnpctext.StartsWith("((It costs "))
                        _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)115, (byte)0, (byte)30);
                      if (_client.Currentnpctext.StartsWith("You remember that you cannot"))
                        _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)115, (byte)0, (byte)47, (byte)1, (byte)2);
                      if (_client.Currentnpctext.StartsWith("Do you prostrate yourself"))
                        _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)115, (byte)0, (byte)73, (byte)1, (byte)2);
                      if (_client.Currentnpctext.StartsWith("You lack experience"))
                        _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)115, (byte)0, (byte)43);
                      if (_client.Currentnpctext.StartsWith("Do you now descend"))
                      {
                        _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)115, (byte)0, (byte)78, (byte)1, (byte)3);
                        _client.Tab.AscendOptions.ascendbutton.Text = "Start";
                      }
                    }
                  }
                }
              }
            }
          }
          if (!_client.pause && _client.Tab.vrescueascender)
          {
            Player characterByName = _client.FindCharacterByName<Player>(_client.Tab.vrescueascendername);
            if (characterByName != null && characterByName.IsOnScreen && characterByName.Location.DistanceFrom(_client.ServerLocation) == 1 && _client.CanSkill("Rescue"))
              _client.UseSkill("Rescue", characterByName.ID);
          }
          if (!_client.pause && _client.Tab.AscendOptions.vbuystatsbtn)
          {
            if (_client.HasItem("Warranty Bag"))
            {
              _client.SendMessage("Deposit your Warranty Bag!");
              _client.Tab.AscendOptions.buystatsbtn.Text = "Start";
              goto label_3045;
            }
            else
            {
              if (_client.MapInfo.Number == 435)
              {
                if (_client.ascendtime != DateTime.MinValue && DateTime.Now.Subtract(_client.ascendtime).TotalMinutes < 210.0)
                {
                  if (_client.ServerLocation.Y < 27 && _client.ServerLocation.Y > 17 && _client.ServerLocation.X < 6)
                    _client.WalkToExact(4, 20);
                  else
                    _client.WalkToExact(6, 23);
                }
                else
                {
                  _client.SendMessage("You cannot ascend. (Drop a hair)");
                  _client.Tab.AscendOptions.buystatsbtn.Text = "Start";
                }
              }
              if (_client.MapInfo.Number == 3085)
                _client.WalkToExact(10, 13);
              if (_client.MapInfo.Number == 3086)
              {
                if (_client.ServerLocation.Y == 5 && (_client.ServerLocation.X == 7 || _client.ServerLocation.X == 6))
                  _client.WalkWithinRange(6, 2, 2);
                else if (_client.ServerLocation.Y == 6 && (_client.ServerLocation.X == 7 || _client.ServerLocation.X == 6))
                {
                  _client.WalkWithinRange(6, 9, 2);
                }
                else
                {
                  Npc npcByName = _client.FindNpcByName<Npc>("Deoch");
                  if (npcByName != null)
                  {
                    if (!_client.deoch)
                    {
                      _client.DialogueRespond(new uint?(npcByName.ID), "Default");
                      _client.deoch = true;
                    }
                    if (_client.Currentnpctext.StartsWith("Aisling, do."))
                      _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)114, (byte)0, (byte)2, (byte)1, (byte)1);
                    if (_client.Currentnpctext.StartsWith("You will transform your Work"))
                      _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)114, (byte)0, (byte)28);
                    if (_client.Currentnpctext.StartsWith("((It costs "))
                      _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)114, (byte)0, (byte)30);
                    if (_client.Currentnpctext.StartsWith("You remember that you cannot"))
                    {
                      if ((Decimal)_client.Statistics.MaximumHP < (Decimal)_client.pathmaxhp + 150M * _client.Tab.AscendOptions.buynum.Value)
                        _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)114, (byte)0, (byte)51, (byte)1, (byte)2);
                      else
                        _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)114, (byte)0, (byte)51, (byte)1, (byte)1);
                    }
                    if (_client.Currentnpctext.StartsWith("Do you prostrate yourself"))
                      _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)114, (byte)0, (byte)80, (byte)1, (byte)2);
                    if (_client.Currentnpctext.StartsWith("You lack experience"))
                      _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)114, (byte)0, (byte)47);
                    if (_client.Currentnpctext.StartsWith("Do you now descend"))
                    {
                      _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)114, (byte)0, (byte)85, (byte)1, (byte)3);
                      _client.Tab.AscendOptions.ascendbutton.Text = "Start";
                    }
                  }
                }
              }
              if (_client.MapInfo.Number == 393 && !_client.needsmats)
              {
                if (Math.Floor((double)(_client.Statistics.MaximumHP - _client.pathmaxhp) / 150.0) > 0.0)
                {
                  Player characterByName = _client.FindCharacterByName<Player>(_client.Tab.AscendOptions.rescuername.Text);
                  if (characterByName != null && characterByName.IsOnScreen && characterByName.Location.DistanceFrom(_client.ServerLocation) > 1)
                  {
                    _client.WalkToPlayer(characterByName.Location.X, characterByName.Location.Y, 1);
                  }
                  else
                  {
                    Npc npcByName = _client.FindNpcByName<Npc>("Aoife");
                    if (npcByName != null)
                    {
                      if (!_client.habab)
                      {
                        _client.DialogueRespond(new uint?(npcByName.ID), "Master Stats");
                        _client.habab = true;
                      }
                      if (_client.Currentnpctext.StartsWith("So, Master Aisling,"))
                        _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)157, (byte)0, (byte)38);
                      if (_client.Currentnpctext.StartsWith("Realize that it is through"))
                        _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)157, (byte)0, (byte)39);
                      if (_client.Currentnpctext.StartsWith("And you will suffer"))
                        _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)157, (byte)0, (byte)40);
                      if (_client.Currentnpctext.StartsWith("Do you wish to"))
                        _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)157, (byte)0, (byte)41, (byte)1, (byte)1);
                      if (_client.Currentnpctext.StartsWith("You are too weak"))
                      {
                        _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)157, (byte)0, (byte)122);
                        _client.Tab.AscendOptions.buystatsbtn.Text = "Start";
                        goto label_3045;
                      }
                      else if (_client.Currentnpctext.StartsWith("One of your abilities"))
                      {
                        _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)157, (byte)0, (byte)230);
                        _client.Tab.AscendOptions.buystatsbtn.Text = "Start";
                        goto label_3045;
                      }
                      else if (_client.Currentnpctext.StartsWith("Which Attribute"))
                      {
                        byte action = 0;
                        byte un = 0;
                        if (_client.myPath == (byte)3)
                          action = (byte)186;
                        else if (_client.myPath == (byte)4)
                          action = (byte)237;
                        else if (_client.myPath == (byte)1)
                          action = (byte)84;
                        else if (_client.myPath == (byte)2)
                          action = (byte)135;
                        else if (_client.myPath == (byte)5)
                        {
                          action = (byte)45;
                          un = (byte)1;
                        }
                        if (_client.Tab.AscendOptions.strt.Text != string.Empty && int.Parse(_client.Tab.AscendOptions.strt.Text) > 0 && (long)int.Parse(_client.Tab.AscendOptions.strt.Text) < (long)_client.pathstr)
                        {
                          _client.Tab.AscendOptions.strl.Text = (int.Parse(_client.Tab.AscendOptions.strl.Text) + 1).ToString();
                          _client.Tab.AscendOptions.strt.Text = (int.Parse(_client.Tab.AscendOptions.strt.Text) - 1).ToString();
                          _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)157, un, action, (byte)1, (byte)1);
                        }
                        else if (_client.Tab.AscendOptions.intt.Text != string.Empty && int.Parse(_client.Tab.AscendOptions.intt.Text) > 0 && (long)int.Parse(_client.Tab.AscendOptions.intt.Text) < (long)_client.pathint)
                        {
                          _client.Tab.AscendOptions.intl.Text = (int.Parse(_client.Tab.AscendOptions.intl.Text) + 1).ToString();
                          _client.Tab.AscendOptions.intt.Text = (int.Parse(_client.Tab.AscendOptions.intt.Text) - 1).ToString();
                          _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)157, un, action, (byte)1, (byte)3);
                        }
                        else if (_client.Tab.AscendOptions.wist.Text != string.Empty && int.Parse(_client.Tab.AscendOptions.wist.Text) > 0 && (long)int.Parse(_client.Tab.AscendOptions.wist.Text) < (long)_client.pathwis)
                        {
                          _client.Tab.AscendOptions.wisl.Text = (int.Parse(_client.Tab.AscendOptions.wisl.Text) + 1).ToString();
                          _client.Tab.AscendOptions.wist.Text = (int.Parse(_client.Tab.AscendOptions.wist.Text) - 1).ToString();
                          _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)157, un, action, (byte)1, (byte)4);
                        }
                        else if (_client.Tab.AscendOptions.cont.Text != string.Empty && int.Parse(_client.Tab.AscendOptions.cont.Text) > 0 && (long)int.Parse(_client.Tab.AscendOptions.cont.Text) < (long)_client.pathcon)
                        {
                          _client.Tab.AscendOptions.conl.Text = (int.Parse(_client.Tab.AscendOptions.conl.Text) + 1).ToString();
                          _client.Tab.AscendOptions.cont.Text = (int.Parse(_client.Tab.AscendOptions.cont.Text) - 1).ToString();
                          _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)157, un, action, (byte)1, (byte)2);
                        }
                        else if (_client.Tab.AscendOptions.dext.Text != string.Empty && int.Parse(_client.Tab.AscendOptions.dext.Text) > 0 && (long)int.Parse(_client.Tab.AscendOptions.dext.Text) < (long)_client.pathdex)
                        {
                          _client.Tab.AscendOptions.dexl.Text = (int.Parse(_client.Tab.AscendOptions.dexl.Text) + 1).ToString();
                          _client.Tab.AscendOptions.dext.Text = (int.Parse(_client.Tab.AscendOptions.dext.Text) - 1).ToString();
                          _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)157, un, action, (byte)1, (byte)5);
                        }
                      }
                    }
                  }
                }
                else
                {
                  if (!(_client.ascendtime == DateTime.MinValue))
                  {
                    TimeSpan timeSpan = DateTime.Now.Subtract(_client.ascendtime);
                    if (timeSpan.TotalMinutes <= 210.0)
                    {
                      if (_client.Tab.AscendOptions.canaffordonestat())
                      {
                        Player characterByName = _client.FindCharacterByName<Player>(_client.Tab.AscendOptions.rescuername.Text);
                        if (characterByName != null && characterByName.IsOnScreen && characterByName.Location.DistanceFrom(_client.ServerLocation) > 1)
                        {
                          _client.WalkToPlayer(characterByName.Location.X, characterByName.Location.Y, 1);
                          goto label_975;
                        }
                        else if (characterByName != null && characterByName.IsOnScreen && characterByName.Location.DistanceFrom(_client.ServerLocation) == 1)
                        {
                          if (_client.ascendtime != DateTime.MinValue)
                          {
                            timeSpan = DateTime.Now.Subtract(_client.ascendtime);
                            if (timeSpan.TotalMinutes < 210.0)
                            {
                              if (_client.rescuedtime != DateTime.MinValue)
                              {
                                timeSpan = DateTime.UtcNow.Subtract(_client.rescuedtime);
                                if (timeSpan.TotalSeconds < 30.0)
                                {
                                  _client.SendMessage("", (byte)18);
                                  if (_client.HasItem("Hemloch"))
                                  {
                                    if (_client.HasItem("Wine"))
                                    {
                                      _client.UseItem("Hemloch");
                                      _client.UseItem("Wine");
                                      Thread.Sleep(2000);
                                      goto label_975;
                                    }
                                    else if (_client.HasItem("Rum"))
                                    {
                                      _client.UseItem("Hemloch");
                                      _client.UseItem("Rum");
                                      Thread.Sleep(2000);
                                      goto label_975;
                                    }
                                    else if (_client.HasItem("Brandy"))
                                    {
                                      _client.UseItem("Hemloch");
                                      _client.UseItem("Brandy");
                                      Thread.Sleep(2000);
                                      goto label_975;
                                    }
                                    else if (_client.Tab.AscendOptions.vwithdrawwine)
                                    {
                                      _client.needsmats = true;
                                      goto label_975;
                                    }
                                    else
                                    {
                                      _client.SendMessage("Out of Drink");
                                      if (_client.MapInfo.Number == 393)
                                      {
                                        _client.Tab.AscendOptions.buystatsbtn.Text = "Start";
                                        goto label_975;
                                      }
                                      else
                                        goto label_975;
                                    }
                                  }
                                  else if (_client.Tab.AscendOptions.vwithdrawhem)
                                  {
                                    _client.needsmats = true;
                                    goto label_975;
                                  }
                                  else
                                  {
                                    _client.SendMessage("Out of Hemloch");
                                    if (_client.MapInfo.Number == 393)
                                    {
                                      _client.Tab.AscendOptions.buystatsbtn.Text = "Start";
                                      goto label_975;
                                    }
                                    else
                                      goto label_975;
                                  }
                                }
                              }
                              _client.SendMessage("Waiting for Rescue", (byte)18);
                              goto label_975;
                            }
                          }
                          _client.SendMessage("Drop a hair");
                          _client.Tab.AscendOptions.buystatsbtn.Text = "Start";
                          goto label_975;
                        }
                        else
                          goto label_975;
                      }
                      else
                      {
                        _client.SendMessage("All done!");
                        _client.Tab.AscendOptions.buystatsbtn.Text = "Start";
                        goto label_975;
                      }
                    }
                  }
                  _client.SendMessage("You need to drop a Succubus Hair in Mileth Altar.");
                  _client.Tab.AscendOptions.buystatsbtn.Text = "Start";
                  goto label_3045;
                }
              }
              else if (_client.MapInfo.Number == 393 && _client.needsmats)
                _client.WalkToExact(11, 6);
              else if (_client.MapInfo.Number == 500 && _client.needsmats)
                _client.WalkToExact(54, 69);
              else if (_client.MapInfo.Number == 135 && _client.needsmats)
              {
                if (_client.ItemAmount("Wine") == 0U)
                {
                  _client.Withdraw("Wine", 15);
                  _client.needsmats = false;
                }
                if (_client.ItemAmount("Hemloch") == 0U)
                {
                  _client.Withdraw("Hemloch", 30);
                  _client.needsmats = false;
                }
              }
              else if (_client.MapInfo.Number == 135)
                _client.WalkToExact(5, 11);
              else if (_client.MapInfo.Number == 500)
                _client.WalkToExact(93, 13);
              else if (_client.MapInfo.Number == 3006)
              {
                if (_client.ServerLocation.Y <= 9 && _client.ServerLocation.Y >= 4)
                  _client.WalkToExact(0, _client.ServerLocation.Y);
                else if (_client.ServerLocation.Y > 9)
                  _client.WalkToExact(0, 9);
                else if (_client.ServerLocation.Y < 4)
                  _client.WalkToExact(0, 4);
              }
              else if (_client.MapInfo.Number == 3079 && _client.ServerLocation.Y == 1)
                _client.WalkToExact(_client.ServerLocation.X, 2);
              else if (_client.MapInfo.Number == 3079 && _client.ServerLocation.Y == 5)
                _client.WalkToExact(_client.ServerLocation.X, 6);
            }
          }
        label_975:
          if (_client.buyfiorsrads && _client.Statistics.Gold > 500U)
          {
            Npc npcByName = _client.FindNpcByName<Npc>("Braz");
            if (npcByName != null && _client.ItemAmount("fior srad") < 30U)
            {
              _client.DialogueRespond(npcByName.ID, (byte)4, (byte)230);
              _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)230, (byte)0, (byte)6);
              for (int index = (int)_client.ItemAmount("fior srad"); index < 30; ++index)
              {
                _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)230, (byte)0, (byte)15, (byte)1, (byte)1);
                Thread.Sleep(50);
              }
              _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)230, (byte)0, (byte)14);
              _client.buyfiorsrads = false;
            }
          }
          if (_client.MapInfo.Number == 10055 && _client.Statistics.Gold > 1000U)
          {
            if (_client.Tab.vautobuykoms && _client.DistanceFrom(_client.poiloc) < 21 && DateTime.UtcNow.Subtract(_client.newmapdelay).TotalMilliseconds > (double)_client.RandomNumber(1200, 1500))
              _client.BuyItem("Komadium", 52U - _client.ItemCount("Komadium"));
            if (_client.Tab.vautobuyhems && (_client.DistanceFrom(_client.poiloc) < 21 || _client.DistanceFrom(_client.muettaloc) < 21))
              _client.BuyItem("Hemloch", 30U - _client.ItemCount("Hemloch"));
          }
          else if (_client.MapInfo.Number == 9382 && _client.Statistics.Gold > 1000U)
          {
            if (_client.Tab.vautobuykoms && _client.DistanceFrom(_client.sutonloc) < 21 && DateTime.UtcNow.Subtract(_client.newmapdelay).TotalMilliseconds > (double)_client.RandomNumber(1200, 1500))
              _client.BuyItem("Komadium", 52U - _client.ItemCount("Komadium"));
            if (_client.Tab.vautobuyhems && _client.DistanceFrom(_client.sutonloc) < 21)
              _client.BuyItem("Hemloch", 30U - _client.ItemCount("Hemloch"));
          }
          if (_client.losterbiedelay != DateTime.MinValue && DateTime.UtcNow.Subtract(_client.losterbiedelay).TotalSeconds > 180.0)
          {
            _client.losterbiedelay = DateTime.MinValue;
            _client.SendMessage("You can catch another erbie now.", "pink");
          }
          if (!_client.pause && _client.frostygift)
          {
            if (_client.Statistics.Level < 99)
            {
              _client.SendMessage("Not level 99, logging off in 5 seconds.", "red");
              _client.autowalkon = false;
              Thread.Sleep(5000);
              _client.LogOff();
            }
            if (!_client.autowalkon && _client.MapInfo.Number == 3271 && !_client.InventoryIsFull())
            {
              Npc npcByName5 = _client.FindNpcByName<Npc>("Frosty3");
              if (npcByName5 != null)
              {
                _client.ClickNpc(npcByName5.ID);
                while (_client.Currentnpctext == string.Empty)
                  Thread.Sleep(100);
                _client.PopupClose(new uint?(npcByName5.ID));
                _client.frostygift = false;
                Thread.Sleep(2000);
                _client.LogOff();
              }
              Npc npcByName6 = _client.FindNpcByName<Npc>("Nadia");
              if (npcByName6 != null)
              {
                _client.ClickNpc(npcByName6.ID);
                while (_client.Currentnpctext == string.Empty)
                  Thread.Sleep(100);
                _client.PopupNext(new uint?(npcByName6.ID));
                _client.PopupClose(new uint?(npcByName6.ID));
                _client.frostygift = false;
                Thread.Sleep(2000);
                _client.LogOff();
              }
              Npc npcByName7 = _client.FindNpcByName<Npc>("Chang");
              if (npcByName7 != null)
              {
                _client.ClickNpc(npcByName7.ID);
                while (_client.Currentnpctext == string.Empty)
                  Thread.Sleep(100);
                _client.PopupNext(new uint?(npcByName7.ID));
                _client.PopupNext(new uint?(npcByName7.ID));
                _client.PopupNext(new uint?(npcByName7.ID));
                _client.frostygift = false;
                Thread.Sleep(4000);
                _client.LogOff();
              }
            }
          }
          if (_client.HasItem("Yule Log") && _client.HasItem("fior srad") && !_client.autowalkon)
          {
            Npc npc = (Npc)null;
            if (_client.MapInfo.Number == 136 && !_client.yulemileth)
              npc = _client.FindNpcByName<Npc>("Riona");
            if (_client.MapInfo.Number == 169 && !_client.yuleabel)
              npc = _client.FindNpcByName<Npc>("Runa");
            if (_client.MapInfo.Number == 150 && !_client.yulepiet)
              npc = _client.FindNpcByName<Npc>("Saskia");
            if (_client.MapInfo.Number == 498 && !_client.yuleruc)
              npc = _client.FindNpcByName<Npc>("Maria");
            if (_client.MapInfo.Number == 1960 && !_client.yuletagor)
              npc = _client.FindNpcByName<Npc>("Dorina");
            if (_client.MapInfo.Number == 950 && !_client.yulesuomi)
              npc = _client.FindNpcByName<Npc>("Eeva");
            if (npc != null)
            {
              _client.Speak("Yule Log");
              while (_client.Currentnpctext == string.Empty)
                Thread.Sleep(100);
              if (_client.Currentnpctext.StartsWith("Thank you for the yule log, aisling"))
              {
                _client.PopupClose(new uint?(_client.CurrentnpcpopupID));
                if (npc.Name == "Riona")
                {
                  _client.yulemileth = true;
                  _client.SendMessage("mileth done");
                }
                else if (npc.Name == "Runa")
                {
                  _client.yuleabel = true;
                  _client.SendMessage("abel done");
                }
                else if (npc.Name == "Saskia")
                {
                  _client.yulepiet = true;
                  _client.SendMessage("piet done");
                }
                else if (npc.Name == "Maria")
                {
                  _client.yuleruc = true;
                  _client.SendMessage("rucesion done");
                }
                else if (npc.Name == "Dorina")
                {
                  _client.yuletagor = true;
                  _client.SendMessage("tagor done");
                }
                else if (npc.Name == "Eeva")
                {
                  _client.yulesuomi = true;
                  _client.SendMessage("suomi done");
                }
                if (!_client.yuleruc)
                {
                  _client.Tab.autowalker_locales.Text = "Rucesion";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Inn";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.Tab.fastwalk.Checked = true;
                  _client.pause = false;
                  _client.Tab.btnPlay.Enabled = false;
                  _client.Tab.btnStop.Enabled = true;
                }
                else if (!_client.yuleabel)
                {
                  _client.Tab.autowalker_locales.Text = "Abel";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Inn";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.Tab.fastwalk.Checked = true;
                  _client.pause = false;
                  _client.Tab.btnPlay.Enabled = false;
                  _client.Tab.btnStop.Enabled = true;
                }
                else if (!_client.yulepiet)
                {
                  _client.Tab.autowalker_locales.Text = "Piet";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Inn";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.Tab.fastwalk.Checked = true;
                  _client.pause = false;
                  _client.Tab.btnPlay.Enabled = false;
                  _client.Tab.btnStop.Enabled = true;
                }
                else if (!_client.yuletagor)
                {
                  _client.Tab.autowalker_locales.Text = "Tagor";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Inn";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.Tab.fastwalk.Checked = true;
                  _client.pause = false;
                  _client.Tab.btnPlay.Enabled = false;
                  _client.Tab.btnStop.Enabled = true;
                }
                else if (!_client.yulemileth)
                {
                  if (_client.HasItem("Abel Song") && _client.MapInfo.Number == 1960)
                  {
                    _client.UseItem("Abel Song");
                    Thread.Sleep(1000);
                  }
                  else if (_client.HasItem("Mileth Song") && _client.MapInfo.Number == 1960)
                  {
                    _client.UseItem("Mileth Song");
                    Thread.Sleep(1000);
                  }
                  _client.Tab.autowalker_locales.Text = "Mileth";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Inn";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.Tab.fastwalk.Checked = true;
                  _client.pause = false;
                  _client.Tab.btnPlay.Enabled = false;
                  _client.Tab.btnStop.Enabled = true;
                }
                else if (!_client.yulesuomi)
                {
                  if (_client.HasItem("Suomi Song"))
                    _client.UseItem("Suomi Song");
                  _client.Tab.autowalker_locales.Text = "Suomi";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Inn";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.Tab.fastwalk.Checked = true;
                  _client.pause = false;
                  _client.Tab.btnPlay.Enabled = false;
                  _client.Tab.btnStop.Enabled = true;
                }
                else
                {
                  _client.Tab.autowalker_locales.Text = "Suomi";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Weapon Shop";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.Tab.fastwalk.Checked = true;
                  _client.pause = false;
                  _client.Tab.btnPlay.Enabled = false;
                  _client.Tab.btnStop.Enabled = true;
                  _client.yulequest = true;
                  _client.LastnpcpopupID = 0U;
                }
              }
              if (_client.Currentnpctext.StartsWith("*shivers* Aisling,"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 8; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                if (npc.Name == "Riona")
                {
                  _client.yulemileth = true;
                  _client.SendMessage("mileth done");
                }
                else if (npc.Name == "Runa")
                {
                  _client.yuleabel = true;
                  _client.SendMessage("abel done");
                }
                else if (npc.Name == "Saskia")
                {
                  _client.yulepiet = true;
                  _client.SendMessage("piet done");
                }
                else if (npc.Name == "Maria")
                {
                  _client.yuleruc = true;
                  _client.SendMessage("rucesion done");
                }
                else if (npc.Name == "Dorina")
                {
                  _client.yuletagor = true;
                  _client.SendMessage("tagor done");
                }
                else if (npc.Name == "Eeva")
                {
                  _client.yulesuomi = true;
                  _client.SendMessage("suomi done");
                }
                if (!_client.yuleruc)
                {
                  _client.Tab.autowalker_locales.Text = "Rucesion";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Inn";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.Tab.fastwalk.Checked = true;
                  _client.pause = false;
                  _client.Tab.btnPlay.Enabled = false;
                  _client.Tab.btnStop.Enabled = true;
                }
                else if (!_client.yuleabel)
                {
                  _client.Tab.autowalker_locales.Text = "Abel";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Inn";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.Tab.fastwalk.Checked = true;
                  _client.pause = false;
                  _client.Tab.btnPlay.Enabled = false;
                  _client.Tab.btnStop.Enabled = true;
                }
                else if (!_client.yulepiet)
                {
                  _client.Tab.autowalker_locales.Text = "Piet";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Inn";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.Tab.fastwalk.Checked = true;
                  _client.pause = false;
                  _client.Tab.btnPlay.Enabled = false;
                  _client.Tab.btnStop.Enabled = true;
                }
                else if (!_client.yuletagor)
                {
                  _client.Tab.autowalker_locales.Text = "Tagor";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Inn";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.Tab.fastwalk.Checked = true;
                  _client.pause = false;
                  _client.Tab.btnPlay.Enabled = false;
                  _client.Tab.btnStop.Enabled = true;
                }
                else if (!_client.yulemileth)
                {
                  if (_client.HasItem("Abel Song") && _client.MapInfo.Number == 1960)
                  {
                    _client.UseItem("Abel Song");
                    Thread.Sleep(1000);
                  }
                  else if (_client.HasItem("Mileth Song") && _client.MapInfo.Number == 1960)
                  {
                    _client.UseItem("Mileth Song");
                    Thread.Sleep(1000);
                  }
                  _client.Tab.autowalker_locales.Text = "Mileth";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Inn";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.Tab.fastwalk.Checked = true;
                  _client.pause = false;
                  _client.Tab.btnPlay.Enabled = false;
                  _client.Tab.btnStop.Enabled = true;
                }
                else if (!_client.yulesuomi)
                {
                  if (_client.HasItem("Suomi Song"))
                  {
                    _client.UseItem("Suomi Song");
                    Thread.Sleep(1000);
                  }
                  _client.Tab.autowalker_locales.Text = "Suomi";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Inn";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.Tab.fastwalk.Checked = true;
                  _client.pause = false;
                  _client.Tab.btnPlay.Enabled = false;
                  _client.Tab.btnStop.Enabled = true;
                }
                else
                {
                  _client.Tab.autowalker_locales.Text = "Suomi";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Weapon Shop";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.Tab.fastwalk.Checked = true;
                  _client.pause = false;
                  _client.Tab.btnPlay.Enabled = false;
                  _client.Tab.btnStop.Enabled = true;
                  _client.yulequest = true;
                  _client.LastnpcpopupID = 0U;
                }
              }
            }
          }
          if (_client.MapInfo.Number == 410 && !_client.autowalkon && !_client.pause)
          {
            Npc npcByName = _client.FindNpcByName<Npc>("Torbjorn");
            if (npcByName != null && (int)_client.LastnpcpopupID != (int)npcByName.ID && _client.yulequest)
            {
              _client.DialogueRespond(npcByName.ID, (byte)11, (byte)67);
              _client.LastnpcpopupID = npcByName.ID;
            }
          }
          if (_client.Currentnpctext.StartsWith("Aisling, do you feel the cold winter wind?"))
          {
            uint currentnpcpopupId = _client.CurrentnpcpopupID;
            _client.PopupNext(new uint?(currentnpcpopupId));
            _client.PopupNext(new uint?(currentnpcpopupId));
            _client.PopupNext(new uint?(currentnpcpopupId));
            _client.PopupNext(new uint?(currentnpcpopupId));
            _client.PopupNext(new uint?(currentnpcpopupId));
            _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
            _client.PopupNext(new uint?(currentnpcpopupId));
            _client.PopupNext(new uint?(currentnpcpopupId));
            _client.PopupNext(new uint?(currentnpcpopupId));
            _client.PopupNext(new uint?(currentnpcpopupId));
            _client.PopupClose(new uint?(currentnpcpopupId));
            _client.SendMessage("Go to Mount Giragan and get logs");
            _client.Tab.autowalker_locales.Text = "Mount Giragan";
            _client.Tab.walklocaleslist.SelectedItem = (object)"Mtg 1";
            _client.Tab.autowalker_button.Text = "Stop";
            _client.autowalkon = true;
            _client.Tab.mediumwalk.Checked = true;
            _client.pause = false;
            _client.Tab.btnPlay.Enabled = false;
            _client.Tab.btnStop.Enabled = true;
            Thread.Sleep(1000);
            if (_client.HasItem("Torbjorn's Axe"))
              _client.UseItem("Torbjorn's Axe");
          }
          if (_client.Currentnpctext.StartsWith("Excellent! Now, here. Take this "))
          {
            uint currentnpcpopupId = _client.CurrentnpcpopupID;
            _client.PopupNext(new uint?(currentnpcpopupId));
            _client.PopupNext(new uint?(currentnpcpopupId));
            _client.PopupNext(new uint?(currentnpcpopupId));
            _client.PopupClose(new uint?(currentnpcpopupId));
            if (_client.HasItem("fior srad") && _client.ItemAmount("fior srad") >= 6U)
            {
              _client.SendMessage("Walking to inns");
              _client.Tab.autowalker_locales.Text = "Suomi";
              _client.Tab.walklocaleslist.SelectedItem = (object)"Inn";
              _client.Tab.autowalker_button.Text = "Stop";
              _client.autowalkon = true;
            }
            else
              _client.SendMessage("Get fiors and go to inns");
          }
          if (_client.Currentnpctext.StartsWith("Aisling, you have shown such generosity"))
          {
            uint currentnpcpopupId = _client.CurrentnpcpopupID;
            for (int index = 0; index < 5; ++index)
            {
              _client.PopupNext(new uint?(currentnpcpopupId));
              Thread.Sleep(10);
            }
            _client.SaveTimedStuff(35);
            _client.yulequest = false;
            _client.SendMessage("Yule Quest Complete!");
          }
          if (_client.Currentnpctext.StartsWith("The inns of Temuair and the people inside are warm"))
          {
            uint currentnpcpopupId = _client.CurrentnpcpopupID;
            _client.PopupNext(new uint?(currentnpcpopupId));
            _client.PopupNext(new uint?(currentnpcpopupId));
            _client.PopupClose(new uint?(currentnpcpopupId));
            _client.SendMessage("Can't do it yet");
          }
          if (!_client.pause && _client.claimsunprotection && _client.MapInfo.Number == 3271 && !_client.autowalkon && !_client.InventoryIsFull())
          {
            Npc npcByName = _client.FindNpcByName<Npc>("Francis");
            if (npcByName != null && npcByName.IsOnScreen)
            {
              _client.DialogueRespond(npcByName.ID, (byte)11, (byte)79);
              _client.PopupNext(new uint?(npcByName.ID));
              _client.PopupNext(new uint?(npcByName.ID));
              Thread.Sleep(1000);
              _client.claimsunprotection = false;
            }
          }
          if (!_client.pause && _client.learnswim && _client.MapInfo.Number == 136 && !_client.autowalkon)
          {
            Npc npcByName = _client.FindNpcByName<Npc>("Riona");
            if (npcByName != null && npcByName.IsOnScreen)
            {
              _client.DialogueRespond(npcByName.ID, (byte)10, (byte)122);
              _client.PopupNext(new uint?(npcByName.ID));
              Thread.Sleep(1000);
              _client.learnswim = false;
              _client.Tab.autowalker_locales.SelectedItem = (object)"Lynith";
              _client.Tab.walklocaleslist.SelectedItem = (object)"Paradise";
              _client.Tab.autowalker_button.Text = "Stop";
              _client.autowalkon = true;
            }
          }
          if (!_client.pause && _client.claimbeachattire && _client.MapInfo.Number == 7900 && !_client.autowalkon && !_client.InventoryIsFull())
          {
            Npc npcByName = _client.FindNpcByName<Npc>("Ruba");
            if (npcByName != null && npcByName.IsOnScreen)
            {
              _client.DialogueRespond(npcByName.ID, (byte)12, (byte)116);
              _client.PopupNext(new uint?(npcByName.ID));
              _client.PopupNext(new uint?(npcByName.ID));
              Thread.Sleep(1000);
              _client.claimbeachattire = false;
            }
          }
          if (!_client.pause && !_client.autowalkon && _client.MapInfo.Number == 192 && _client.makeawish)
          {
            Npc npcByName = _client.FindNpcByName<Npc>("Naomhan");
            if (npcByName != null)
            {
              _client.DialogueRespond(npcByName.ID, (byte)11, (byte)102);
              for (int index = 0; index < 17; ++index)
              {
                _client.PopupNext(new uint?(npcByName.ID));
                Thread.Sleep(10);
              }
              _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
              _client.PopupNext(new uint?(npcByName.ID));
              _client.PopupNext(new uint?(npcByName.ID));
              _client.PopupClose(new uint?(npcByName.ID));
              _client.SendMessage("Go to fountain and pick your wish", "orange");
              _client.makeawish = false;
              Thread.Sleep(1000);
            }
          }
          if (_client.MapInfo.Number == 3029)
          {
            if (_client.Currentnpctext.StartsWith("Welcome to Dark Ages : "))
            {
              _client.PopupClose(new uint?(_client.CurrentnpcpopupID), (byte)4);
              _client.Tab.fastwalk.Checked = true;
              _client.Tab.usemonster.Checked = true;
              _client.walktut = true;
              //if (!_client.BotThread.IsAlive)
              //  _client.BotThread.Start();
              _client.pause = false;
              _client.Tab.btnPlay.Enabled = false;
              _client.Tab.btnStop.Enabled = true;
              Thread.Sleep(1000);
            }
            if (!_client.pause && _client.walktut && _client.Currentnpctext == "" && (_client.ServerLocation.X != 49 || _client.ServerLocation.Y != 30))
              _client.WalkToExact(49, 30);
            if (_client.Currentnpctext.StartsWith("You are about to leave the "))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)18, (byte)0, (byte)2, (byte)4);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)18, (byte)0, (byte)3, (byte)4);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)18, (byte)0, (byte)11, (byte)1, (byte)1, (byte)4);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)18, (byte)0, (byte)19, (byte)4);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)18, (byte)0, (byte)49, (byte)4);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)18, (byte)0, (byte)109, (byte)1, (byte)2, (byte)4);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)18, (byte)0, (byte)42, (byte)4);
              Thread.Sleep(1000);
            }
          }
          if (_client.MapInfo.Number == 3000)
          {
            if (!_client.pause && _client.walktut && _client.Currentnpctext == "" && (_client.ServerLocation.X != 2 || _client.ServerLocation.Y != 5))
              _client.WalkToExact(2, 5);
            if (_client.Currentnpctext.StartsWith("You rub your eyes..."))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)174, (byte)0, (byte)52, (byte)4);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)174, (byte)0, (byte)66, (byte)4);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)174, (byte)0, (byte)69, (byte)4);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)174, (byte)0, (byte)72, (byte)4);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)174, (byte)0, (byte)75, (byte)4);
              Thread.Sleep(1000);
              _client.walktut = false;
            }
            if (!_client.pause && !_client.walktut && _client.Currentnpctext == "" && (_client.ServerLocation.X != 4 || _client.ServerLocation.Y != 7))
            {
              _client.WalkToExact(4, 7);
              _client.walktut = true;
            }
          }
          if (_client.MapInfo.Number == 136 && _client.walktut)
          {
            Npc npcByName = _client.FindNpcByName<Npc>("Riona");
            if (npcByName != null)
            {
              _client.DialogueRespond(npcByName.ID, (byte)4, (byte)185);
              _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)185, (byte)0, (byte)26);
              _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)185, (byte)0, (byte)27);
              _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)185, (byte)0, (byte)28);
              _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)185, (byte)0, (byte)29);
              _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)185, (byte)0, (byte)31, (byte)1, (byte)2);
              _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)185, (byte)0, (byte)41);
              _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)185, (byte)0, (byte)42, (byte)1, (byte)2);
              _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)185, (byte)0, (byte)52);
              _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)185, (byte)0, (byte)54);
              _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)185, (byte)0, (byte)55);
              Thread.Sleep(1000);
              _client.DialogueRespond(npcByName.ID, (byte)4, (byte)185);
              _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)185, (byte)0, (byte)31, (byte)1, (byte)2);
              _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)185, (byte)0, (byte)41);
              _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)185, (byte)0, (byte)42, (byte)1, (byte)2);
              _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)185, (byte)0, (byte)52);
              _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)185, (byte)0, (byte)54);
              _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)185, (byte)0, (byte)55);
              _client.walktut = false;
              Thread.Sleep(1000);
            }
          }
          if (!_client.pause && !_client.autowalkon)
          {
            if (_client.warrior && _client.MapInfo.Number == 347)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Neal");
              if (npcByName != null && npcByName.IsOnScreen)
              {
                _client.DialogueRespond(npcByName.ID, (byte)4, (byte)160);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)160, (byte)0, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)160, (byte)0, (byte)60, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)160, (byte)0, (byte)173);
                _client.rogue = false;
              }
            }
            if (_client.rogue && _client.MapInfo.Number == 346)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Keefe");
              if (npcByName != null && npcByName.IsOnScreen)
              {
                _client.DialogueRespond(npcByName.ID, (byte)4, (byte)161);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)161, (byte)0, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)161, (byte)0, (byte)57, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)161, (byte)0, (byte)170);
                _client.rogue = false;
              }
            }
            if (_client.monk && _client.MapInfo.Number == 348)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Donnan");
              if (npcByName != null && npcByName.IsOnScreen)
              {
                _client.DialogueRespond(npcByName.ID, (byte)4, (byte)164);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)164, (byte)0, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)164, (byte)0, (byte)60, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)164, (byte)0, (byte)174);
                _client.monk = false;
              }
            }
            if (_client.priest && _client.MapInfo.Number == 345)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Erin");
              if (npcByName != null && npcByName.IsOnScreen)
              {
                _client.DialogueRespond(npcByName.ID, (byte)4, (byte)163);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)163, (byte)0, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)163, (byte)0, (byte)60, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)163, (byte)0, (byte)174);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)163, (byte)0, (byte)175);
                _client.rogue = false;
              }
            }
            if (_client.wizard && _client.MapInfo.Number == 398)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Logan");
              if (npcByName != null && npcByName.IsOnScreen)
              {
                _client.DialogueRespond(npcByName.ID, (byte)4, (byte)162);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)162, (byte)0, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)162, (byte)0, (byte)63, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)162, (byte)0, (byte)176);
                _client.rogue = false;
              }
            }
          }
          if (!_client.pause && _client.Tab.altar.Checked)
          {
            if (_client.Currentnpctext.StartsWith("There are spaces in between time,"))
            {
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)0, (byte)37, (byte)0, (byte)64, (byte)1, (byte)1, (byte)4);
              Thread.Sleep(1000);
            }
            else if (_client.Currentnpctext != string.Empty)
            {
              User32._SendKeys(_client.mainProc.MainWindowHandle, "Space");
              Thread.Sleep(1000);
            }
          }
          if (!_client.pause && _client.Tab.altar.Checked && (_client.altartimer == DateTime.MinValue || DateTime.Now.Subtract(_client.altartimer).TotalHours >= 3.01))
          {
            if (_client.MapInfo.Number == 500)
            {
              if (_client.HasItem("Wine"))
              {
                if (_client.WithinRange(31, 53, 3))
                {
                  _client.Drop(31, 53, _client.ItemSlot("Wine"), 1);
                  Thread.Sleep(1000);
                }
                else
                  _client.WalkWithinRange(31, 53, 3);
              }
              else
                _client.WalkToExact(54, 69);
            }
            if (_client.MapInfo.Number == 135)
            {
              if (!_client.HasItem("Wine"))
              {
                _client.Withdraw("Wine", 15);
                Thread.Sleep(1000);
              }
              else
                _client.WalkToExact(5, 11);
            }
          }
          if (_client.slabquest && _client.MapInfo.Number == 10266 && !_client.autowalkon)
          {
            Npc npcByName = _client.FindNpcByName<Npc>("Cheung");
            if (npcByName != null && npcByName.IsOnScreen)
            {
              _client.Speak("marble slab");
              while (_client.Currentnpctext == string.Empty)
                Thread.Sleep(100);
            }
            if (_client.Currentnpctext.StartsWith("I don't have any, but I know where you can find some."))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              for (int index = 0; index < 6; ++index)
              {
                _client.PopupNext(new uint?(currentnpcpopupId));
                Thread.Sleep(10);
              }
              Thread.Sleep(1000);
              _client.slabquest = false;
              _client.Tab.autowalker_locales.SelectedItem = (object)"Lost Ruins";
              _client.Tab.walklocaleslist.SelectedItem = (object)"Nairn";
              _client.Tab.autowalker_button.Text = "Stop";
              _client.autowalkon = true;
            }
            else if (_client.Currentnpctext.Equals("You seem confused."))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              for (int index = 0; index < 3; ++index)
              {
                _client.PopupNext(new uint?(currentnpcpopupId));
                Thread.Sleep(10);
              }
              Thread.Sleep(1000);
              _client.slabquest = false;
              _client.Tab.autowalker_locales.SelectedItem = (object)"Lost Ruins";
              _client.Tab.walklocaleslist.SelectedItem = (object)"Nairn";
              _client.Tab.autowalker_button.Text = "Stop";
              _client.autowalkon = true;
            }
          }
          if (_client.darkmaze && !_client.autowalkon)
          {
            if (_client.MapInfo.Number == 378)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Marlin");
              if (npcByName != null && npcByName.IsOnScreen && (int)_client.LastnpcpopupID != (int)npcByName.ID)
              {
                if (_client.darkmazequest == 0)
                {
                  _client.DialogueRespond(npcByName.ID, (byte)6, (byte)104);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)104, (byte)0, (byte)20, (byte)1, (byte)1);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)104, (byte)0, (byte)37, (byte)1, (byte)1);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)104, (byte)0, (byte)41);
                  Thread.Sleep(1000);
                  _client.DialogueRespond(npcByName.ID, (byte)6, (byte)104);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)104, (byte)0, (byte)20, (byte)1, (byte)1);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)104, (byte)0, (byte)37, (byte)1, (byte)2);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)104, (byte)0, (byte)44);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)104, (byte)0, (byte)51);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)104, (byte)0, (byte)51);
                  Thread.Sleep(1000);
                  _client.DialogueRespond(npcByName.ID, (byte)6, (byte)104);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)104, (byte)0, (byte)20, (byte)1, (byte)2);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)104, (byte)0, (byte)27);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)104, (byte)0, (byte)29, (byte)1, (byte)1);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)104, (byte)0, (byte)55, (byte)1, (byte)1);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)104, (byte)0, (byte)65);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)104, (byte)0, (byte)55, (byte)1, (byte)2);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)104, (byte)0, (byte)62);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)104, (byte)0, (byte)55, (byte)1, (byte)3);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)104, (byte)0, (byte)69);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)104, (byte)0, (byte)55, (byte)1, (byte)4);
                  _client.darkmazequest = 1;
                  Thread.Sleep(1000);
                  _client.Tab.autowalker_locales.SelectedItem = (object)"Loures";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Jean";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                }
                if (_client.darkmazequest == 3)
                {
                  _client.DialogueRespond(npcByName.ID, (byte)6, (byte)104);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)104, (byte)0, (byte)84, (byte)1, (byte)1);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)104, (byte)0, (byte)92, (byte)1, (byte)2);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)104, (byte)0, (byte)105, (byte)1, (byte)3);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)104, (byte)0, (byte)121, (byte)1, (byte)2);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)104, (byte)0, (byte)130);
                  _client.darkmazequest = 4;
                  _client.darkmaze = false;
                  Thread.Sleep(1000);
                  _client.Tab.autowalker_locales.SelectedItem = (object)"Loures";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Dark Maze";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                }
              }
            }
            if (_client.MapInfo.Number == 124)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Jean");
              if (npcByName != null && npcByName.IsOnScreen && (int)_client.LastnpcpopupID != (int)npcByName.ID && _client.darkmazequest == 1)
              {
                _client.DialogueRespond(npcByName.ID, (byte)6, (byte)105);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)105, (byte)0, (byte)3, (byte)1, (byte)2);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)105, (byte)0, (byte)20);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)105, (byte)0, (byte)21);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)105, (byte)0, (byte)22);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)105, (byte)0, (byte)36, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)105, (byte)0, (byte)40);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)105, (byte)0, (byte)41, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)105, (byte)0, (byte)52, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)105, (byte)0, (byte)56);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)105, (byte)0, (byte)57);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)105, (byte)0, (byte)36, (byte)1, (byte)2);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)105, (byte)0, (byte)61, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)105, (byte)0, (byte)66);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)2, (byte)105, (byte)0, (byte)35);
                _client.darkmazequest = 2;
                Thread.Sleep(1000);
                if (_client.HasItem("Loures Song"))
                {
                  _client.UseItem("Loures Song");
                  Thread.Sleep(1000);
                }
                else if (_client.HasItem("Abel Song"))
                {
                  _client.UseItem("Abel Song");
                  Thread.Sleep(1000);
                }
                if (_client.HasItem("beothaich deum") || _client.HasItem("Red Potion"))
                {
                  _client.Tab.autowalker_locales.SelectedItem = (object)"Mileth";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Enchanted Garden";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                }
                else
                  _client.SendMessage("You need a beothaich for the next step.");
              }
            }
            if (_client.MapInfo.Number == 622 && _client.darkmazequest == 2)
            {
              _client.WalkToExact(81, 8);
              if (_client.Currentnpctext.StartsWith("You notice what appears to"))
              {
                _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)106, (byte)0, (byte)4, (byte)1, (byte)1);
                Thread.Sleep(1000);
              }
              if (_client.Currentnpctext.StartsWith("You wait but nothing appears"))
              {
                _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)106, (byte)0, (byte)40, (byte)1, (byte)2);
                Thread.Sleep(1000);
              }
              if (_client.Currentnpctext.StartsWith("*peeks* Are all the mundanes away?"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)106, (byte)0, (byte)35, (byte)1, (byte)2);
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)106, (byte)0, (byte)91, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)106, (byte)0, (byte)101);
                _client.darkmazequest = 3;
                Thread.Sleep(1000);
                _client.Tab.autowalker_locales.SelectedItem = (object)"Loures";
                _client.Tab.walklocaleslist.SelectedItem = (object)"Dungeon (aite)";
                _client.Tab.autowalker_button.Text = "Stop";
                _client.autowalkon = true;
              }
            }
          }
          if (_client.theletter && !_client.autowalkon && _client.GroupMembers.Count<string>() > 0 && _client.GroupIsInRange(7))
          {
            if (_client.MapInfo.Number == 129)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Oona");
              if (npcByName != null)
              {
                if (_client.Gender == (byte)0 && _client.letterquest == 1)
                {
                  foreach (Client client in Server.Alts.Values.ToArray<Client>())
                  {
                    if (client.Name.ToLower() == _client.GroupMembers[0].ToLower())
                    {
                      client.Tab.autowalker_button.Text = "Start";
                      client.autowalkon = false;
                    }
                  }
                  _client.DialogueRespond(npcByName.ID, (byte)4, (byte)78);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)78, (byte)0, (byte)19);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)78, (byte)0, (byte)20, (byte)1, (byte)2);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)78, (byte)0, (byte)28, (byte)2, _client.GroupMembers[0]);
                  _client.letterquest = 2;
                }
                if (_client.Gender == (byte)1 && _client.letterquest == 1)
                {
                  if (_client.Currentnpctext.Contains("If you aided him,"))
                  {
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)78, (byte)0, (byte)59);
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)78, (byte)0, (byte)60, (byte)1, (byte)2);
                  }
                  else if (_client.Currentnpctext.Contains("Frida of Abel"))
                  {
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)78, (byte)0, (byte)157);
                    Thread.Sleep(1000);
                    _client.letterquest = 2;
                    foreach (Client client in Server.Alts.Values.ToArray<Client>())
                    {
                      if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() || client.Name.ToLower() == _client.Name.ToLower())
                      {
                        client.Tab.mediumwalk.Checked = true;
                        client.Tab.autowalker_locales.SelectedItem = (object)"Abel";
                        client.Tab.walklocaleslist.SelectedItem = (object)"Tavern";
                        client.Tab.autowalker_button.Text = "Stop";
                        client.autowalkon = true;
                        client.letterquest = 2;
                        if (client.HasItem("Abel Song"))
                          client.UseItem("Abel Song");
                        else if (client.HasItem("Loures Song"))
                          client.UseItem("Loures Song");
                      }
                    }
                  }
                  else if (_client.Currentnpctext.Contains("Aoife dwell."))
                  {
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)78, (byte)0, (byte)144);
                    Thread.Sleep(1000);
                    _client.letterquest = 2;
                    foreach (Client client in Server.Alts.Values.ToArray<Client>())
                    {
                      if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() || client.Name.ToLower() == _client.Name.ToLower())
                      {
                        client.Tab.mediumwalk.Checked = true;
                        client.Tab.autowalker_locales.SelectedItem = (object)"Mileth";
                        client.Tab.walklocaleslist.SelectedItem = (object)"Temple of Choosing";
                        client.Tab.autowalker_button.Text = "Stop";
                        client.autowalkon = true;
                        client.letterquest = 2;
                      }
                    }
                  }
                  else if (_client.Currentnpctext.Contains("Riona scarcely"))
                  {
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)78, (byte)0, (byte)133);
                    Thread.Sleep(1000);
                    _client.letterquest = 2;
                    foreach (Client client in Server.Alts.Values.ToArray<Client>())
                    {
                      if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() || client.Name.ToLower() == _client.Name.ToLower())
                      {
                        client.Tab.mediumwalk.Checked = true;
                        client.Tab.autowalker_locales.SelectedItem = (object)"Mileth";
                        client.Tab.walklocaleslist.SelectedItem = (object)"Inn";
                        client.Tab.autowalker_button.Text = "Stop";
                        client.autowalkon = true;
                        client.letterquest = 2;
                      }
                    }
                  }
                  else if (_client.Currentnpctext.Contains("Duana"))
                  {
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)78, (byte)0, (byte)168);
                    Thread.Sleep(1000);
                    _client.letterquest = 2;
                    foreach (Client client in Server.Alts.Values.ToArray<Client>())
                    {
                      if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() || client.Name.ToLower() == _client.Name.ToLower())
                      {
                        client.Tab.mediumwalk.Checked = true;
                        client.Tab.autowalker_locales.SelectedItem = (object)"Mileth";
                        client.Tab.walklocaleslist.SelectedItem = (object)"Tavern";
                        client.Tab.autowalker_button.Text = "Stop";
                        client.autowalkon = true;
                        client.letterquest = 2;
                      }
                    }
                  }
                }
              }
            }
            if (_client.MapInfo.Number == 168)
            {
              if (_client.letterquest == 1 && _client.GroupMembers.Count<string>() == 1)
              {
                foreach (Client client in Server.Alts.Values.ToArray<Client>())
                {
                  if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() || client.Name.ToLower() == _client.Name.ToLower())
                  {
                    client.Tab.autowalker_button.Text = "Start";
                    client.autowalkon = false;
                  }
                }
              }
              Npc npcByName = _client.FindNpcByName<Npc>("Frida");
              if (npcByName != null)
              {
                if (_client.Gender == (byte)1 && _client.letterquest == 6)
                {
                  foreach (Client client in Server.Alts.Values.ToArray<Client>())
                  {
                    if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() && !client.autowalkon)
                    {
                      _client.Speak("the letter");
                      Thread.Sleep(500);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)84, (byte)0, (byte)186);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)84, (byte)0, (byte)187);
                      _client.theletter = false;
                      _client.SaveTimedStuff(33);
                      break;
                    }
                  }
                }
                if (_client.Gender == (byte)1 && _client.letterquest < 3)
                {
                  foreach (Client client in Server.Alts.Values.ToArray<Client>())
                  {
                    if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() && !client.autowalkon)
                    {
                      _client.Speak("the letter");
                      Thread.Sleep(500);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)84, (byte)0, (byte)63);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)84, (byte)0, (byte)64);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)84, (byte)0, (byte)66);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)84, (byte)0, (byte)67);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)84, (byte)0, (byte)70, (byte)1, (byte)6);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)84, (byte)0, (byte)106);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)84, (byte)0, (byte)108);
                      _client.letterquest = 3;
                      break;
                    }
                  }
                }
                if (_client.Gender == (byte)0 && _client.letterquest < 3 && _client.Currentnpctext != "")
                {
                  int num = 0;
                  if (_client.Currentnpctext.Contains("Courtney"))
                  {
                    _client.lettercourtney = true;
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)84, (byte)0, (byte)176);
                    num = 1;
                  }
                  else if (_client.Currentnpctext.Contains("Lowell"))
                  {
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)84, (byte)0, (byte)147);
                    num = 2;
                  }
                  else if (_client.Currentnpctext.Contains("Marlon"))
                  {
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)84, (byte)0, (byte)132);
                    num = 3;
                  }
                  else if (_client.Currentnpctext.Contains("Thibault"))
                  {
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)84, (byte)0, (byte)162);
                    num = 4;
                  }
                  Thread.Sleep(1000);
                  _client.letterquest = 3;
                  foreach (Client client in Server.Alts.Values.ToArray<Client>())
                  {
                    if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() || client.Name.ToLower() == _client.Name.ToLower())
                    {
                      client.Tab.mediumwalk.Checked = true;
                      client.letterquest = 3;
                      client.Tab.autowalker_locales.SelectedItem = (object)"Loures";
                      switch (num)
                      {
                        case 1:
                          client.Tab.walklocaleslist.SelectedItem = (object)"Throne Room";
                          break;
                        case 2:
                          client.Tab.walklocaleslist.SelectedItem = (object)"2nd Floor Weapon";
                          break;
                        case 3:
                          client.Tab.walklocaleslist.SelectedItem = (object)"1st Floor Weapon";
                          break;
                        case 4:
                          client.Tab.walklocaleslist.SelectedItem = (object)"Throne Room";
                          break;
                      }
                      client.Tab.autowalker_button.Text = "Stop";
                      client.autowalkon = true;
                      client.letterquest = 3;
                      if (client.HasItem("Loures Song"))
                        client.UseItem("Loures Song");
                      else if (client.HasItem("Abel Song"))
                        client.UseItem("Abel Song");
                    }
                  }
                }
              }
            }
            if (_client.MapInfo.Number == 393)
            {
              if (_client.letterquest == 1 && _client.GroupMembers.Count<string>() == 1)
              {
                foreach (Client client in Server.Alts.Values.ToArray<Client>())
                {
                  if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() || client.Name.ToLower() == _client.Name.ToLower())
                  {
                    client.Tab.autowalker_button.Text = "Start";
                    client.autowalkon = false;
                  }
                }
              }
              Npc npcByName = _client.FindNpcByName<Npc>("Aoife");
              if (npcByName != null)
              {
                if (_client.Gender == (byte)1 && _client.letterquest == 6)
                {
                  foreach (Client client in Server.Alts.Values.ToArray<Client>())
                  {
                    if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() && !client.autowalkon)
                    {
                      _client.Speak("the letter");
                      Thread.Sleep(500);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)83, (byte)0, (byte)186);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)83, (byte)0, (byte)187);
                      _client.theletter = false;
                      _client.SaveTimedStuff(33);
                      break;
                    }
                  }
                }
                if (_client.Gender == (byte)1 && _client.letterquest < 3)
                {
                  foreach (Client client in Server.Alts.Values.ToArray<Client>())
                  {
                    if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() && !client.autowalkon)
                    {
                      _client.Speak("the letter");
                      Thread.Sleep(500);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)83, (byte)0, (byte)63);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)83, (byte)0, (byte)64);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)83, (byte)0, (byte)66);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)83, (byte)0, (byte)67);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)83, (byte)0, (byte)70, (byte)1, (byte)6);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)83, (byte)0, (byte)106);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)83, (byte)0, (byte)108);
                      _client.letterquest = 3;
                      break;
                    }
                  }
                }
                if (_client.Gender == (byte)0 && _client.letterquest < 3 && _client.Currentnpctext != "")
                {
                  int num = 0;
                  if (_client.Currentnpctext.Contains("Courtney"))
                  {
                    _client.lettercourtney = true;
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)83, (byte)0, (byte)176);
                    num = 1;
                  }
                  else if (_client.Currentnpctext.Contains("Lowell"))
                  {
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)83, (byte)0, (byte)147);
                    num = 2;
                  }
                  else if (_client.Currentnpctext.Contains("Marlon"))
                  {
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)83, (byte)0, (byte)132);
                    num = 3;
                  }
                  else if (_client.Currentnpctext.Contains("Thibault"))
                  {
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)83, (byte)0, (byte)162);
                    num = 4;
                  }
                  Thread.Sleep(1000);
                  _client.letterquest = 3;
                  foreach (Client client in Server.Alts.Values.ToArray<Client>())
                  {
                    if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() || client.Name.ToLower() == _client.Name.ToLower())
                    {
                      client.Tab.mediumwalk.Checked = true;
                      client.letterquest = 3;
                      client.Tab.autowalker_locales.SelectedItem = (object)"Loures";
                      switch (num)
                      {
                        case 1:
                          client.Tab.walklocaleslist.SelectedItem = (object)"Throne Room";
                          break;
                        case 2:
                          client.Tab.walklocaleslist.SelectedItem = (object)"2nd Floor Weapon";
                          break;
                        case 3:
                          client.Tab.walklocaleslist.SelectedItem = (object)"1st Floor Weapon";
                          break;
                        case 4:
                          client.Tab.walklocaleslist.SelectedItem = (object)"Throne Room";
                          break;
                      }
                      client.Tab.autowalker_button.Text = "Stop";
                      client.autowalkon = true;
                      client.letterquest = 3;
                      if (client.HasItem("Loures Song"))
                        client.UseItem("Loures Song");
                      else if (client.HasItem("Abel Song"))
                        client.UseItem("Abel Song");
                    }
                  }
                }
              }
            }
            if (_client.MapInfo.Number == 134)
            {
              if (_client.letterquest == 1 && _client.GroupMembers.Count<string>() == 1)
              {
                foreach (Client client in Server.Alts.Values.ToArray<Client>())
                {
                  if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() || client.Name.ToLower() == _client.Name.ToLower())
                  {
                    client.Tab.autowalker_button.Text = "Start";
                    client.autowalkon = false;
                  }
                }
              }
              Npc npcByName = _client.FindNpcByName<Npc>("Duana");
              if (npcByName != null)
              {
                if (_client.Gender == (byte)1 && _client.letterquest == 6)
                {
                  foreach (Client client in Server.Alts.Values.ToArray<Client>())
                  {
                    if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() && !client.autowalkon)
                    {
                      _client.Speak("the letter");
                      Thread.Sleep(500);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)85, (byte)0, (byte)186);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)85, (byte)0, (byte)187);
                      _client.theletter = false;
                      _client.SaveTimedStuff(33);
                      break;
                    }
                  }
                }
                if (_client.Gender == (byte)1 && _client.letterquest < 3)
                {
                  foreach (Client client in Server.Alts.Values.ToArray<Client>())
                  {
                    if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() && !client.autowalkon)
                    {
                      _client.Speak("the letter");
                      Thread.Sleep(500);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)85, (byte)0, (byte)63);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)85, (byte)0, (byte)64);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)85, (byte)0, (byte)66);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)85, (byte)0, (byte)67);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)85, (byte)0, (byte)70, (byte)1, (byte)6);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)85, (byte)0, (byte)106);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)85, (byte)0, (byte)108);
                      _client.letterquest = 3;
                      break;
                    }
                  }
                }
                if (_client.Gender == (byte)0 && _client.letterquest < 3 && _client.Currentnpctext != "")
                {
                  int num = 0;
                  if (_client.Currentnpctext.Contains("Courtney"))
                  {
                    _client.lettercourtney = true;
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)85, (byte)0, (byte)176);
                    num = 1;
                  }
                  else if (_client.Currentnpctext.Contains("Lowell"))
                  {
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)85, (byte)0, (byte)147);
                    num = 2;
                  }
                  else if (_client.Currentnpctext.Contains("Marlon"))
                  {
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)85, (byte)0, (byte)132);
                    num = 3;
                  }
                  else if (_client.Currentnpctext.Contains("Thibault"))
                  {
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)85, (byte)0, (byte)162);
                    num = 4;
                  }
                  Thread.Sleep(1000);
                  _client.letterquest = 3;
                  foreach (Client client in Server.Alts.Values.ToArray<Client>())
                  {
                    if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() || client.Name.ToLower() == _client.Name.ToLower())
                    {
                      client.Tab.mediumwalk.Checked = true;
                      client.letterquest = 3;
                      client.Tab.autowalker_locales.SelectedItem = (object)"Loures";
                      switch (num)
                      {
                        case 1:
                          client.Tab.walklocaleslist.SelectedItem = (object)"Throne Room";
                          break;
                        case 2:
                          client.Tab.walklocaleslist.SelectedItem = (object)"2nd Floor Weapon";
                          break;
                        case 3:
                          client.Tab.walklocaleslist.SelectedItem = (object)"1st Floor Weapon";
                          break;
                        case 4:
                          client.Tab.walklocaleslist.SelectedItem = (object)"Throne Room";
                          break;
                      }
                      client.Tab.autowalker_button.Text = "Stop";
                      client.autowalkon = true;
                      client.letterquest = 3;
                      if (client.HasItem("Loures Song"))
                        client.UseItem("Loures Song");
                      else if (client.HasItem("Abel Song"))
                        client.UseItem("Abel Song");
                    }
                  }
                }
              }
            }
            if (_client.MapInfo.Number == 136)
            {
              if (_client.letterquest == 1 && _client.GroupMembers.Count<string>() == 1)
              {
                foreach (Client client in Server.Alts.Values.ToArray<Client>())
                {
                  if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() || client.Name.ToLower() == _client.Name.ToLower())
                  {
                    client.Tab.autowalker_button.Text = "Start";
                    client.autowalkon = false;
                  }
                }
              }
              Npc npcByName = _client.FindNpcByName<Npc>("Riona");
              if (npcByName != null)
              {
                if (_client.Gender == (byte)1 && _client.letterquest == 6)
                {
                  foreach (Client client in Server.Alts.Values.ToArray<Client>())
                  {
                    if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() && !client.autowalkon)
                    {
                      _client.Speak("the letter");
                      Thread.Sleep(500);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)79, (byte)0, (byte)186);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)79, (byte)0, (byte)187);
                      _client.theletter = false;
                      _client.SaveTimedStuff(33);
                      break;
                    }
                  }
                }
                if (_client.Gender == (byte)1 && _client.letterquest < 3)
                {
                  foreach (Client client in Server.Alts.Values.ToArray<Client>())
                  {
                    if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() && !client.autowalkon)
                    {
                      _client.Speak("the letter");
                      Thread.Sleep(500);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)79, (byte)0, (byte)63);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)79, (byte)0, (byte)64);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)79, (byte)0, (byte)66);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)79, (byte)0, (byte)67);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)79, (byte)0, (byte)70, (byte)1, (byte)6);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)79, (byte)0, (byte)106);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)79, (byte)0, (byte)108);
                      _client.letterquest = 3;
                      break;
                    }
                  }
                }
                if (_client.Gender == (byte)0 && _client.letterquest < 3 && _client.Currentnpctext != "")
                {
                  int num = 0;
                  if (_client.Currentnpctext.Contains("Courtney"))
                  {
                    _client.lettercourtney = true;
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)79, (byte)0, (byte)176);
                    num = 1;
                  }
                  else if (_client.Currentnpctext.Contains("Lowell"))
                  {
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)79, (byte)0, (byte)147);
                    num = 2;
                  }
                  else if (_client.Currentnpctext.Contains("Marlon"))
                  {
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)79, (byte)0, (byte)132);
                    num = 3;
                  }
                  else if (_client.Currentnpctext.Contains("Thibault"))
                  {
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)79, (byte)0, (byte)162);
                    num = 4;
                  }
                  Thread.Sleep(1000);
                  _client.letterquest = 3;
                  foreach (Client client in Server.Alts.Values.ToArray<Client>())
                  {
                    if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() || client.Name.ToLower() == _client.Name.ToLower())
                    {
                      client.Tab.mediumwalk.Checked = true;
                      client.letterquest = 3;
                      client.Tab.autowalker_locales.SelectedItem = (object)"Loures";
                      switch (num)
                      {
                        case 1:
                          client.Tab.walklocaleslist.SelectedItem = (object)"Throne Room";
                          break;
                        case 2:
                          client.Tab.walklocaleslist.SelectedItem = (object)"2nd Floor Weapon";
                          break;
                        case 3:
                          client.Tab.walklocaleslist.SelectedItem = (object)"1st Floor Weapon";
                          break;
                        case 4:
                          client.Tab.walklocaleslist.SelectedItem = (object)"Throne Room";
                          break;
                      }
                      client.Tab.autowalker_button.Text = "Stop";
                      client.autowalkon = true;
                      client.letterquest = 3;
                      if (client.HasItem("Loures Song"))
                        client.UseItem("Loures Song");
                      else if (client.HasItem("Abel Song"))
                        client.UseItem("Abel Song");
                    }
                  }
                }
              }
            }
            if (_client.MapInfo.Number == 115)
            {
              if (_client.lettercourtney)
              {
                if (_client.letterquest == 1 && _client.GroupMembers.Count<string>() == 1)
                {
                  foreach (Client client in Server.Alts.Values.ToArray<Client>())
                  {
                    if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() || client.Name.ToLower() == _client.Name.ToLower())
                    {
                      client.Tab.autowalker_button.Text = "Start";
                      client.autowalkon = false;
                    }
                  }
                }
                Npc npcByName = _client.FindNpcByName<Npc>("Courtney");
                if (npcByName != null && npcByName.Name == "Courtney" && _client.Gender == (byte)0 && _client.letterquest < 4)
                {
                  foreach (Client client in Server.Alts.Values.ToArray<Client>())
                  {
                    if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() && !client.autowalkon)
                    {
                      _client.Speak("the love letter");
                      Thread.Sleep(500);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)88, (byte)0, (byte)26, (byte)1, (byte)3);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)88, (byte)0, (byte)43);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)88, (byte)0, (byte)46);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)88, (byte)0, (byte)65);
                      Thread.Sleep(1000);
                      _client.letterquest = 4;
                      break;
                    }
                  }
                }
              }
              else
              {
                if (_client.letterquest == 1 && _client.GroupMembers.Count<string>() == 1)
                {
                  foreach (Client client in Server.Alts.Values.ToArray<Client>())
                  {
                    if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() || client.Name.ToLower() == _client.Name.ToLower())
                    {
                      client.Tab.autowalker_button.Text = "Start";
                      client.autowalkon = false;
                    }
                  }
                }
                Npc npcByName = _client.FindNpcByName<Npc>("Thibault");
                if (npcByName != null && npcByName.Name == "Thibault" && _client.Gender == (byte)0 && _client.letterquest < 4)
                {
                  foreach (Client client in Server.Alts.Values.ToArray<Client>())
                  {
                    if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() && !client.autowalkon)
                    {
                      _client.Speak("the letter");
                      Thread.Sleep(500);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)87, (byte)0, (byte)26, (byte)1, (byte)3);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)87, (byte)0, (byte)43);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)87, (byte)0, (byte)46);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)87, (byte)0, (byte)65);
                      Thread.Sleep(1000);
                      _client.letterquest = 4;
                      break;
                    }
                  }
                }
              }
            }
            if (_client.MapInfo.Number == 118)
            {
              if (_client.letterquest == 1 && _client.GroupMembers.Count<string>() == 1)
              {
                foreach (Client client in Server.Alts.Values.ToArray<Client>())
                {
                  if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() || client.Name.ToLower() == _client.Name.ToLower())
                  {
                    client.Tab.autowalker_button.Text = "Start";
                    client.autowalkon = false;
                  }
                }
              }
              Npc npcByName = _client.FindNpcByName<Npc>("Lowell");
              if (npcByName != null && _client.Gender == (byte)0 && _client.letterquest < 4)
              {
                foreach (Client client in Server.Alts.Values.ToArray<Client>())
                {
                  if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() && !client.autowalkon)
                  {
                    _client.Speak("the letter");
                    Thread.Sleep(500);
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)86, (byte)0, (byte)26, (byte)1, (byte)3);
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)86, (byte)0, (byte)43);
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)86, (byte)0, (byte)46);
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)86, (byte)0, (byte)65);
                    Thread.Sleep(1000);
                    _client.letterquest = 4;
                    break;
                  }
                }
              }
            }
            if (_client.MapInfo.Number == 122)
            {
              if (_client.letterquest == 1 && _client.GroupMembers.Count<string>() == 1)
              {
                foreach (Client client in Server.Alts.Values.ToArray<Client>())
                {
                  if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() || client.Name.ToLower() == _client.Name.ToLower())
                  {
                    client.Tab.autowalker_button.Text = "Start";
                    client.autowalkon = false;
                  }
                }
              }
              Npc npcByName = _client.FindNpcByName<Npc>("Marlon");
              if (npcByName != null && _client.Gender == (byte)0 && _client.letterquest < 4)
              {
                foreach (Client client in Server.Alts.Values.ToArray<Client>())
                {
                  if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() && !client.autowalkon)
                  {
                    _client.Speak("the letter");
                    Thread.Sleep(500);
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)80, (byte)0, (byte)26, (byte)1, (byte)3);
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)80, (byte)0, (byte)43);
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)80, (byte)0, (byte)46);
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)80, (byte)0, (byte)65);
                    Thread.Sleep(1000);
                    _client.letterquest = 4;
                    break;
                  }
                }
              }
            }
            if (_client.MapInfo.Number == 303)
            {
              if (_client.letterquest == 1 && _client.GroupMembers.Count<string>() == 1)
              {
                foreach (Client client in Server.Alts.Values.ToArray<Client>())
                {
                  if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() || client.Name.ToLower() == _client.Name.ToLower())
                  {
                    client.Tab.autowalker_button.Text = "Start";
                    client.autowalkon = false;
                  }
                }
              }
              Npc npcByName = _client.FindNpcByName<Npc>("Baltasar");
              if (npcByName != null)
              {
                if (_client.Gender == (byte)1 && _client.letterquest < 5 && _client.Currentnpctext != "")
                {
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)81, (byte)0, (byte)35, (byte)1, (byte)2);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)81, (byte)0, (byte)43);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)81, (byte)0, (byte)44);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)81, (byte)0, (byte)45);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)81, (byte)0, (byte)47, (byte)1, (byte)2);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)81, (byte)0, (byte)64);
                  Thread.Sleep(1000);
                  _client.letterquest = 5;
                }
                if (_client.Gender == (byte)0 && _client.letterquest < 5)
                {
                  foreach (Client client in Server.Alts.Values.ToArray<Client>())
                  {
                    if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() && !client.autowalkon)
                    {
                      _client.Speak("the letter");
                      Thread.Sleep(500);
                      _client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)81, (byte)0, (byte)20, (byte)1, (byte)1);
                      _client.letterquest = 5;
                      break;
                    }
                  }
                }
              }
            }
            if (_client.MapInfo.Number == 3041)
            {
              if (_client.letterquest == 1 && _client.GroupMembers.Count<string>() == 1)
              {
                foreach (Client client in Server.Alts.Values.ToArray<Client>())
                {
                  if (client.Name.ToLower() == _client.GroupMembers[0].ToLower() || client.Name.ToLower() == _client.Name.ToLower())
                  {
                    client.Tab.autowalker_button.Text = "Start";
                    client.autowalkon = false;
                  }
                }
              }
              if (_client.Gender == (byte)1 && _client.letterquest < 6 && (_client.ServerLocation.X != 1 || _client.ServerLocation.Y != 14))
                _client.WalkToExact(1, 14);
              if (_client.Gender == (byte)1 && _client.letterquest < 6 && _client.Currentnpctext.Contains("You attempt to pull out the compartment"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)82, (byte)0, (byte)15, (byte)1, (byte)2);
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)82, (byte)0, (byte)30, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)82, (byte)0, (byte)37);
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)82, (byte)0, (byte)30, (byte)1, (byte)2);
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)82, (byte)0, (byte)42);
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)82, (byte)0, (byte)30, (byte)1, (byte)3);
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)82, (byte)0, (byte)47);
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)82, (byte)0, (byte)30, (byte)1, (byte)4);
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)82, (byte)0, (byte)78, (byte)1, (byte)2);
                _client.letterquest = 6;
                Thread.Sleep(1000);
              }
            }
          }
          if (_client.molo && !_client.autowalkon)
          {
            Npc npc = (Npc)null;
            if (_client.MapInfo.Number == 181)
              npc = _client.FindNpcByName<Npc>("Aud");
            if (_client.MapInfo.Number == 129)
              npc = _client.FindNpcByName<Npc>("Oona");
            if (_client.MapInfo.Number == 162)
              npc = _client.FindNpcByName<Npc>("Alleen");
            if (npc != null)
            {
              _client.DialogueRespond(npc.ID, (byte)4, (byte)70);
              while (!_client.Currentnpctext.Contains("I think I have something"))
              {
                if (_client.Currentnpctext.Contains("has horrible nightmares"))
                {
                  _client.PopupRespond(new uint?(npc.ID), (byte)0, (byte)70, (byte)0, (byte)16);
                  break;
                }
                Thread.Sleep(200);
              }
              while (!_client.Currentnpctext.Contains("I think I have something"))
                Thread.Sleep(200);
              if (_client.Currentnpctext.Contains("terrible viper"))
              {
                if (!_client.HasPersonaca())
                {
                  _client.SendMessage("Get a personaca deum");
                  _client.PopupRespond(new uint?(npc.ID), (byte)0, (byte)70, (byte)0, (byte)52);
                }
                else
                {
                  _client.PopupRespond(new uint?(npc.ID), (byte)0, (byte)70, (byte)0, (byte)53, (byte)1, (byte)2);
                  _client.PopupRespond(new uint?(npc.ID), (byte)0, (byte)70, (byte)0, (byte)71);
                  _client.PopupRespond(new uint?(npc.ID), (byte)0, (byte)70, (byte)0, (byte)72);
                  _client.PopupRespond(new uint?(npc.ID), (byte)0, (byte)70, (byte)0, (byte)73);
                  _client.SaveTimedStuff(31);
                }
              }
              else if (_client.Currentnpctext.Contains("nightmares grow"))
              {
                if (!_client.HasBetony())
                {
                  _client.SendMessage("Get a betony deum");
                  _client.PopupRespond(new uint?(npc.ID), (byte)0, (byte)70, (byte)0, (byte)34);
                }
                else
                {
                  _client.PopupRespond(new uint?(npc.ID), (byte)0, (byte)70, (byte)0, (byte)35, (byte)1, (byte)2);
                  _client.PopupRespond(new uint?(npc.ID), (byte)0, (byte)70, (byte)0, (byte)71);
                  _client.PopupRespond(new uint?(npc.ID), (byte)0, (byte)70, (byte)0, (byte)72);
                  _client.PopupRespond(new uint?(npc.ID), (byte)0, (byte)70, (byte)0, (byte)73);
                  _client.SaveTimedStuff(31);
                }
              }
              else if (_client.Currentnpctext.Contains("restore her strength"))
              {
                if (!_client.HasHydele())
                {
                  _client.SendMessage("Get a hydele deum");
                  _client.PopupRespond(new uint?(npc.ID), (byte)0, (byte)70, (byte)0, (byte)52);
                }
                else
                {
                  _client.PopupRespond(new uint?(npc.ID), (byte)0, (byte)70, (byte)0, (byte)44, (byte)1, (byte)2);
                  _client.PopupRespond(new uint?(npc.ID), (byte)0, (byte)70, (byte)0, (byte)71);
                  _client.PopupRespond(new uint?(npc.ID), (byte)0, (byte)70, (byte)0, (byte)72);
                  _client.PopupRespond(new uint?(npc.ID), (byte)0, (byte)70, (byte)0, (byte)73);
                  _client.SaveTimedStuff(31);
                }
              }
              else if (_client.Currentnpctext.Contains("Thank you. My da"))
                _client.PopupRespond(new uint?(npc.ID), (byte)0, (byte)70, (byte)0, (byte)10);
              _client.molo = false;
            }
          }
          if (!_client.pause && !_client.autowalkon)
          {
            if (_client.MapInfo.Number == 1006 && _client.HasItem("Papaya") && _client.HasItem("Rambutan") && _client.HasItem("Green Grapes") && _client.HasItem("Grapes") && _client.HasItem("Cherry") && !_client.InventoryIsFull())
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Qaeli");
              if (npcByName != null)
              {
                _client.SkillSpellCaption("giant ant");
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)134, (byte)0, (byte)5, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)134, (byte)0, (byte)9, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)134, (byte)0, (byte)13);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)134, (byte)0, (byte)14, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)134, (byte)0, (byte)18);
                Thread.Sleep(1000);
                _client.SkillSpellCaption("i have the fruits");
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)135, (byte)0, (byte)38);
                Thread.Sleep(1000);
              }
            }
            if (_client.MapInfo.Number == 1006 && _client.ItemCount("Mantis's Eye") >= 3U && _client.ItemCount("Tangerines") >= 2U && _client.HasItem("Passion Flower"))
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Qaeli");
              if (npcByName != null)
              {
                _client.SkillSpellCaption("red mantis");
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)136, (byte)0, (byte)5, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)136, (byte)0, (byte)9);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)136, (byte)0, (byte)10);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)136, (byte)0, (byte)11);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)136, (byte)0, (byte)12);
                Thread.Sleep(1000);
                _client.SkillSpellCaption("i have the mantis stuff");
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)137, (byte)0, (byte)14);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)137, (byte)0, (byte)25);
                Thread.Sleep(1000);
              }
            }
            if (_client.MapInfo.Number == 1007 && _client.HasItem("Red Mantis Claw") && _client.HasItem("Ant Head"))
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Codus");
              if (npcByName != null)
              {
                _client.SkillSpellCaption("creant");
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)143, (byte)0, (byte)5);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)143, (byte)0, (byte)6, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)143, (byte)0, (byte)15);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)143, (byte)0, (byte)19);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)143, (byte)0, (byte)20);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)143, (byte)0, (byte)21);
                Thread.Sleep(1000);
                _client.SkillSpellCaption("i have the head");
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)144, (byte)0, (byte)13);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)144, (byte)0, (byte)14);
                Thread.Sleep(1000);
                _client.SkillSpellCaption("i have the claw");
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)145, (byte)0, (byte)12);
                Thread.Sleep(1000);
              }
            }
            if (_client.MapInfo.Number == 134 && _client.HasItem("Tentacle"))
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Theradus");
              if (npcByName != null && (int)_client.LastnpcpopupID != (int)npcByName.ID)
              {
                _client.ClickNpc(npcByName.ID);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)131, (byte)0, (byte)5);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)131, (byte)0, (byte)6);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)131, (byte)0, (byte)7, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)131, (byte)0, (byte)17);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)131, (byte)0, (byte)18);
                Thread.Sleep(1000);
                _client.Tab.autowalker_locales.SelectedItem = (object)"Shinewood";
                _client.Tab.walklocaleslist.SelectedItem = (object)"SW 8 (DSS)";
                _client.Tab.autowalker_button.Text = "Stop";
                _client.autowalkon = true;
                //if (!_client.BotThread.IsAlive)
                //  _client.BotThread.Start();
                _client.pause = false;
                _client.Tab.btnPlay.Enabled = false;
                _client.Tab.btnStop.Enabled = true;
              }
            }
          }
          if (!_client.pause && _client.buy2ndtalisman)
          {
            if (_client.MapInfo.Number == 6718 && !_client.InventoryIsFull() && _client.Statistics.Gold > 500000U)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Lalerid");
              if (npcByName != null && npcByName.IsOnScreen)
              {
                _client.DialogueRespond(npcByName.ID, (byte)8, (byte)109);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)109, (byte)0, (byte)8, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)109, (byte)0, (byte)12, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)109, (byte)0, (byte)16);
                Thread.Sleep(1000);
                _client.DialogueRespond(npcByName.ID, (byte)8, (byte)109);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)109, (byte)0, (byte)24, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)109, (byte)0, (byte)36, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)109, (byte)0, (byte)39);
                Thread.Sleep(1000);
              }
            }
            if (_client.MapInfo.Number == 6716 && _client.Statistics.Gold > 100000U && _client.HasItem("Half Talisman") && _client.HasItem("2nd Half Talisman"))
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Uliam");
              if (npcByName != null && npcByName.IsOnScreen)
              {
                _client.DialogueRespond(npcByName.ID, (byte)8, (byte)110);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)110, (byte)0, (byte)1, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)110, (byte)0, (byte)16);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)110, (byte)0, (byte)17, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)110, (byte)0, (byte)33);
                Thread.Sleep(1000);
              }
            }
            if (_client.MapInfo.Number == 6716 && _client.Statistics.Gold > 500000U && _client.HasItem("Talisman") && _client.HasItem("Ruby Eye") && _client.HasItem("Sapphire Eye") && _client.HasItem("Emerald Eye") && _client.HasItem("Diamond Eye") && _client.HasItem("Iron Ore Eye") && _client.HasItem("Orange Beryl Eye"))
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Uliam");
              if (npcByName != null && npcByName.IsOnScreen)
              {
                _client.DialogueRespond(npcByName.ID, (byte)8, (byte)110);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)110, (byte)0, (byte)1, (byte)1, (byte)2);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)110, (byte)0, (byte)64, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)110, (byte)0, (byte)95);
                Thread.Sleep(1000);
              }
            }
            if (_client.MapInfo.Number == 6716 && _client.Statistics.Gold > 100000U && _client.HasItem("Giant Pearl") && _client.HasItem("Talisman with Gems"))
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Uliam");
              if (npcByName != null && npcByName.IsOnScreen)
              {
                _client.DialogueRespond(npcByName.ID, (byte)8, (byte)110);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)110, (byte)0, (byte)1, (byte)1, (byte)3);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)110, (byte)0, (byte)109);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)110, (byte)0, (byte)110, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)110, (byte)0, (byte)126);
                Thread.Sleep(1000);
              }
            }
          }
          if (!_client.pause && _client.giantpearl)
          {
            if (!_client.giantpearl2)
            {
              if (_client.Gender == (byte)0 && _client.HasItem("Bathing Trousers"))
              {
                _client.UseItem("Bathing Trousers");
                while (_client.Overcoat != "Bathing Trousers")
                  Thread.Sleep(200);
                _client.Refresh();
              }
              else if (_client.Gender == (byte)1 && _client.HasItem("Sarong"))
              {
                _client.UseItem("Sarong");
                while (_client.Overcoat != "Sarong")
                  Thread.Sleep(200);
                _client.Refresh();
              }
            }
            if (_client.giantpearl2 && (_client.Overcoat == "Bathing Trousers" || _client.Overcoat == "Sarong"))
              _client.UnequipSlot((byte)15);
            if (_client.giantpearl2)
            {
              if (_client.HasItem("Bathing Trousers"))
              {
                _client.giantpearl2 = false;
                _client.DropItems("Bathing Trousers");
                if (_client.MapInfo.Number == 6627)
                  _client.Drop(7, 6, _client.ItemSlot("Giant Pearl"), 1);
                else if (_client.MapInfo.Number == 6625)
                  _client.Drop(58, 26, _client.ItemSlot("Giant Pearl"), 1);
                _client.giantpearl = false;
                _client.Tab.autowalker_button.Text = "Start";
                _client.autowalkon = false;
              }
              if (_client.HasItem("Sarong"))
              {
                _client.giantpearl2 = false;
                _client.DropItems("Sarong");
                if (_client.MapInfo.Number == 6627)
                  _client.Drop(7, 6, _client.ItemSlot("Giant Pearl"), 1);
                else if (_client.MapInfo.Number == 6625)
                  _client.Drop(58, 26, _client.ItemSlot("Giant Pearl"), 1);
                _client.giantpearl = false;
                _client.Tab.autowalker_button.Text = "Start";
                _client.autowalkon = false;
              }
            }
          }
          if (_client.Currentnpctext != string.Empty)
          {
            if (_client.Currentnpctext.StartsWith("You see something shiny next to your feet"))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)3, _client.Currentnpcscript, (byte)0, (byte)15, (byte)4);
            if (_client.Currentnpctext.StartsWith("You find some bones"))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)4, _client.Currentnpcscript, (byte)0, (byte)13, (byte)4);
            if (_client.Currentnpctext.StartsWith("You feel the ground shift beneath your feet. It does not seem solid. You take"))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)4, _client.Currentnpcscript, (byte)0, (byte)11, (byte)4);
            else if (_client.Currentnpctext.StartsWith("You feel the ground shift beneath"))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)4, _client.Currentnpcscript, (byte)0, (byte)7, (byte)4);
          }
          if (_client.Tab.vwalklocaleslist == "Shamensyth ent" || _client.Tab.vwalklocaleslist == "Shamensyth" || _client.Tab.vwalklocaleslist == "Top Blazing Wand" || _client.Tab.vwalklocaleslist == "Bottom Blazing Wand")
          {
            if (!_client.pause && _client.autowalkon && _client.MapInfo.Number == 706)
              _client.Speak("enter sewer maze", 2);
            if (!_client.pause && _client.MapInfo.Number == 6716 && _client.Statistics.Gold > 300000U && _client.HasItem("Top Blazing Wand Piece") && _client.HasItem("Lower Blazing Wand Piece"))
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Uliam");
              if (npcByName != null && npcByName.IsOnScreen)
              {
                _client.DialogueRespond(npcByName.ID, (byte)8, (byte)110);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)110, (byte)0, (byte)1, (byte)1, (byte)4);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)110, (byte)0, (byte)138, (byte)1, (byte)1);
                Thread.Sleep(1000);
              }
            }
          }
          if (!_client.pause && _client.Tab.vwalklocaleslist == "tauren nose ring")
          {
            if (_client.HasItem("Goblin Hy-brasyl Gauntlet") && _client.nosering != 3)
              _client.nosering = 3;
            else if (_client.HasItem("Throne Key") && _client.nosering < 2)
              _client.nosering = 2;
            if (_client.MapInfo.Number == 2082 && _client.Currentnpctext != string.Empty && _client.Currentnpctext.StartsWith("*You hear a low"))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)4, (byte)175, (byte)0, (byte)4, (byte)1, (byte)1);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)4, (byte)175, (byte)0, (byte)12);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)4, (byte)175, (byte)0, (byte)14);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)4, (byte)175, (byte)0, (byte)15);
              Thread.Sleep(1000);
            }
            if (_client.MapInfo.Number == 2080 && _client.Currentnpctext != string.Empty && _client.Currentnpctext.StartsWith("You unlocked"))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)4, (byte)174, (byte)0, (byte)17);
            if (_client.MapInfo.Number == 2076 && _client.Currentnpctext != string.Empty && _client.Currentnpctext.StartsWith("Yes, I was told that"))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)4, (byte)173, (byte)0, (byte)5);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)4, (byte)173, (byte)0, (byte)6, (byte)1, (byte)1);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)4, (byte)173, (byte)0, (byte)10, (byte)1, (byte)1);
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)4, (byte)173, (byte)0, (byte)17);
              Thread.Sleep(1000);
            }
            if (_client.MapInfo.Number == 2078)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Hogor");
              if (npcByName != null && npcByName.IsOnScreen)
              {
                if (_client.nosering == 0)
                {
                  _client.ClickNpc(npcByName.ID);
                  while (_client.Currentnpctext == string.Empty)
                    Thread.Sleep(200);
                  if (_client.Currentnpctext.StartsWith("You want the Goblin Gauntlet?"))
                  {
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)172, (byte)0, (byte)9, (byte)1, (byte)1);
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)172, (byte)0, (byte)16);
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)172, (byte)0, (byte)17, (byte)1, (byte)1);
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)172, (byte)0, (byte)21);
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)172, (byte)0, (byte)22);
                    _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)172, (byte)0, (byte)23);
                    _client.nosering = 1;
                  }
                }
                else if (_client.nosering == 2)
                {
                  _client.ClickNpc(npcByName.ID);
                  _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)172, (byte)0, (byte)33);
                  _client.nosering = 3;
                }
              }
            }
            if (_client.MapInfo.Number == 2110)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Gridak");
              if (npcByName != null && npcByName.IsOnScreen)
              {
                _client.ClickNpc(npcByName.ID);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)171, (byte)0, (byte)9);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)171, (byte)0, (byte)10);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)171, (byte)0, (byte)11);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)171, (byte)0, (byte)12, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)171, (byte)0, (byte)16);
                _client.PopupRespond(new uint?(npcByName.ID), (byte)4, (byte)171, (byte)0, (byte)17, (byte)1, (byte)1);
                Thread.Sleep(1000);
              }
            }
          }
          if (!_client.pause && _client.Tab.vwalklocaleslist == "tauren horn")
          {
            if (_client.Currentnpctext != string.Empty && _client.Currentnpctext.StartsWith("Keep away! A mystical beast"))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)4, (byte)160, (byte)0, (byte)1);
            if (_client.Currentnpctext != string.Empty && _client.Currentnpctext.StartsWith("Even is where it lies."))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)4, (byte)161, (byte)0, (byte)1);
            if (_client.Currentnpctext != string.Empty && _client.Currentnpctext.StartsWith("Bring nothing with you."))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)4, (byte)163, (byte)0, (byte)1);
            if (_client.Currentnpctext != string.Empty && _client.Currentnpctext.StartsWith("Enter if you dare."))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)4, (byte)164, (byte)0, (byte)1);
            if (_client.Currentnpctext != string.Empty && _client.Currentnpctext.StartsWith("To summon the beast."))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)4, (byte)167, (byte)0, (byte)1);
            if (_client.Currentnpctext != string.Empty && _client.Currentnpctext.StartsWith("Howl its name,"))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)4, (byte)168, (byte)0, (byte)1);
            if (_client.Currentnpctext != string.Empty && _client.Currentnpctext.StartsWith("Rage will consume it"))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)4, (byte)165, (byte)0, (byte)1);
            if (_client.Currentnpctext != string.Empty && _client.Currentnpctext.StartsWith("Old is the beast"))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)4, (byte)166, (byte)0, (byte)1);
            if (_client.Currentnpctext != string.Empty && _client.Currentnpctext.StartsWith("Light is its life"))
              _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)4, (byte)162, (byte)0, (byte)1);
            foreach (Character character in _client.Characters.Values.ToArray<Character>())
            {
              if (character != null && character is Npc && character.Map == _client.MapInfo.Number && character.IsOnScreen)
              {
                if (character.Name == "IceStone1" && _client.taurenhorn == 0)
                {
                  _client.ClickNpc(character.ID);
                  _client.taurenhorn = 1;
                }
                if (character.Name == "IceStone2" && _client.taurenhorn == 1)
                {
                  _client.ClickNpc(character.ID);
                  _client.taurenhorn = 2;
                }
                if (character.Name == "IceStone4" && _client.taurenhorn == 2)
                {
                  _client.ClickNpc(character.ID);
                  _client.taurenhorn = 3;
                }
                if (character.Name == "IceStone5" && _client.taurenhorn == 3)
                {
                  _client.ClickNpc(character.ID);
                  _client.taurenhorn = 4;
                }
                if (character.Name == "IceStone8" && _client.taurenhorn == 4)
                {
                  _client.ClickNpc(character.ID);
                  _client.taurenhorn = 5;
                }
                if (character.Name == "IceStone9" && _client.taurenhorn == 5)
                {
                  _client.ClickNpc(character.ID);
                  _client.taurenhorn = 6;
                }
                if (character.Name == "IceStone6" && _client.taurenhorn == 6)
                {
                  _client.ClickNpc(character.ID);
                  _client.taurenhorn = 7;
                }
                if (character.Name == "IceStone7" && _client.taurenhorn == 7)
                {
                  _client.ClickNpc(character.ID);
                  _client.taurenhorn = 8;
                }
                if (character.Name == "IceStone3" && _client.taurenhorn == 8)
                {
                  _client.ClickNpc(character.ID);
                  _client.taurenhorn = 9;
                }
              }
            }
          }
          if (!_client.pause && _client.lawquest)
          {
            if (_client.MapInfo.Name.Contains("Dung Field") && _client.LastnpcpopupID > 0U)
              _client.LastnpcpopupID = 0U;
            if ((_client.MapInfo.Number == 10056 || _client.MapInfo.Number == 10004) && _client.Tab.autowalker_button.Text != "Stop")
            {
              _client.Tab.LoadTemplate("default");
              _client.Tab.fastwalk.Checked = true;
              _client.Tab.autowalker_locales.Text = "Lost Ruins";
              _client.Tab.walklocaleslist.SelectedItem = (object)"Nairn";
              _client.Tab.autowalker_button.Text = "Stop";
              _client.autowalkon = true;
              _client.Tab.btnPlay.Enabled = false;
              _client.Tab.btnStop.Enabled = true;
              _client.pause = false;
            }
            if (_client.MapInfo.Name.Contains("Lost Ruins 7") && _client.LastnpcpopupID > 0U)
              _client.LastnpcpopupID = 0U;
            else if (_client.MapInfo.Number == 8995)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Nairn");
              if (npcByName != null && npcByName.IsOnScreen && (int)_client.LastnpcpopupID != (int)npcByName.ID)
              {
                _client.ClickNpc(npcByName.ID);
                if (_client.Currentnpctext.StartsWith("Hello.  What can I do for you?"))
                  _client.PopupOption3();
                else if (_client.Currentnpctext.StartsWith("You just discovered the Lost Ruins."))
                {
                  _client.PopupClose(new uint?(_client.CurrentnpcpopupID));
                  _client.SendMessage("Not time yet! (1 hour wait)");
                  _client.LastnpcpopupID = npcByName.ID;
                }
                else if (_client.Currentnpctext.StartsWith("Have you found any sign of the ruins?"))
                {
                  _client.PopupClose(new uint?(_client.CurrentnpcpopupID));
                  _client.Tab.autowalker_locales.Text = "Lost Ruins";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"LR2 Rocks";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.LastnpcpopupID = npcByName.ID;
                }
                else if (_client.Currentnpctext.StartsWith("I found several rocks that could have been one of the five structures you were talking about.") || _client.Currentnpctext.StartsWith("Go on, find the second ruins site."))
                {
                  _client.PopupClose(new uint?(_client.CurrentnpcpopupID));
                  _client.Tab.autowalker_locales.Text = "Lost Ruins";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"LR3 Rocks";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.LastnpcpopupID = npcByName.ID;
                }
                else if (_client.Currentnpctext.StartsWith("Great news, I found another ruin.") || _client.Currentnpctext.StartsWith("Go on, find the wall scriptures."))
                {
                  _client.PopupClose(new uint?(_client.CurrentnpcpopupID));
                  _client.Tab.autowalker_locales.Text = "Lost Ruins";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Ass Dungeon";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.LastnpcpopupID = npcByName.ID;
                }
                else if (_client.Currentnpctext.StartsWith("You were right, the wall scriptures were hidden from plain sight.") || _client.Currentnpctext.StartsWith("Go on, find the third set of ruins!"))
                {
                  _client.PopupClose(new uint?(_client.CurrentnpcpopupID));
                  _client.Tab.autowalker_locales.Text = "Lost Ruins";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"LR4 Rocks";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.LastnpcpopupID = npcByName.ID;
                }
                else if (_client.Currentnpctext.StartsWith("Looks like you got some scrapes and bruises."))
                {
                  _client.PopupClose(new uint?(_client.CurrentnpcpopupID));
                  _client.LastnpcpopupID = 0U;
                }
                else if (_client.Currentnpctext.StartsWith("Go find the rest of the wall tablets."))
                {
                  _client.PopupClose(new uint?(_client.CurrentnpcpopupID));
                  _client.Tab.autowalker_locales.Text = "Lost Ruins";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Marble Vault";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.LastnpcpopupID = npcByName.ID;
                }
                else if (_client.Currentnpctext.StartsWith("Great job on taking notes on the wall tablets.") || _client.Currentnpctext.StartsWith("Go find the altar."))
                {
                  _client.PopupClose(new uint?(_client.CurrentnpcpopupID));
                  _client.Tab.autowalker_locales.Text = "Lost Ruins";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"LR5 Altar";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.LastnpcpopupID = npcByName.ID;
                }
                else if (_client.Currentnpctext.StartsWith("Great news! I found the altar.") || _client.Currentnpctext.StartsWith("Go back to the altar and perform that ritual."))
                {
                  _client.PopupClose(new uint?(_client.CurrentnpcpopupID));
                  _client.Tab.autowalker_locales.Text = "Lost Ruins";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Dung Field";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.LastnpcpopupID = npcByName.ID;
                }
                else if (_client.Currentnpctext.StartsWith("From the look on your face, I guess you were successful.") || _client.Currentnpctext.StartsWith("Go find the FINAL Altar."))
                {
                  _client.PopupClose(new uint?(_client.CurrentnpcpopupID));
                  _client.Tab.autowalker_locales.Text = "Lost Ruins";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Law";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.LastnpcpopupID = npcByName.ID;
                }
                else if (_client.Currentnpctext.StartsWith("Guess who we encountered after performing the ritual?"))
                {
                  _client.lawwall = 0;
                  _client.PopupClose(new uint?(_client.CurrentnpcpopupID));
                  _client.SendMessage("DONE!");
                  _client.LastnpcpopupID = npcByName.ID;
                  _client.lawquest = false;
                }
                else
                {
                  while (!_client.Currentnpctext.StartsWith("Great.  Here's what I know about"))
                  {
                    if (_client.Currentnpctext.StartsWith("I will not accept your notes if they are out of order"))
                    {
                      _client.PopupClose(new uint?(_client.CurrentnpcpopupID));
                      _client.Tab.autowalker_locales.Text = "Lost Ruins";
                      _client.Tab.walklocaleslist.SelectedItem = (object)"Marble Vault";
                      _client.Tab.autowalker_button.Text = "Stop";
                      _client.autowalkon = true;
                      goto label_1712;
                    }
                    else
                    {
                      if (_client.Currentnpctext.StartsWith("Do you want to help me discover the Lost Ruins?"))
                        _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
                      else if (_client.Currentnpctext != "")
                        _client.PopupNext(new uint?(_client.CurrentnpcpopupID));
                      if (!_client.pause)
                      {
                        Thread.Sleep(200);
                        if (_client.Currentpopuptype == 10)
                          goto label_1712;
                      }
                      else
                        goto label_1712;
                    }
                  }
                  _client.PopupClose(new uint?(_client.CurrentnpcpopupID));
                  _client.Tab.autowalker_locales.Text = "Lost Ruins";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"LR2 Rocks";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                label_1712:
                  _client.LastnpcpopupID = npcByName.ID;
                }
              }
            }
            else if (_client.MapInfo.Number == 8994 || _client.MapInfo.Number == 8988 || _client.MapInfo.Number == 8990)
            {
              if (_client.MapInfo.Number == 8994 && _client.Tab.autowalker_button.Text == "Start")
                _client.SearchAllTiles(6, 13, 17, 24);
              if (_client.MapInfo.Number == 8988)
              {
                if (_client.lawwall == 0)
                  _client.WalkToExact(25, 13);
                else if (_client.lawwall == 1)
                  _client.WalkToExact(2, 25);
                else if (_client.lawwall == 2)
                  _client.WalkToExact(13, 19);
                else if (_client.lawwall == 3)
                  _client.WalkToExact(8, 2);
                else if (_client.lawwall == 4)
                  _client.WalkToExact(13, 25);
                else if (_client.lawwall == 5)
                  _client.WalkToExact(2, 8);
                else if (_client.lawwall == 6)
                  _client.WalkToExact(16, 13);
                else if (_client.lawwall == 7 && _client.SpellBar.Contains((ushort)10))
                  _client.FindAutoWalkPath(8995);
              }
              if (_client.Currentnpctext != "")
              {
                _client.Currentnpctext.StartsWith("Finally!!! I'm glad this is the last Wall Tablet I have to take notes on.");
                if (_client.Currentnpctext.StartsWith("This must be the wall markings that Nairn was talking about."))
                  _client.SendMessage("I got the lame popup =(");
                if (_client.Currentnpctext.StartsWith("You notice an odd arrangement of rocks"))
                {
                  _client.Tab.autowalker_locales.Text = "Lost Ruins";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Nairn";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                }
                _client.PopupClose(new uint?(_client.CurrentnpcpopupID), (byte)4);
                _client.LastnpcpopupID = 0U;
              }
            }
            else if (_client.MapInfo.Number == 8993 || _client.MapInfo.Number == 8989 || _client.MapInfo.Number == 8987)
            {
              if (_client.MapInfo.Number == 8993 && _client.Tab.autowalker_button.Text == "Start")
                _client.SearchAllTiles(16, 27, 6, 17);
              if (_client.MapInfo.Number == 8989 && _client.Tab.autowalker_button.Text == "Start")
                _client.SearchAllTiles(7, 19, 5, 17);
              if (_client.MapInfo.Number == 8987 && _client.Tab.autowalker_button.Text == "Start" && _client.Tab.walklocaleslist.SelectedItem.ToString() != "Dung Field")
                _client.SearchAllTiles(5, 14, 7, 15);
              if (_client.MapInfo.Number == 8989 && _client.Tab.autowalker_button.Text == "Start" && _client.lawwall > 0 && _client.lawwall < 7)
                _client.WalkToExact(45, 2);
              if (_client.Currentnpctext != "")
              {
                if (_client.Currentnpctext.StartsWith("What is this? It looks scary down there!"))
                  _client.PopupNext(new uint?(_client.CurrentnpcpopupID), (byte)4);
                else if (_client.Currentnpctext.StartsWith("This must be the underground passage."))
                  _client.PopupNext(new uint?(_client.CurrentnpcpopupID), (byte)4);
                else if (_client.Currentnpctext != "")
                {
                  _client.PopupClose(new uint?(_client.CurrentnpcpopupID), (byte)4);
                  _client.Tab.autowalker_locales.Text = "Lost Ruins";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Nairn";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                }
                _client.LastnpcpopupID = _client.MapInfo.Number != 8989 ? 0U : 1U;
              }
            }
          }
          if (!_client.pause)
          {
            if (_client.MapInfo.Number == 8296)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Vortigern");
              if (npcByName != null && npcByName.IsOnScreen && _client.ajquest == 1)
              {
                _client.ClickNpc(npcByName.ID);
                while (_client.Currentnpctext == string.Empty)
                  Thread.Sleep(100);
              }
              if (_client.Currentnpctext.StartsWith("Hi! My name is Vortigern."))
              {
                _client.ajquest = 2;
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 11; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
                Thread.Sleep(10);
                for (int index = 0; index < 13; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupClose(new uint?(currentnpcpopupId));
                Thread.Sleep(1000);
                if (_client.HasItem("Scrap of Clothing") && _client.ItemAmount("Scrap of Clothing") >= 20U)
                {
                  _client.Tab.autowalker_locales.SelectedItem = (object)"Aman Jungle";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Bank";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                }
                else
                  _client.SendMessage("Get 20 Scrap of Clothing, and go to AJ bank");
              }
              else if (_client.Currentnpctext.StartsWith("From the look on your face,"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 24; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.SendMessage("Wait 24 hours to receive the Note.", "orange");
              }
              else if (_client.Currentnpctext.StartsWith("Here is the note."))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 3; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupClose(new uint?(currentnpcpopupId));
                Thread.Sleep(1000);
                if (!_client.SpellBar.Contains((ushort)10))
                {
                  _client.UseItem("Hostile Headgear");
                  _client.UseItem("Male Hostile Costume");
                  _client.UseItem("Female Hostile Costume");
                  if (_client.HasItem("Elemus Mount") && (int)_client.ClientForm - 16384 != 658 && (int)_client.ClientForm - 16384 != 659)
                    _client.UseItem("Elemus Mount");
                }
                Thread.Sleep(500);
                _client.Tab.autowalker_locales.SelectedItem = (object)"Aman Jungle";
                _client.Tab.walklocaleslist.SelectedItem = (object)"AJ 0 End";
                _client.Tab.autowalker_button.Text = "Stop";
                _client.autowalkon = true;
                _client.SendMessage("*walking to AJ ent* - Go to HG end");
              }
              else if (_client.Currentnpctext.StartsWith("You have good news?"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 16; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.SendMessage("AJ quest chain finished!", "orange");
              }
            }
            if (_client.MapInfo.Number == 8295)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Jovino");
              if (npcByName != null && npcByName.IsOnScreen && (int)_client.LastnpcpopupID != (int)npcByName.ID)
              {
                _client.LastnpcpopupID = npcByName.ID;
                _client.DialogueRespond(npcByName.ID, (byte)13, (byte)81);
                while (_client.Currentnpctext == string.Empty)
                  Thread.Sleep(100);
              }
              if (_client.Currentnpctext.StartsWith("Hi there, are you old enough to be here?"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 6; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
                Thread.Sleep(10);
                for (int index = 0; index < 5; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
                Thread.Sleep(10);
                for (int index = 0; index < 3; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
                Thread.Sleep(10);
                for (int index = 0; index < 6; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                Thread.Sleep(1000);
                _client.Tab.autowalker_locales.SelectedItem = (object)"Aman Jungle";
                _client.Tab.walklocaleslist.SelectedItem = (object)"AJ 0 End";
                _client.Tab.autowalker_button.Text = "Stop";
                _client.autowalkon = true;
                _client.SendMessage("*walking to AJ ent* - Go to AJ 8 (dendrons)");
              }
              else if (_client.Currentnpctext.StartsWith("I can't believe you killed"))
              {
                _client.ajquest = 90;
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 4; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupClose(new uint?(currentnpcpopupId));
                Thread.Sleep(1000);
                _client.Tab.autowalker_locales.SelectedItem = (object)"Aman Jungle";
                _client.Tab.walklocaleslist.SelectedItem = (object)"Oriana";
                _client.Tab.autowalker_button.Text = "Stop";
                _client.autowalkon = true;
              }
            }
            if (_client.MapInfo.Number == 8299)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Glenna");
              if (npcByName != null && npcByName.IsOnScreen && _client.ajquest == 2)
              {
                _client.DialogueRespond(npcByName.ID, (byte)13, (byte)67);
                while (_client.Currentnpctext == string.Empty)
                  Thread.Sleep(100);
              }
              if (_client.Currentnpctext.StartsWith("Hi. Do you need anything?"))
              {
                _client.ajquest = 3;
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 5; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
                Thread.Sleep(10);
                for (int index = 0; index < 6; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupClose(new uint?(currentnpcpopupId));
                _client.SendMessage("Wait 24 hours to receive Hostile Clothing.", "orange");
              }
              else if (_client.Currentnpctext.StartsWith("Ahhh, here for your new outfit?"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 2; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupClose(new uint?(currentnpcpopupId));
                _client.ajquest = 3;
                Thread.Sleep(1000);
                if (!_client.SpellBar.Contains((ushort)10))
                {
                  _client.UseItem("Hostile Headgear");
                  _client.UseItem("Male Hostile Costume");
                  _client.UseItem("Female Hostile Costume");
                  if (_client.HasItem("Elemus Mount") && (int)_client.ClientForm - 16384 != 658 && (int)_client.ClientForm - 16384 != 659)
                    _client.UseItem("Elemus Mount");
                }
                Thread.Sleep(500);
                _client.Tab.autowalker_locales.SelectedItem = (object)"Aman Jungle";
                _client.Tab.walklocaleslist.SelectedItem = (object)"AJ 0 End";
                _client.Tab.autowalker_button.Text = "Stop";
                _client.autowalkon = true;
                _client.SendMessage("*walking to AJ ent* - Go to HG end");
              }
            }
            if (_client.MapInfo.Number == 8306)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Weylin");
              if (npcByName != null && npcByName.IsOnScreen && _client.Tab.vwalklocaleslist.Equals("AJ 6"))
              {
                _client.Speak("oriana", 2);
                _client.repeatspeech = DateTime.UtcNow;
                while (_client.MapInfo.Number != 8319)
                {
                  if (_client.repeatspeech != DateTime.MinValue && DateTime.UtcNow.Subtract(_client.repeatspeech).TotalSeconds > 1.0)
                  {
                    _client.Speak("oriana", 2);
                    _client.repeatspeech = DateTime.UtcNow;
                  }
                  Thread.Sleep(200);
                  if (!(_client.Currentnpctext == string.Empty))
                    break;
                }
              }
              if (_client.Currentnpctext.StartsWith("Good thing you know Oriana"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 3; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
              }
              if (_client.Currentnpctext.StartsWith("I heard you helped Oriana"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 2; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
              }
            }
            if (_client.MapInfo.Number == 8329)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Hostile Chief");
              if (npcByName != null && npcByName.IsOnScreen && !_client.autowalkon)
              {
                if (_client.ajquest == 3)
                {
                  _client.ClickNpc(npcByName.ID);
                  while (_client.Currentnpctext == string.Empty)
                    Thread.Sleep(100);
                }
                if (_client.ajquest == 7)
                {
                  _client.ClickNpc(npcByName.ID);
                  while (_client.Currentnpctext == string.Empty)
                    Thread.Sleep(100);
                }
              }
              if (_client.Currentnpctext.StartsWith("You dare speak to me in that tone!"))
              {
                _client.ajquest = 4;
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 19; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
                Thread.Sleep(10);
                for (int index = 0; index < 2; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupClose(new uint?(currentnpcpopupId));
                Thread.Sleep(1000);
                _client.SendMessage("Go say Ugliest Mask to Oriana");
              }
              else if (_client.Currentnpctext.StartsWith("You have returned!"))
              {
                _client.ajquest = 8;
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 2; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
                Thread.Sleep(10);
                for (int index = 0; index < 6; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupClose(new uint?(currentnpcpopupId));
                Thread.Sleep(1000);
                _client.Speak("tell me the plan", 2);
              }
              else if (_client.Currentnpctext.StartsWith("Okay I don't normal share my plans with anybody,"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 21; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.SendMessage("Go talk to Vortigern");
              }
              else if (_client.Currentnpctext.StartsWith("What do you know about the Note?"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 20; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupClose(new uint?(currentnpcpopupId));
                Thread.Sleep(1000);
                _client.SendMessage("Go talk to Vortigern");
              }
            }
            if (_client.MapInfo.Number == 8300)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Oriana");
              if (npcByName != null && npcByName.IsOnScreen && (int)_client.LastnpcpopupID != (int)npcByName.ID && !_client.autowalkon && (_client.ajquest == 90 || _client.ajquest == 91))
              {
                _client.LastnpcpopupID = npcByName.ID;
                _client.Speak("jovino");
                while (_client.Currentnpctext == string.Empty)
                  Thread.Sleep(100);
              }
              if (_client.Currentnpctext.StartsWith("Hi! You seem to know my dad's name."))
              {
                _client.ajquest = 91;
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 19; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
                Thread.Sleep(10);
                for (int index = 0; index < 5; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupClose(new uint?(currentnpcpopupId));
                Thread.Sleep(1000);
                _client.Tab.autowalker_locales.SelectedItem = (object)"Aman Jungle";
                _client.Tab.walklocaleslist.SelectedItem = (object)"AJ 0 End";
                _client.Tab.autowalker_button.Text = "Stop";
                _client.autowalkon = true;
                _client.SendMessage("*walking to AJ ent* - Go to AJ 6");
              }
              else if (_client.Currentnpctext.StartsWith("Did you find him?"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 3; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupClose(new uint?(currentnpcpopupId));
                _client.SendMessage("Wait 24 hours to receive Elemus Mount.", "orange");
              }
              else if (_client.Currentnpctext.StartsWith("Here is your surprise."))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 2; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupClose(new uint?(currentnpcpopupId));
                _client.ajquest = 1;
                Thread.Sleep(1000);
                _client.Tab.autowalker_locales.SelectedItem = (object)"Aman Jungle";
                _client.Tab.walklocaleslist.SelectedItem = (object)"Vortigern";
                _client.Tab.autowalker_button.Text = "Stop";
                _client.autowalkon = true;
              }
              else if (_client.Currentnpctext.StartsWith("Hey, "))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 7; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
                Thread.Sleep(10);
                for (int index = 0; index < 11; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupClose(new uint?(currentnpcpopupId));
                Thread.Sleep(1000);
                if (_client.ItemAmount("Crystal Bar") >= 20U)
                  _client.Speak("ugliest mask", 2);
                else
                  _client.SendMessage("You need 20 Crystal Bars", "red");
              }
              else if (_client.Currentnpctext.StartsWith("Back so soon?"))
              {
                _client.ajquest = 7;
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                _client.PopupNext(new uint?(currentnpcpopupId));
                Thread.Sleep(10);
                _client.PopupClose(new uint?(currentnpcpopupId));
                Thread.Sleep(1000);
                if (!_client.SpellBar.Contains((ushort)10))
                {
                  _client.UseItem("Hostile Headgear");
                  _client.UseItem("Male Hostile Costume");
                  _client.UseItem("Female Hostile Costume");
                  if (_client.HasItem("Elemus Mount") && (int)_client.ClientForm - 16384 != 658 && (int)_client.ClientForm - 16384 != 659)
                    _client.UseItem("Elemus Mount");
                }
                Thread.Sleep(500);
                _client.Tab.autowalker_locales.SelectedItem = (object)"Aman Jungle";
                _client.Tab.walklocaleslist.SelectedItem = (object)"AJ 0 End";
                _client.Tab.autowalker_button.Text = "Stop";
                _client.autowalkon = true;
                _client.SendMessage("*walking to AJ ent* - Go to HG End");
              }
            }
            if (_client.MapInfo.Number == 8337)
            {
              if (!_client.autowalkon)
              {
                Npc npcByName = _client.FindNpcByName<Npc>("Kharlo");
                if (npcByName != null && npcByName.IsOnScreen)
                {
                  if (_client.ytquest == 2)
                  {
                    _client.Speak("jowella is mad at you");
                    while (_client.Currentnpctext == string.Empty)
                      Thread.Sleep(100);
                  }
                  else if (_client.ytquest == 4)
                  {
                    _client.Speak("favorite flower");
                    while (_client.Currentnpctext == string.Empty)
                      Thread.Sleep(100);
                  }
                  else if (_client.ytquest == 7)
                  {
                    _client.Speak("dendron bouquet");
                    while (_client.Currentnpctext == string.Empty)
                      Thread.Sleep(100);
                  }
                  else if (_client.ytquest == 9)
                  {
                    _client.Speak("she forgives you");
                    while (_client.Currentnpctext == string.Empty)
                      Thread.Sleep(100);
                  }
                }
              }
              if (_client.Currentnpctext.StartsWith("Thank you for waking me up."))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 20; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                Thread.Sleep(1000);
                _client.Tab.autowalker_locales.SelectedItem = (object)"Aman Jungle";
                _client.Tab.walklocaleslist.SelectedItem = (object)"YT ent";
                _client.Tab.autowalker_button.Text = "Stop";
                _client.autowalkon = true;
                _client.ytquest = 1;
              }
              else if (_client.Currentnpctext.StartsWith("Who told you that name?"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 14; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                Thread.Sleep(1000);
                _client.Tab.autowalker_locales.SelectedItem = (object)"Aman Jungle";
                _client.Tab.walklocaleslist.SelectedItem = (object)"YT ent";
                _client.Tab.autowalker_button.Text = "Stop";
                _client.autowalkon = true;
                _client.ytquest = 3;
              }
              else if (_client.Currentnpctext.StartsWith("Huh? What are you talking about?"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 17; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                Thread.Sleep(1000);
                _client.Tab.autowalker_locales.SelectedItem = (object)"Aman Jungle";
                _client.Tab.walklocaleslist.SelectedItem = (object)"YT ent";
                _client.Tab.autowalker_button.Text = "Stop";
                _client.autowalkon = true;
                _client.ytquest = 5;
              }
              else if (_client.Currentnpctext.StartsWith("Wow! I forgot how beautiful it looks."))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                _client.PopupNext(new uint?(currentnpcpopupId));
                Thread.Sleep(10);
                _client.PopupClose(new uint?(currentnpcpopupId));
                Thread.Sleep(1000);
                _client.Tab.autowalker_locales.SelectedItem = (object)"Aman Jungle";
                _client.Tab.walklocaleslist.SelectedItem = (object)"YT ent";
                _client.Tab.autowalker_button.Text = "Stop";
                _client.autowalkon = true;
                _client.ytquest = 8;
              }
              else if (_client.Currentnpctext.StartsWith("Did she really forgive me?"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 19; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                Thread.Sleep(1000);
                _client.Tab.autowalker_locales.SelectedItem = (object)"Aman Jungle";
                _client.Tab.walklocaleslist.SelectedItem = (object)"YT ent";
                _client.Tab.autowalker_button.Text = "Stop";
                _client.autowalkon = true;
                _client.ytquest = 10;
              }
              else if (_client.Currentnpctext.StartsWith("Are you joking?"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 3; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupClose(new uint?(currentnpcpopupId));
                Thread.Sleep(1000);
                _client.Tab.autowalker_locales.SelectedItem = (object)"Aman Jungle";
                _client.Tab.walklocaleslist.SelectedItem = (object)"YT ent";
                _client.Tab.autowalker_button.Text = "Stop";
                _client.autowalkon = true;
                _client.ytquest = 70;
              }
            }
            if (_client.MapInfo.Number == 8347)
            {
              if (!_client.autowalkon)
              {
                Npc npcByName = _client.FindNpcByName<Npc>("Jowella");
                if (npcByName != null && npcByName.IsOnScreen)
                {
                  if (_client.ytquest == 1)
                  {
                    _client.Speak("i know kharlo");
                    while (_client.Currentnpctext == string.Empty)
                      Thread.Sleep(100);
                  }
                  else if (_client.ytquest == 3)
                  {
                    _client.Speak("he is apologizing");
                    while (_client.Currentnpctext == string.Empty)
                      Thread.Sleep(100);
                  }
                  else if (_client.ytquest == 8)
                  {
                    _client.Speak("dendron bouquet");
                    while (_client.Currentnpctext == string.Empty)
                      Thread.Sleep(100);
                  }
                }
              }
              if (_client.Currentnpctext.StartsWith("Whaaaa! You know Kharlo?"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 8; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                Thread.Sleep(1000);
                _client.Tab.autowalker_locales.SelectedItem = (object)"Aman Jungle";
                _client.Tab.walklocaleslist.SelectedItem = (object)"YT ent";
                _client.Tab.autowalker_button.Text = "Stop";
                _client.autowalkon = true;
                _client.ytquest = 2;
              }
              else if (_client.Currentnpctext.StartsWith("Did he really say that?"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 12; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                Thread.Sleep(1000);
                _client.Tab.autowalker_locales.SelectedItem = (object)"Aman Jungle";
                _client.Tab.walklocaleslist.SelectedItem = (object)"YT ent";
                _client.Tab.autowalker_button.Text = "Stop";
                _client.autowalkon = true;
                _client.ytquest = 4;
              }
              else if (_client.Currentnpctext.StartsWith("Yaaay! Kharlo still remembers my favorite bouquet!"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 6; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupClose(new uint?(currentnpcpopupId));
                Thread.Sleep(1000);
                _client.Tab.autowalker_locales.SelectedItem = (object)"Aman Jungle";
                _client.Tab.walklocaleslist.SelectedItem = (object)"YT ent";
                _client.Tab.autowalker_button.Text = "Stop";
                _client.autowalkon = true;
                _client.ytquest = 9;
              }
            }
            if (_client.MapInfo.Number == 8348)
            {
              if (!_client.autowalkon && _client.WithinRange(53, 50, 21))
              {
                if (_client.ytquest == 13)
                {
                  _client.Speak("yowien fishing pole", 2);
                  while (_client.Currentnpctext == string.Empty)
                    Thread.Sleep(100);
                }
                else if (_client.ytquest == 14 && _client.ItemAmount("Yowien Fish") >= 20U)
                {
                  _client.Speak("yowien fishes", 2);
                  while (_client.Currentnpctext == string.Empty)
                    Thread.Sleep(100);
                }
              }
              if (_client.Currentnpctext.StartsWith("How did you find me?"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 11; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.ytquest = 14;
                Thread.Sleep(1000);
              }
              else if (_client.Currentnpctext.StartsWith("Yummy!"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 2; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupClose(new uint?(currentnpcpopupId));
                Thread.Sleep(1000);
                _client.Tab.autowalker_locales.SelectedItem = (object)"Aman Jungle";
                _client.Tab.walklocaleslist.SelectedItem = (object)"YT 5";
                _client.Tab.autowalker_button.Text = "Stop";
                _client.autowalkon = true;
                _client.ytquest = 15;
              }
            }
            if (_client.MapInfo.Number == 8349)
            {
              if (!_client.autowalkon)
              {
                Npc npcByName = _client.FindNpcByName<Npc>("Luwella");
                if (npcByName != null && npcByName.IsOnScreen)
                {
                  if (_client.ytquest == 12)
                  {
                    _client.Speak("why are you crying?");
                    while (_client.Currentnpctext == string.Empty)
                      Thread.Sleep(100);
                  }
                  else if (_client.HasItem("Luwella's House Key"))
                  {
                    _client.Speak("house key");
                    while (_client.Currentnpctext == string.Empty)
                      Thread.Sleep(100);
                  }
                  else if (_client.ytquest == 17)
                  {
                    _client.Speak("can i come in?");
                    while (_client.Currentnpctext == string.Empty)
                      Thread.Sleep(100);
                  }
                }
              }
              if (_client.Currentnpctext.StartsWith("You're the first aisling to ask me that."))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 21; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                Thread.Sleep(1000);
                _client.ytquest = 13;
                if (!_client.SpellBar.Contains((ushort)10))
                {
                  _client.SendMessage("Enter Yt 4 to make Fishing Pole (get hide first!)", "red");
                }
                else
                {
                  _client.Tab.autowalker_locales.SelectedItem = (object)"Aman Jungle";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"YT 4";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                }
              }
              else if (_client.Currentnpctext.StartsWith("Thank you so much for finding my house key."))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 2; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupClose(new uint?(currentnpcpopupId));
                _client.Speak("can i come in?");
                _client.ytquest = 17;
              }
              else if (_client.Currentnpctext.StartsWith("Of course. Come in."))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 2; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                if (_client.ytquest == 17)
                  _client.ytquest = 18;
              }
            }
            if (_client.MapInfo.Number == 8370)
            {
              if (!_client.autowalkon)
              {
                Npc npcByName = _client.FindNpcByName<Npc>("Granny");
                if (npcByName != null && npcByName.IsOnScreen)
                {
                  if (_client.ytquest == 18)
                  {
                    _client.Speak("hello granny");
                    while (_client.Currentnpctext == string.Empty)
                      Thread.Sleep(100);
                  }
                  else if (_client.ytquest == 19)
                  {
                    _client.Speak("i can protect luwella");
                    while (_client.Currentnpctext == string.Empty)
                      Thread.Sleep(100);
                  }
                  else if (_client.ytquest == 20)
                  {
                    _client.Speak("i am all done");
                    while (_client.Currentnpctext == string.Empty)
                      Thread.Sleep(100);
                  }
                  else if (_client.ytquest == 21)
                  {
                    _client.Speak("wake up granny!");
                    while (_client.Currentnpctext == string.Empty)
                      Thread.Sleep(100);
                  }
                  else if (_client.ytquest == 22 && _client.ItemAmount("Yowien Blue Vine") >= 25U && _client.ItemAmount("Yowien Yellow Vine") >= 25U)
                  {
                    _client.Speak("i have the vines");
                    while (_client.Currentnpctext == string.Empty)
                      Thread.Sleep(100);
                  }
                }
              }
              if (_client.Currentnpctext.StartsWith("How did you get in here?"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 17; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.ytquest = 19;
                Thread.Sleep(1000);
              }
              else if (_client.Currentnpctext.StartsWith("Really?"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 7; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                if (_client.ytquest != 19)
                {
                  _client.PopupClose(new uint?(currentnpcpopupId));
                  _client.ytquest = 21;
                  Thread.Sleep(1000);
                }
                else
                {
                  Thread.Sleep(1000);
                  _client.Tab.autowalker_locales.SelectedItem = (object)"Aman Jungle";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"YT 5";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.ytquest = 20;
                }
              }
              else if (_client.Currentnpctext.StartsWith("Oh...sorry. Did I fall asleep?"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 24; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.ytquest = 22;
                Thread.Sleep(1000);
              }
              else if (_client.Currentnpctext.StartsWith("Good job. Let me see"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 9; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupClose(new uint?(currentnpcpopupId));
                _client.ytquest = 23;
              }
            }
            if (_client.MapInfo.Number == 8350)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Kheven");
              if (npcByName != null && npcByName.IsOnScreen)
              {
                if ((_client.ytquest == 5 || _client.ytquest == 70) && !_client.autowalkon)
                {
                  _client.Speak("dendron bouquet");
                  while (_client.Currentnpctext == string.Empty)
                    Thread.Sleep(100);
                }
                else if (_client.ytquest == 6 && DateTime.UtcNow.Subtract(_client.KhevenTimer).TotalSeconds > 601.0)
                {
                  _client.Speak("dendron bouquet");
                  while (_client.Currentnpctext == string.Empty)
                    Thread.Sleep(100);
                }
              }
              if (_client.Currentnpctext.StartsWith("I haven't heard someone shout that in a long time"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 15; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupClose(new uint?(currentnpcpopupId));
                _client.ytquest = 6;
                _client.KhevenTimer = DateTime.UtcNow;
                _client.SendMessage("Wait here 10 minutes, walk the hider to yt5 for rehide");
              }
              else if (_client.Currentnpctext.StartsWith("Here it is! It's perfection if I have to say so myself."))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                _client.PopupNext(new uint?(currentnpcpopupId));
                Thread.Sleep(10);
                _client.PopupClose(new uint?(currentnpcpopupId));
                Thread.Sleep(1000);
                _client.UseItem("Dendron Bouquet");
                _client.Tab.autowalker_locales.SelectedItem = (object)"Aman Jungle";
                _client.Tab.walklocaleslist.SelectedItem = (object)"YT 5";
                _client.Tab.autowalker_button.Text = "Stop";
                _client.autowalkon = true;
                _client.ytquest = 7;
                _client.KhevenTimer = DateTime.MinValue;
              }
              else if (_client.Currentnpctext.StartsWith("Hmmm...let me guess,"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 4; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)2);
                for (int index = 0; index < 4; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupClose(new uint?(currentnpcpopupId));
                Thread.Sleep(2000);
                _client.Assail();
                _client.UseItem("Dendron Bouquet");
                Thread.Sleep(500);
                _client.Tab.autowalker_locales.SelectedItem = (object)"Aman Jungle";
                _client.Tab.walklocaleslist.SelectedItem = (object)"YT 5";
                _client.Tab.autowalker_button.Text = "Stop";
                _client.autowalkon = true;
                _client.ytquest = 7;
              }
            }
            if (_client.MapInfo.Number == 8355)
            {
              if (!_client.autowalkon)
              {
                Npc npcByName = _client.FindNpcByName<Npc>("Norrie");
                if (npcByName != null && npcByName.IsOnScreen)
                {
                  if (_client.ytquest == 10)
                  {
                    _client.ClickNpc(npcByName.ID);
                    while (_client.Currentnpctext == string.Empty)
                      Thread.Sleep(100);
                  }
                  else if (_client.ytquest == 11 && _client.HasItem("Crystal Orb"))
                  {
                    _client.Speak("crystal orb");
                    while (_client.Currentnpctext == string.Empty)
                      Thread.Sleep(100);
                  }
                }
              }
              if (_client.Currentnpctext.StartsWith("Yes, I'm the great Norrie, the fortune teller."))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 15; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.ytquest = 11;
                Thread.Sleep(1000);
              }
              else if (_client.Currentnpctext.StartsWith("You're back!"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                for (int index = 0; index < 12; ++index)
                {
                  _client.PopupNext(new uint?(currentnpcpopupId));
                  Thread.Sleep(10);
                }
                _client.PopupClose(new uint?(currentnpcpopupId));
                Thread.Sleep(1000);
                _client.Tab.autowalker_locales.SelectedItem = (object)"Aman Jungle";
                _client.Tab.walklocaleslist.SelectedItem = (object)"YT 5";
                _client.Tab.autowalker_button.Text = "Stop";
                _client.autowalkon = true;
                _client.ytquest = 12;
              }
            }
          }
          if (!_client.pause && _client.Tab.vautowalker_locales.Equals("Aman Jungle") && _client.Tab.autowalker_button.Text == "Stop")
          {
            if (_client.MapInfo.Number == 8355 && _client.WithinRange(36, 3, 11) && (_client.Tab.vwalklocaleslist.Equals("YT 15") || _client.Tab.vwalklocaleslist.Equals("Yellow Vines") || _client.Tab.vwalklocaleslist.Equals("YT 12")))
            {
              _client.Speak("let me through", 2);
              _client.repeatspeech = DateTime.UtcNow;
              do
              {
                if (_client.repeatspeech != DateTime.MinValue && DateTime.UtcNow.Subtract(_client.repeatspeech).TotalSeconds > 1.0)
                {
                  _client.Speak("let me through", 2);
                  _client.repeatspeech = DateTime.UtcNow;
                }
                Thread.Sleep(200);
              }
              while (_client.Currentnpctext == string.Empty);
              if (_client.Currentnpctext.Contains("Sure") && _client.Currentnpctext.Contains("Not again") && !_client.HasItem("Crystal Orb"))
              {
                _client.Tab.autowalker_button.Text = "Start";
                _client.autowalkon = false;
                _client.PopupClose(new uint?(_client.CurrentnpcpopupID));
                _client.SendMessage("You have no Crystal Orb!", "red");
              }
              else if (_client.Currentnpctext.Contains("Sure") && _client.Currentnpctext.Contains("Not again") && _client.HasItem("Crystal Orb"))
              {
                uint currentnpcpopupId = _client.CurrentnpcpopupID;
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
                Thread.Sleep(1000);
                _client.Speak("let me through", 2);
              }
              if (_client.Currentnpctext.StartsWith("Sorry I forgot"))
              {
                do
                {
                  Thread.Sleep(200);
                  if (_client.Currentnpctext != string.Empty)
                    _client.PopupNext(new uint?(_client.CurrentnpcpopupID));
                }
                while (_client.Currentpopuptype != 10);
              }
            }
            else if (_client.Tab.vwalklocaleslist.Equals("HG end"))
            {
              if (_client.MapInfo.Number == 8310 && _client.SpellBar.Contains((ushort)3))
              {
                Npc npcByName = _client.FindNpcByName<Npc>("Elemus Guard");
                if (npcByName != null && npcByName.IsOnScreen)
                {
                  _client.ClickNpc(npcByName.ID);
                  Thread.Sleep(200);
                  _client.PopupNext(new uint?(npcByName.ID));
                  _client.PopupNext(new uint?(npcByName.ID));
                  Thread.Sleep(800);
                }
              }
              else if (_client.MapInfo.Number == 8328)
              {
                Npc npcByName = _client.FindNpcByName<Npc>("Hostile Guard");
                if (npcByName != null && npcByName.IsOnScreen)
                {
                  _client.ClickNpc(npcByName.ID);
                  Thread.Sleep(200);
                  _client.PopupNext(new uint?(npcByName.ID));
                  _client.PopupClose(new uint?(npcByName.ID));
                  Thread.Sleep(800);
                }
              }
            }
            else if (_client.MapInfo.Number == 8318 && (_client.Tab.vwalklocaleslist.Equals("YT 3") || _client.Tab.vwalklocaleslist.Equals("YT 5") || _client.Tab.vwalklocaleslist.Equals("YT 6") || _client.Tab.vwalklocaleslist.Equals("Yellow Vines") || _client.Tab.vwalklocaleslist.Equals("YT 15") || _client.Tab.vwalklocaleslist.Equals("YT 11") || _client.Tab.vwalklocaleslist.Equals("YT 12")))
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Ashlee");
              if (npcByName != null && npcByName.IsOnScreen)
              {
                _client.ClickNpc(npcByName.ID);
                Thread.Sleep(200);
                _client.PopupNext(new uint?(npcByName.ID));
                Thread.Sleep(800);
              }
            }
            else if (_client.MapInfo.Number == 8361 && _client.WithinRange(29, 4, 11) && _client.Tab.vwalklocaleslist.Equals("YT Boss"))
            {
              _client.Speak("graauuloow", 2);
              _client.repeatspeech = DateTime.UtcNow;
              do
              {
                if (_client.repeatspeech != DateTime.MinValue && DateTime.UtcNow.Subtract(_client.repeatspeech).TotalSeconds > 1.0)
                {
                  _client.Speak("graauuloow", 2);
                  _client.repeatspeech = DateTime.UtcNow;
                }
                Thread.Sleep(200);
              }
              while (_client.Currentnpctext == string.Empty);
              do
              {
                Thread.Sleep(200);
                if (_client.Currentnpctext != string.Empty)
                  _client.PopupNext(new uint?(_client.CurrentnpcpopupID));
              }
              while (_client.Currentpopuptype != 10);
            }
          }
          if (_client.meditate && _client.Currentnpctext == "")
          {
            if (_client.MapInfo.Number == 5210)
              _client.WalkToExact(5, 0);
            if (_client.MapInfo.Number == 5219)
            {
              if (_client.distracted)
              {
                _client.Refresh();
                _client.distracted = false;
              }
              if (_client.mwhite)
                _client.WalkToExact(4, 4);
              if (_client.mgreen)
                _client.WalkToExact(13, 9);
              if (_client.mblue)
                _client.WalkToExact(4, 12);
              if (_client.myellow)
                _client.WalkToExact(9, 3);
              if (_client.mpurple)
                _client.WalkToExact(11, 13);
              if (_client.mbrown)
                _client.WalkToExact(3, 7);
              if (_client.mred)
                _client.WalkToExact(13, 5);
              if (_client.mblack)
                _client.WalkToExact(8, 14);
            }
          }
          if (_client.meditatedone && _client.Currentnpctext == "")
          {
            if (_client.MapInfo.Number == 5219)
              _client.WalkToExact(8, 16);
            if (_client.MapInfo.Number == 5210)
            {
              if (!_client.WithinRange(5, 5, 2))
              {
                _client.WalkWithinRange(5, 5, 2);
              }
              else
              {
                _client.Speak("sabonim, i understand the " + _client.currentdugon + " dugon");
                _client.meditatedone = false;
              }
            }
          }
          if (_client.Currentnpctext.StartsWith("You must defeat a Rock Cobbler"))
          {
            uint currentnpcpopupId = _client.CurrentnpcpopupID;
            _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)232, (byte)0, (byte)202);
            _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)232, (byte)0, (byte)228);
            _client.meditate = true;
            _client.mblack = true;
          }
          if (_client.Currentnpctext.StartsWith("You must defeat a Faerie"))
          {
            uint currentnpcpopupId = _client.CurrentnpcpopupID;
            _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)227, (byte)0, (byte)203);
            _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)227, (byte)0, (byte)215);
            _client.meditate = true;
            _client.mred = true;
          }
          if (_client.Currentnpctext.StartsWith("You must defeat a Shrieker"))
          {
            uint currentnpcpopupId = _client.CurrentnpcpopupID;
            _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)222, (byte)0, (byte)204);
            _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)222, (byte)0, (byte)232);
            _client.meditate = true;
            _client.mbrown = true;
          }
          if (_client.Currentnpctext.StartsWith("You must defeat a Leech"))
          {
            uint currentnpcpopupId = _client.CurrentnpcpopupID;
            _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)217, (byte)0, (byte)205);
            _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)217, (byte)0, (byte)221);
            _client.meditate = true;
            _client.mpurple = true;
          }
          if (_client.Currentnpctext.StartsWith("You must defeat a Gruesomefly"))
          {
            uint currentnpcpopupId = _client.CurrentnpcpopupID;
            _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)212, (byte)0, (byte)206);
            _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)212, (byte)0, (byte)234);
            _client.meditate = true;
            _client.myellow = true;
          }
          if (_client.Currentnpctext.StartsWith("You must defeat a Chest"))
          {
            uint currentnpcpopupId = _client.CurrentnpcpopupID;
            _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)207, (byte)0, (byte)207);
            _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)207, (byte)0, (byte)235);
            _client.meditate = true;
            _client.mblue = true;
          }
          if (_client.Currentnpctext.StartsWith("You must defeat a Crab"))
          {
            uint currentnpcpopupId = _client.CurrentnpcpopupID;
            _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)202, (byte)0, (byte)204);
            _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)202, (byte)0, (byte)228);
            _client.meditate = true;
            _client.mgreen = true;
          }
          if (_client.Currentnpctext.StartsWith("You must defeat a Bat"))
          {
            uint currentnpcpopupId = _client.CurrentnpcpopupID;
            _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)197, (byte)0, (byte)215);
            _client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)197, (byte)1, (byte)7);
            _client.meditate = true;
            _client.mwhite = true;
          }
          if (_client.Currentnpctext.StartsWith("*has a sparkle in his eye*"))
            _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)199, (byte)0, (byte)83);
          if (_client.Currentnpctext.StartsWith("*kindly smiles*"))
            _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)204, (byte)0, (byte)74);
          if (_client.Currentnpctext.StartsWith("*serenely sits*"))
            _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)209, (byte)0, (byte)74);
          if (_client.Currentnpctext.StartsWith("*appears deep in contemplation*"))
            _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)214, (byte)0, (byte)74);
          if (_client.Currentnpctext.StartsWith("*respectfully sits*"))
            _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)219, (byte)0, (byte)74);
          if (_client.Currentnpctext.StartsWith("*closes his palm quickly*"))
            _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)224, (byte)0, (byte)74);
          if (_client.Currentnpctext.StartsWith("*stares intently*"))
            _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)229, (byte)0, (byte)74);
          if (_client.Currentnpctext.StartsWith("*is gravely still*"))
            _client.PopupRespond(new uint?(_client.CurrentnpcpopupID), (byte)2, (byte)234, (byte)0, (byte)74);
          if (_client.Currentnpctext.StartsWith("Will you be respectful of the m"))
            _client.PopupOption1();
          if (_client.MapInfo.Number == 5219)
          {
            if (_client.Currentnpctext.StartsWith("Meditate on the "))
            {
              _client.attemptingdugon = _client.Currentnpctext.Split(' ')[3];
              _client.PopupOption1();
            }

            // Provide right response for Dugon meditation
            var color = _client.GetDugonColor();
            dugonMeditations = Meditation.DugonMediation(_client);
            foreach (var entry in dugonMeditations)
            {
              if (_client.Currentnpctext.StartsWith(entry.Key) &&
                  color.HasValue &&
                  entry.Value.TryGetValue(color.Value, out var action))
              {
                action();
                _client.meditate = false;
                break;
              }
            }

          }
          if (_client.Tab.togglehaxloop.Text == "stop" && (_client.haxtimer == DateTime.MinValue || DateTime.UtcNow.Subtract(_client.haxtimer).TotalSeconds > (double)_client.Tab.haxtimenum.Value))
          {
            if (_client.MapInfo.Number == 6138)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Table");
              if (npcByName != null && npcByName.IsOnScreen)
              {
                _client.DialogueRespond(npcByName.ID, (byte)9, (byte)202);
                Thread.Sleep(100);
                _client.PopupNext(new uint?(npcByName.ID));
                _client.haxtimer = DateTime.UtcNow;
              }
            }
            if (_client.MapInfo.Number == 2051)
            {
              Npc npcByName = _client.FindNpcByName<Npc>("Dreval");
              if (npcByName != null && npcByName.IsOnScreen)
              {
                _client.DialogueRespond(npcByName.ID, (byte)9, (byte)216);
                Thread.Sleep(100);
                _client.PopupClose(new uint?(npcByName.ID));
                _client.Tab.togglehaxloop.Text = "start";
              }
            }
          }
          if (_client.buyballpots)
          {
            Npc npcByName = _client.FindNpcByName<Npc>("Bartender1");
            if (npcByName != null && npcByName.IsOnScreen)
            {
              if (_client.Statistics.Gold < 50000U)
              {
                _client.SendMessage("Monies all spent.");
                _client.buyballpots = false;
              }
              else
              {
                uint id = npcByName.ID;
                _client.ClickNpc(id);
                _client.PopupRespond(new uint?(id), (byte)0, (byte)0, (byte)0, (byte)2);
                _client.PopupRespond(new uint?(id), (byte)0, (byte)0, (byte)0, (byte)2);
                _client.PopupRespond(new uint?(id), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
                _client.PopupRespond(new uint?(id), (byte)0, (byte)0, (byte)0, (byte)2);
                Thread.Sleep(1000);
              }
            }
          }
          if (_client.Currentnpcname == "Taster")
          {
            if (_client.Currentnpctext.StartsWith("Hello there "))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupNext(new uint?(currentnpcpopupId));
              _client.PopupNext(new uint?(currentnpcpopupId));
              Thread.Sleep(1000);
              _client.SkillSpellCaption("good");
            }
            if (_client.Currentnpctext.StartsWith("I see, well I"))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupNext(new uint?(currentnpcpopupId));
              _client.PopupNext(new uint?(currentnpcpopupId));
              _client.PopupNext(new uint?(currentnpcpopupId));
              Thread.Sleep(1000);
              if (_client.HasItem("Glass of Wine") && _client.WithinRange(5, 27, 3))
                _client.DropInMonster(currentnpcpopupId, _client.ItemSlot("Glass of Wine"), 1);
              else
                _client.SendMessage("Get a glass of wine and drop it on Taster");
            }
            if (_client.Currentnpctext.StartsWith("Why thank you very much"))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupNext(new uint?(currentnpcpopupId));
              _client.PopupNext(new uint?(currentnpcpopupId));
              Thread.Sleep(1000);
              if (_client.HasItem("Lobster Meal") && _client.WithinRange(5, 27, 3))
                _client.DropInMonster(currentnpcpopupId, _client.ItemSlot("Lobster Meal"), 1);
              else if (_client.HasItem("Lobster") && _client.WithinRange(5, 27, 3))
                _client.DropInMonster(currentnpcpopupId, _client.ItemSlot("Lobster"), 1);
              else
                _client.SendMessage("Get a Lobster Meal and drop it on Taster");
            }
            if (_client.Currentnpctext.StartsWith("Wonderful,"))
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID));
          }
          if (_client.Currentnpcname == "Snobi")
          {
            if (_client.Currentnpctext.StartsWith("Greetings, and how are"))
            {
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID));
              Thread.Sleep(1000);
              _client.Speak("good");
            }
            if (_client.Currentnpctext.StartsWith("That is good to hear"))
            {
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID));
              Thread.Sleep(1000);
              _client.Speak("how are you");
            }
            if (_client.Currentnpctext.StartsWith("I am doing just fine"))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupNext(new uint?(currentnpcpopupId));
              _client.PopupNext(new uint?(currentnpcpopupId));
              Thread.Sleep(1000);
              _client.Speak("what requirements");
            }
            if (_client.Currentnpctext.StartsWith("First you need"))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupNext(new uint?(currentnpcpopupId));
              _client.PopupNext(new uint?(currentnpcpopupId));
              Thread.Sleep(1000);
              _client.Speak("no");
            }
            if (_client.Currentnpctext.StartsWith("What? Now that is just "))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupNext(new uint?(currentnpcpopupId));
              _client.PopupNext(new uint?(currentnpcpopupId));
              Thread.Sleep(1000);
              _client.Speak("heart");
            }
            if (_client.Currentnpctext.StartsWith("Hmmmm, that's right, "))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupNext(new uint?(currentnpcpopupId));
              _client.PopupNext(new uint?(currentnpcpopupId));
              Thread.Sleep(1000);
              _client.Speak("i forgive you");
            }
            if (_client.Currentnpctext.StartsWith("Thank you for understanding"))
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID));
          }
          if (_client.Currentnpcname == "Boogle")
          {
            if (_client.Currentnpctext.StartsWith("Hello there, "))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
              _client.PopupNext(new uint?(currentnpcpopupId));
              _client.PopupNext(new uint?(currentnpcpopupId));
              Thread.Sleep(1000);
              _client.Speak("rat");
            }
            if (_client.Currentnpctext.StartsWith("Good answer, I "))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupNext(new uint?(currentnpcpopupId));
              _client.PopupRespond(new uint?(currentnpcpopupId), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
              _client.PopupNext(new uint?(currentnpcpopupId));
              _client.PopupNext(new uint?(currentnpcpopupId));
              Thread.Sleep(1000);
              _client.Speak("nothing");
            }
            if (_client.Currentnpctext.StartsWith("Correct! Those"))
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID));
          }
          if (_client.Currentnpcname == "Modly")
          {
            if (_client.Currentnpctext.StartsWith("*looking ne"))
            {
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID));
              Thread.Sleep(1000);
              _client.Speak("good");
            }
            if (_client.Currentnpctext.StartsWith("Hey, how's it going?"))
            {
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID));
              Thread.Sleep(1000);
              _client.Speak("good");
            }
            if (_client.Currentnpctext.StartsWith("Sorry for sounding"))
            {
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID));
              Thread.Sleep(1000);
              _client.Speak("expert");
            }
            if (_client.Currentnpctext.StartsWith("Interesting,"))
            {
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID));
              Thread.Sleep(1000);
              _client.Speak("expert");
            }
            if (_client.Currentnpctext.StartsWith("Really?"))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupNext(new uint?(currentnpcpopupId));
              _client.PopupNext(new uint?(currentnpcpopupId));
              _client.PopupNext(new uint?(currentnpcpopupId));
              Thread.Sleep(1000);
              _client.Speak("sure");
            }
            if (_client.Currentnpctext.StartsWith("Great I really "))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupNext(new uint?(currentnpcpopupId));
              _client.PopupNext(new uint?(currentnpcpopupId));
              _client.ballemoboy = 1;
              //if (!_client.BotThread.IsAlive)
              //  _client.BotThread.Start();
              _client.pause = false;
              _client.Tab.btnPlay.Enabled = false;
              _client.Tab.btnStop.Enabled = true;
              _client.Tab.fastwalk.Checked = true;
            }
            if (_client.Currentnpctext.StartsWith("I can't go"))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupNext(new uint?(currentnpcpopupId));
              _client.PopupNext(new uint?(currentnpcpopupId));
              Thread.Sleep(1000);
              _client.Speak("handsome");
            }
            if (_client.Currentnpctext.StartsWith("Really! Someone"))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupNext(new uint?(currentnpcpopupId));
              _client.PopupNext(new uint?(currentnpcpopupId));
              Thread.Sleep(1000);
              _client.Speak("be yourself");
            }
            if (_client.Currentnpctext.StartsWith("Thank you"))
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID));
          }
          if (_client.ballemoboy == 1 && _client.Currentnpctext == "")
          {
            if (_client.WithinRange(21, 14, 2))
            {
              _client.ballemoboy = 2;
              Thread.Sleep(1000);
              _client.Speak("hello");
            }
            else
              _client.WalkWithinRange(21, 14, 2);
          }
          if (_client.ballemoboy == 3 && _client.Currentnpctext == "")
          {
            if (_client.WithinRange(22, 28, 2))
            {
              _client.ballemoboy = 4;
              Thread.Sleep(1000);
              _client.Speak("go talk to her");
            }
            else
              _client.WalkWithinRange(22, 28, 2);
          }
          if (_client.Currentnpcname == "Grace")
          {
            if (_client.Currentnpctext.StartsWith("Hello"))
            {
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID));
              Thread.Sleep(1000);
              _client.Speak("i have a friend");
            }
            if (_client.Currentnpctext.StartsWith("A friend?"))
            {
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID));
              Thread.Sleep(1000);
              _client.Speak("he is interested");
            }
            if (_client.Currentnpctext.StartsWith("Well, which one"))
            {
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID));
              Thread.Sleep(1000);
              _client.Speak("in the corner");
            }
            if (_client.Currentnpctext.StartsWith("Oh him, I noticed"))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupNext(new uint?(currentnpcpopupId));
              _client.PopupNext(new uint?(currentnpcpopupId));
              _client.ballemoboy = 3;
              //if (!_client.BotThread.IsAlive)
              //  _client.BotThread.Start();
              _client.pause = false;
              _client.Tab.btnPlay.Enabled = false;
              _client.Tab.btnStop.Enabled = true;
              _client.Tab.fastwalk.Checked = true;
            }
          }
          if (_client.Currentnpcname == "Josephine")
          {
            if (_client.Currentnpctext.StartsWith("Hello"))
            {
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID));
              Thread.Sleep(1000);
              _client.Speak("want to dance?");
            }
            if (_client.Currentnpctext.StartsWith("Thank you for asking"))
            {
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID));
              Thread.Sleep(1000);
              _client.Speak("i can teach you");
            }
            if (_client.Currentnpctext.StartsWith("Really, that would be"))
            {
              uint currentnpcpopupId = _client.CurrentnpcpopupID;
              _client.PopupNext(new uint?(currentnpcpopupId));
              Thread.Sleep(800);
              _client.Speak("right");
              Thread.Sleep(800);
              _client.PopupNext(new uint?(currentnpcpopupId));
              Thread.Sleep(800);
              _client.Speak("forward");
              Thread.Sleep(800);
              _client.PopupNext(new uint?(currentnpcpopupId));
              Thread.Sleep(800);
              _client.Speak("left");
              Thread.Sleep(800);
              _client.PopupNext(new uint?(currentnpcpopupId));
              Thread.Sleep(800);
              _client.Speak("forward");
              Thread.Sleep(800);
              _client.PopupNext(new uint?(currentnpcpopupId));
              Thread.Sleep(800);
              _client.Speak("left");
              Thread.Sleep(800);
              _client.PopupNext(new uint?(currentnpcpopupId));
              Thread.Sleep(800);
              _client.Speak("forward");
              Thread.Sleep(800);
              _client.PopupNext(new uint?(currentnpcpopupId));
              Thread.Sleep(800);
              _client.Speak("left");
              Thread.Sleep(800);
              _client.PopupNext(new uint?(currentnpcpopupId));
            }
            if (_client.Currentnpctext.StartsWith("Wonderful I think"))
              _client.PopupNext(new uint?(_client.CurrentnpcpopupID));
          }
        }
        catch
        {
        }
      label_3045:
        Thread.Sleep(200);
      }
    }



  }
}
