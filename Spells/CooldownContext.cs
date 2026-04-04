using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Spells
{
  public class CooldownContext
  {
    public Client Client;
    public int Type; // 0 for spell, 1 for skill
    public int Slot;
    public int Duration; // in seconds

    /// <summary>
    /// Parsing cooldown information from a server packet.
    /// </summary>
    /// <param name="client">The client associated with the cooldown context. Cannot be null.</param>
    /// <param name="msg">The server packet containing the cooldown data to parse. Must be positioned at the start of the cooldown
    /// information.</param>
    /// <returns>A CooldownContext object populated with the cooldown type, slot, and duration.</returns>
    public static CooldownContext FromPacket(Client client, ServerPacket msg)
    {
      int type = (int)msg.ReadByte();
      int slot = (int)msg.ReadByte();
      int duration = (int)msg.ReadUInt32();

      return new CooldownContext
      {
        Client = client,
        Type = type,
        Slot = slot,
        Duration = duration
      };
    }
  }
}
