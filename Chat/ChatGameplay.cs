using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Slowpoke.Chat
{
  public static class ChatGameplay
  {
    public static void Handle(ChatContext ctx)
    {
      var client = ctx.Client;

      // Lost pet logic
      if (ctx.Type == 0 && ctx.SenderId == client.PlayerID)
      {
        if (ctx.Message.Equals("Don't get lost again.") 
            || ctx.Equals("Let's go to mommy.") 
            || ctx.Equals("You are safe now!") 
            || ctx.Equals("Gotcha!") 
            || ctx.Equals("Don't be scared."))
          client.losterbiedelay = DateTime.UtcNow;

        if (ctx.Equals("I caught one!") 
            || ctx.Equals("Victory!") 
            || ctx.Equals("Got it!") 
            || ctx.Equals("Gotcha!") 
            || ctx.Equals("Got one!"))
          client.bugtimer = DateTime.UtcNow;

      }



      // NPC summon trigger
      if ((ctx.Message.Contains("Kill...") || ctx.Message.Contains("Ahhh...")) &&
          Server.StaticCharacters.ContainsKey(ctx.SenderId))
      {
        var npc = Server.StaticCharacters[ctx.SenderId];
        if (!npc.HasSummoned)
          npc.HasSummoned = true;
      }
    }
  }
}
