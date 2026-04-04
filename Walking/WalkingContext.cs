using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flintstones;

namespace Slowpoke.Walking
{
  public class WalkingContext
  {
    public Client Client;
    public Direction Direction;

    public static WalkingContext FromPacket(Client client, ClientPacket msg)
    {
      int direction = msg.ReadByte();

      Direction dir = (Direction)direction;


      // parse packet to create context
      return new WalkingContext
      {
        Client = client,
        Direction = dir,
      };
    }

    public static void ToPacket(Client client, ClientPacket msg) {
      msg.BodyData[1] = client.WalkCounter++;
    }
  }
}
