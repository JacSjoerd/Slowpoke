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
    public static void Handle(ChatContext context)
    {
      var client = context.Client;

      if (context.Type == 0 && context.SenderId == client.PlayerID)
      {
         // Lost pet logic
        if (context.Message.Equals("Don't get lost again.") 
            || context.Equals("Let's go to mommy.") 
            || context.Equals("You are safe now!") 
            || context.Equals("Gotcha!") 
            || context.Equals("Don't be scared."))
          client.losterbiedelay = DateTime.UtcNow;

        // Bug catching event logic
        if (context.Equals("I caught one!") 
            || context.Equals("Victory!") 
            || context.Equals("Got it!") 
            || context.Equals("Gotcha!") 
            || context.Equals("Got one!"))
          client.bugtimer = DateTime.UtcNow;

      }



      // NPC summon trigger
      if ((context.Message.Contains("Kill...") || context.Message.Contains("Ahhh...")) &&
          Server.StaticCharacters.ContainsKey(context.SenderId))
      {
        var npc = Server.StaticCharacters[context.SenderId];
        if (!npc.HasSummoned)
          npc.HasSummoned = true;
      }
    }
  }
}
