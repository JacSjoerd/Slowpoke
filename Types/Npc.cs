//SlowPoke
// Type: Flintstones.Npc
//SlowPoke
//SlowPoke
//SlowPoke

namespace Flintstones
{
  public class Npc : Character
  {
    public int Image { get; set; }

    public Npc.NpcType Type { get; set; }

    public byte Color { get; set; }

    public enum NpcType
    {
      NormalMonster,
      PassableMonster,
      Mundane,
      Item,
    }
  }
}
