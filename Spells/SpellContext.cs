using Flintstones;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Spells
{
  public class SpellContext
  {
    public Client Client;
    public Spell Spell;
    public int Slot;
    public uint? TargetId;
    public bool HasTarget;
    public int BodyDataLength;
    public static SpellContext FromPacket(Client client, ClientPacket msg)
    {
      int slot = msg.ReadByte();
      Spell spell = client.SpellBook[slot - 1];
      int bodyDataLength = msg.BodyData.Length;

      uint? target = null;

      if (bodyDataLength > 6)
      {
        target = msg.ReadUInt32();
      }

      Logger.Debug($"SpellContext.FromPacket: Client={client.Name}, Spell={spell.Name}, Slot={slot}, TargetId={(target.HasValue ? target.Value.ToString() : "None")}");

      return new SpellContext
      {
        Client = client,
        Spell = spell,
        Slot = slot,
        TargetId = target,
        HasTarget = target.HasValue,
        BodyDataLength = bodyDataLength
      };
    }

  }
}
