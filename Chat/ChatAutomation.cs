using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Chat
{
  public static class ChatAutomation
  {
    public static void Handle(ChatContext ctx)
    {
      var client = ctx.Client;

      if (ctx.Type != 0 && ctx.Type != 1)
        return;

      if (!client.Characters.ContainsKey(ctx.SenderId))
        return;

      var character = client.Characters[ctx.SenderId];

      if (!character.IsOnScreen)
        return;

      if (ctx.Message.Equals("aite", StringComparison.OrdinalIgnoreCase))
      {
        Console.WriteLine($"Spelling: {ctx.SenderId}");
        if (client.HasSpell("ard naomh aite"))
          client.CastSpell("ard naomh aite", ctx.SenderId);
      }
    }
  }
}
