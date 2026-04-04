using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Chat
{
  public static class ChatUI
  {
    public static void Handle(ChatContext ctx)
    {
      var client = ctx.Client;

      if (client.Tab.ExternalChat.Visible)
      {
        client.Tab.ExternalChat.chatbox.AppendText(
            Environment.NewLine + ctx.RawMessage);

        client.Tab.ExternalChat.chatbox.ScrollToCaret();
      }
    }
  }
}
