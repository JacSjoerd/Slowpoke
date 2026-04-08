using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Chat
{
  internal class PopupSelectContext
  {
    internal Client Client;
    internal int PopupOptionIndex;
    internal int PopupId;
    internal int ActionCode;
    internal int CloseureIndex;
    internal int ChestType;
    internal bool ChestOpened;
    internal string ChestOpenFee;

    internal static PopupSelectContext FromPacket(Client client, ClientPacket msg)
    {
      int chestType = 0;
      bool chestOpened = false;
      string chestOpenFee = string.Empty;

      msg.Read(6);
      int popupOptionIndex = (int)msg.ReadByte();
      int popupId = (int)msg.ReadUInt32();
      int actionCode = (int)msg.ReadByte();
      msg.ReadByte();
      msg.ReadByte();
      msg.ReadByte();
      int closureIndex = (int)msg.ReadByte();

      if (msg.Length >= 22)
      {
        chestType = msg.ReadByte();
        switch (chestType)
        {
          case 1:
            int openIndicator = msg.ReadByte();
            if (openIndicator == 1 || openIndicator == 3)
              chestOpened = true;
            break;
          case 2:
            chestOpenFee = msg.ReadString(msg.ReadByte());
            break;
        }
      }

      return new PopupSelectContext
      {
        Client = client,
        PopupOptionIndex = popupOptionIndex,
        PopupId = popupId,
        ActionCode = actionCode,
        CloseureIndex = closureIndex,
        ChestType = chestType,
        ChestOpened = chestOpened,
        ChestOpenFee = chestOpenFee,
      };
    }
  }
}
