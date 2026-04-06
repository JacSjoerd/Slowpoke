using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flintstones;

namespace Slowpoke.Chat
{
  internal class SpeakContext
  {
    public Client Client;
    public int Slot;
    public string Message;

    public static SpeakContext FromPacket(Client client, ClientPacket msg)
    {
      int slot = msg.ReadByte();
      string message = msg.ReadString(msg.ReadByte());

      return new SpeakContext
      {
        Client = client,
        Slot = slot,
        Message = message,
      };
    }
  }
}
