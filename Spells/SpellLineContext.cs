using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Spells
{
  internal class SpellLineContext
  {
    internal Client Client;
    internal int Lines;

    internal static SpellLineContext FromPacket(Client client, ClientPacket msg)
    {
      int lines = msg.ReadByte();

      return new SpellLineContext
      {
        Client = client,
        Lines = lines,
      };      
    }
  }
}
