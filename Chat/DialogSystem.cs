using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flintstones;

namespace Slowpoke.Chat
{
  public static class DialogSystem
  {

    public static bool HandleDialogSelect(Client client, ClientPacket msg)
    {
      DialogReset.ResetDialogInteraction(client);

      var context = DialogSelectContext.FromPacket(client, msg);

      SetClientMode(context);

      Logger.Debug($"Client {client.Name} selected dialog option {context.DialogueOptionIndex} in dialog {context.DialogueCategory} with character {context.NpcId} and action {context.ActionCode}");
      return true;
    }

    public static bool HandlePopupSelect(Client client, ClientPacket msg)
    {
      DialogReset.ResetPopupInteraction(client);
      
      var context = PopupSelectContext.FromPacket(client, msg);
      

      Logger.Debug($"Client {client.Name} selected popup option {context.PopupOptionIndex} in popup {context.PopupId} with action {context.ActionCode}");
      return true;
    }


    private static void SetClientMode(DialogSelectContext context)
    {
      Client client = context.Client;

      switch (context.ActionCode)
      {
        case 99:
          if (client.sendmode == 2)
            client.sendmode = 1;
          Logger.Debug($"Client {client.Name} sendmode set to {client.sendmode}");
          break;

        case 86:
          if (client.withdrawmode == 2)
            client.withdrawmode = 1;
          Logger.Debug($"Client {client.Name} withdrawmode set to {client.withdrawmode}");
          break;

        case 83:
          if (client.depositmode == 2)
            client.depositmode = 1;
          Logger.Debug($"Client {client.Name} depositmode set to {client.depositmode}");
          break;
      }
    }
  }
}
