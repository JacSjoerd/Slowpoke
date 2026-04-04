using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Chat
{
  public static class ChatFormatting
  {
    public static bool Handle(ChatContext ctx)
    {
      var client = ctx.Client;

      if (!client.Tab.chattimestamp.Checked || ctx.Type != 0)
        return true;

      var now = DateTime.Now;

      var msg = new ServerPacket(13);
      msg.WriteByte(ctx.Type);
      msg.WriteUInt32(ctx.SenderId);
      msg.WriteString8(now.ToString("hh:mm") + ">" + ctx.RawMessage);
      msg.Write(new byte[3]);
      msg.Write(new byte[3]);

      client.Enqueue(msg);

      return false;
    }
  }
}
