//SlowPoke
// Type: Flintstones.Map
//SlowPoke
//SlowPoke
//SlowPoke
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flintstones
{
  public class Room
  {
    public int ID;
    public string Name;
    public List<Point> Locations;

    public Room(int iD, string name, List<Point> locations)
    {
      ID = iD;
      Name = name;
      Locations = locations;
    }
  }
}