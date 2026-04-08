using Flintstones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpoke.Chat
{
  internal class DialogSelectContext
  {
    internal Client Client;
    internal byte DialogueOptionIndex;
    internal uint NpcId;
    internal byte DialogueCategory;
    internal byte ActionCode;

    internal static DialogSelectContext FromPacket(Client client, ClientPacket msg)
    {
      msg.Read(6);

      byte dialogueOptionIndex = msg.ReadByte(); 
      uint npcId = msg.ReadUInt32();            
      byte dialogueCategory = msg.ReadByte(); 
      byte actionCode = msg.ReadByte(); 


      return new DialogSelectContext
      {
        Client = client,
        DialogueOptionIndex = dialogueOptionIndex,
        NpcId = npcId,
        DialogueCategory = dialogueCategory,
        ActionCode = actionCode
      };
    }
  }
}
