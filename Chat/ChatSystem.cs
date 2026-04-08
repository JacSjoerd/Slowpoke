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

      //Logger.Debug($"Chat: [{context.Client.Name}] {context.RawMessage}");

      return ChatFormatting.Handle(context);
    }

    public static bool HandleSpeak(Client client, ClientPacket msg)
    {
      var context = SpeakContext.FromPacket(client, msg);
      var message = context.Message;

      if (client.LastPermMessage != string.Empty)
        client.SendMessage(client.LastPermMessage);

      //Logger.Debug($"Speak: [{context.Client.Name}] {message}");

      if (!message.StartsWith("/") || !client.Tab.vslash_commands)
        return true;

      if (client.SpeakMessage == string.Empty)
        client.SpeakMessage = message;

      return false;
    }

  }
}
