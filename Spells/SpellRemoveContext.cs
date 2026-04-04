using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Spells
{
  public class SpellRemoveContext
  {
    public Client Client;
    public int Slot;
    public static SpellRemoveContext FromPacket(Client client, ServerPacket msg)
    {
      var slot = (int)msg.ReadByte();

      return new SpellRemoveContext
      {
        Client = client,
        Slot = slot
      };
    }
  }
}
