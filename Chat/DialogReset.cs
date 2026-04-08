using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flintstones;

namespace Slowpoke.Chat
{
  internal class DialogReset
  {
    internal static void ResetDialogInteraction(Client client)
    {
      client.CurrentnpcpopupID = 0;
      client.Currentnpctext = string.Empty;
      client.Currentnpcscript = 0;
    }

    internal static void ResetPopupInteraction(Client client)
    {
      client.anttunnels = 0;
      client.guardiananttunnels = 0;
      client.popup = false;
      client.cancast = true;
      client.canskill = true;
      client.donotwalk = false;
      client.pionome = false;
      client.niomope = false;
      client.mionope = false;
      client.deoch = false;
      client.gramail = false;
      client.brody = false;
      client.habab = false;
      client.nairn = false;
      client.banker = false;
    }
  }
}
