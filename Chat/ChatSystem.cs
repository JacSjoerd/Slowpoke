using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Chat
{
  public static class ChatSystem
  {
    public static bool HandleChat(Client client, ServerPacket msg)
    {
      var context = ChatContext.FromPacket(client, msg);

      if (!ChatFilter.ShouldAllow(context))
        return false;

      ChatUI.Handle(context);
      ChatGameplay.Handle(context);
      ChatAutomation.Handle(context);

      Logger.Debug($"Chat: [{context.Client.Name}] {context.RawMessage}");

      return ChatFormatting.Handle(context);
    }
  }
}
