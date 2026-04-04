using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Spells
{
  public static class SpellTargeting
  {
    public static void Handle(SpellContext context)
    {
      if (!context.HasTarget)
        return;

      var client = context.Client;

      client.newtargetdelay = DateTime.UtcNow;
      if (context.TargetId != null)
        client.LastTarget = (uint)context.TargetId;

      if (client.Characters.ContainsKey(client.LastTarget) &&
          client.Characters[client.LastTarget] != null &&
          client.Characters[client.LastTarget] is Npc &&
          client.Characters[client.LastTarget].IsOnScreen)
      {
        client.LastMonsterId = client.LastTarget;
      }
    }
  }
}
