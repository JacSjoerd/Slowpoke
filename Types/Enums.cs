using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flintstones
{
  /// <summary>
  /// Enum to specify the available character classes for a player character.
  /// </summary>
  /// <remarks> Using a byte with Peasant = 0, Warrior = 1, Rogue = 2, Wizard = 3, Priest = 4, Monk = 5</remarks>
  public enum CharacterClass : byte
  {
    Peasant = 0,
    Warrior,
    Rogue,
    Wizard,
    Priest,
    Monk,
  }

  /// <summary>
  /// Specifies the cardinal directions and a special value indicating no direction.
  /// </summary>
  /// <remarks>Use this enumeration to represent orientation or movement in applications such as navigation,
  /// mapping, or games. The None value indicates the absence of a direction and can be used as a default or
  /// uninitialized state.</remarks>
  public enum Direction
  {
    North = 0,
    East = 1,
    South = 2,
    West = 3,
    None = 255, // 0x000000FF
  }

  /// <summary>
  /// Enum with all dugon colors in order of achieving.
  /// </summary>
  public enum DugonColor
  {
    White,
    Green,
    Blue,
    Yellow,
    Purple,
    Brown,
    Red,
    Black,
  }

  /// <summary>
  /// Specifies the Gender of a character or gender specific items. The Any value can be used to indicate that an item is suitable for all genders.
  /// </summary>
  public enum Gender
  {
    Male,
    Female,
    Any,
  }

  /// <summary>
  /// Specifies the types of items that can be represented in the inventory system.
  /// </summary>
  /// <remarks>Use this enumeration to categorize items such as weapons, armor, accessories, and other
  /// equipment. The values can be used to determine item behavior, restrictions, or display logic within the
  /// application.</remarks>
  public enum ItemType
  {
    Generic,
    Armor,
    Belt,
    Boots,
    Earring,
    Gauntlet,
    Greaves,
    Helmet,
    Necklace,
    Ring,
    Shield,
    Trinket,
    Weapon,
  }

  /// <summary>
  /// Specifies the types of NPCs (Non-Player Characters) that can be encountered in the game. 
  /// This enumeration can be used to categorize NPCs based on their behavior, interaction, or role within the game world.
  /// </summary>
  /// <remarks> Also an Item laying around in the world is indicated as an Npc by the DarkAges server</remarks>
  public enum NpcType
  {
    NormalMonster,
    PassableMonster,
    Mundane,
    Item,
  }

}
