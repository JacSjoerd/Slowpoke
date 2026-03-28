//SlowPoke
// Type: Flintstones.Map
//SlowPoke
//SlowPoke
//SlowPoke

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;

namespace Flintstones
{
  public class ManualBlocks
  {
    public List<Room> Rooms = new List<Room>();
    public Dictionary<int, List<Point>> LocationsByID = new Dictionary<int, List<Point>>();

    string filePath = Path.Combine(Program.StartupPath, "Settings", "ManualBlocks.xml");

    public ManualBlocks()
    {
      Load();
      Console.WriteLine($"Loaded {Rooms.Count} rooms from ManualBlocks.xml");

      foreach (Room room in Rooms)
      {
        LocationsByID.Add(room.ID, room.Locations);
      }
    }

    private void Load()
    {
      var doc = XDocument.Load(filePath);

      foreach (var roomElement in doc.Root.Elements("Room"))
      {
        if (!int.TryParse(roomElement.Attribute("id")?.Value, out int id))
          continue;

        var name = (string)roomElement.Attribute("name") ?? "Unknown";

        List<Point> points = new List<Point>();
        foreach (var loc in roomElement.Element("Locations").Elements("Location"))
        {
          int x = (int)loc.Attribute("x");
          int y = (int)loc.Attribute("y");
          points.Add(new Point(x, y));
        }

        Room newRoom = new Room(id, name, points);

        Rooms.Add(newRoom);
      }
    }
  }
}