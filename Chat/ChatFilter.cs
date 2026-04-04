using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Chat
{
  public static class ChatFilter
  {
    public static bool ShouldAllow(ChatContext context)
    {
      if ((context.Type == 0 || context.Type == 1) && Server.ignoreaislinglist.Count > 0)
      {
        foreach (var aisling in Server.ignoreaislinglist)
        {
          if (context.RawMessage.ToLower().Contains(aisling))
            return false;
        }
      }

      return true;
    }
  }
}
