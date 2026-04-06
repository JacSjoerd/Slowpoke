//SlowPoke
// Type: Flintstones.Map
//SlowPoke
//SlowPoke
//SlowPoke

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Flintstones
{
  public class Map
  {
    public byte[,] BaseMatrix = new byte[0, 0];

    public static byte[] Sotp { get; private set; }

    public string Name { get; set; }

    public int Number { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public byte Bitmask { get; set; }

    public ushort Checksum { get; set; }

    public Point[,] Tiles { get; private set; }

    public bool IsLoaded { get; private set; }

    public ManualBlocks manualBlocks = new ManualBlocks();

    public bool Initialize()
    {
      try
      {
        this.Tiles = new Point[this.Width, this.Height];
        string path = Path.Combine(Options.DarkAgesMapsDirectoryName, $"lod{this.Number.ToString()}.map");
        this.IsLoaded = false;
        if (File.Exists(path))
        {
          FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
          for (int y = 0; y < this.Height; ++y)
          {
            for (int x = 0; x < this.Width; ++x)
            {
              this.Tiles[x, y] = new Point(x, y);
              this.Tiles[x, y].StepCount = -1;
              fileStream.ReadByte();
              fileStream.ReadByte();
              ushort num1 = (ushort) (fileStream.ReadByte() | fileStream.ReadByte() << 8);
              ushort num2 = (ushort) (fileStream.ReadByte() | fileStream.ReadByte() << 8);
              this.Tiles[x, y].IsWall = num1 != (ushort) 0 && Map.Sotp[(int) num1 - 1] != (byte) 0 || num2 != (ushort) 0 && Map.Sotp[(int) num2 - 1] > (byte) 0;
            }
          }
          fileStream.Close();
          this.LoadMatrix();
          this.IsLoaded = true;
        }
        return true;
      }
      catch
      {
        return true;
      }
    }

    public static bool LoadSotp(string iaDatPath)
    {
      if (Map.Sotp == null && File.Exists(iaDatPath))
      {
        using (FileStream input = File.Open(iaDatPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
          using (BinaryReader binaryReader = new BinaryReader((Stream) input))
          {
            int count = binaryReader.ReadInt32() - 1;
            for (int index = 0; index < count; ++index)
            {
              int num = binaryReader.ReadInt32();
              byte[] bytes = binaryReader.ReadBytes(13);
              binaryReader.ReadInt32();
              if (Encoding.ASCII.GetString(bytes).StartsWith("sotp.dat\0"))
              {
                input.Position = (long) num;
                Map.Sotp = binaryReader.ReadBytes(count);
                break;
              }
              input.Position -= 4L;
            }
          }
        }
      }
      return Map.Sotp != null;
    }

    private static bool CheckSOTP(ushort Left, ushort Right)
    {
      if (Left == (ushort) 0 && Right == (ushort) 0)
        return false;
      if (Left == (ushort) 0)
      {
        try
        {
          return Map.Sotp[(int) Right - 1] > (byte) 0;
        }
        catch
        {
          return true;
        }
      }
      else if (Right == (ushort) 0)
      {
        try
        {
          return Map.Sotp[(int) Left - 1] > (byte) 0;
        }
        catch
        {
          return true;
        }
      }
      else
      {
        try
        {
          return Map.Sotp[(int) Left - 1] != (byte) 0 || Map.Sotp[(int) Right - 1] > (byte) 0;
        }
        catch
        {
          return true;
        }
      }
    }

    public bool LoadMatrix()
    {
      try
      {
        FileStream input = File.OpenRead(Options.DarkAgesMapsDirectoryName + "\\lod" + this.Number.ToString() + ".map");
        BinaryReader binaryReader = new BinaryReader((Stream) input);
        this.BaseMatrix = new byte[this.Width + 1, this.Height + 1];
        for (int index1 = 0; index1 < this.Height; ++index1)
        {
          for (int index2 = 0; index2 < this.Width; ++index2)
          {
            int num = (int) binaryReader.ReadUInt16();
            this.BaseMatrix[index2, index1] = Map.CheckSOTP(binaryReader.ReadUInt16(), binaryReader.ReadUInt16()) ? (byte) 0 : (byte) 1;
          }
        }
        input.Close();
        return true;
      }
      catch (ObjectDisposedException ex)
      {
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public Point[] SurroundingPoints(Point pt)
    {
      List<Point> pointList = new List<Point>();
      if (pt.X > 0)
        pointList.Add(this.Tiles[pt.X - 1, pt.Y]);
      if (pt.Y > 0)
        pointList.Add(this.Tiles[pt.X, pt.Y - 1]);
      if (pt.X < this.Width - 1)
        pointList.Add(this.Tiles[pt.X + 1, pt.Y]);
      if (pt.Y < this.Height - 1)
        pointList.Add(this.Tiles[pt.X, pt.Y + 1]);
      return pointList.ToArray();
    }

    public void UpdateBlocks(Client client)
    {
      Point[,] tiles = this.Tiles;
      
      // Check if there are blocks defined in the ManualBlocks
      int lowerBoundX = tiles.GetLowerBound(0);
      int upperBoundX = tiles.GetUpperBound(0);
      int lowerBoundY = tiles.GetLowerBound(1);
      int upperBoundY = tiles.GetUpperBound(1);

      for (int x = lowerBoundX; x <= upperBoundX; x++)
      {
        for (int y = lowerBoundY ; y <= upperBoundY; y++)
        {
          Point point = tiles[x, y];
          point = tiles[x, y];
          if (point == null) continue;

          if (manualBlocks.LocationsByID.ContainsKey(Number))
          {
            if (manualBlocks.LocationsByID[Number].FirstOrDefault(p => p.X == x && p.Y == y) != null)
            {
              point.HasBlock = true;
              point.HasEntity = false;
            }
          }
        }
      }


      if (client.TempRegions.ContainsKey(this.Number))
      {
        foreach (KeyValuePair<Location, string> region in client.TempRegions[this.Number].Regions)
        {
          if (region.Key != null && region.Value == "Block" && client.MapInfo.Tiles[region.Key.X, region.Key.Y] != null)
            client.MapInfo.Tiles[region.Key.X, region.Key.Y].HasBlock = true;
        }
      }
      lock (client.Characters)
      {
        foreach (Character character in client.Characters.Values.ToArray<Character>())
        {
          if (character != null && character.IsOnScreen && (int) character.ID != (int) client.PlayerID)
          {
            int num;
            switch (character)
            {
              case Player _:
                if (((character as Player).Body != (byte) 0 || character.Name != "" && !character.Name.Equals("ishikawa", StringComparison.CurrentCultureIgnoreCase) && !character.Name.Equals("error", StringComparison.CurrentCultureIgnoreCase) && !character.Name.Equals("and", StringComparison.CurrentCultureIgnoreCase) && !character.Name.Equals("trial", StringComparison.CurrentCultureIgnoreCase)) && client.MapInfo.Tiles[character.Location.X, character.Location.Y] != null)
                {
                  client.MapInfo.Tiles[character.Location.X, character.Location.Y].HasBlock = true;
                  client.MapInfo.Tiles[character.Location.X, character.Location.Y].HasEntity = true;
                  continue;
                }
                continue;
              case Npc _:
                num = (character as Npc).Type == NpcType.PassableMonster || character.Map != client.MapInfo.Number ? 0 : ((character as Npc).Type != NpcType.Item ? 1 : 0);
                break;
              default:
                num = 1;
                break;
            }
            if (num != 0 && client.MapInfo.Tiles[character.Location.X, character.Location.Y] != null)
            {
              client.MapInfo.Tiles[character.Location.X, character.Location.Y].HasBlock = true;
              client.MapInfo.Tiles[character.Location.X, character.Location.Y].HasEntity = true;
            }
          }
        }
      }
    }

    public Point[] FindPath(int startX, int startY, int endX, int endY, bool ignoreentities)
    {
      List<Point> pointList1 = new List<Point>();
      List<Point> pointList2 = new List<Point>();
      List<Point> pointList3 = new List<Point>();
      List<Point> pointList4 = new List<Point>();
      bool[,] flagArray = new bool[this.Width, this.Height];
      bool flag = false;
      try
      {
        if ((startX + 1) * (startY + 1) > this.Tiles.Length)
          return new Point[0];
        if ((endX + 1) * (endY + 1) > this.Tiles.Length)
          return new Point[0];
        Point[,] tiles = this.Tiles;
        int upperBound1 = tiles.GetUpperBound(0);
        int upperBound2 = tiles.GetUpperBound(1);
        for (int lowerBound1 = tiles.GetLowerBound(0); lowerBound1 <= upperBound1; ++lowerBound1)
        {
          for (int lowerBound2 = tiles.GetLowerBound(1); lowerBound2 <= upperBound2; ++lowerBound2)
          {
            Point point = tiles[lowerBound1, lowerBound2];
            if (point != null)
              point.StepCount = -1;
          }
        }
        if (this.Tiles[startX, startY] != null)
        {
          this.Tiles[startX, startY].StepCount = 0;
          pointList2.Add(this.Tiles[startX, startY]);
        }
        while (!flag)
        {
          List<Point> pointList5 = new List<Point>();
          foreach (Point pt in pointList2)
          {
            if (pt != null)
            {
              foreach (Point surroundingPoint in this.SurroundingPoints(pt))
              {
                if (surroundingPoint != null)
                {
                  if (!flagArray[surroundingPoint.X, surroundingPoint.Y])
                  {
                    if (!ignoreentities)
                    {
                      if (surroundingPoint.Passable || surroundingPoint.X == endX && surroundingPoint.Y == endY)
                      {
                        pointList5.Add(surroundingPoint);
                        flagArray[surroundingPoint.X, surroundingPoint.Y] = true;
                        surroundingPoint.StepCount = pt.StepCount + 1;
                        if (surroundingPoint.X == endX && surroundingPoint.Y == endY)
                          flag = true;
                      }
                    }
                    else if (!surroundingPoint.IsWall || surroundingPoint.X == endX && surroundingPoint.Y == endY)
                    {
                      pointList5.Add(surroundingPoint);
                      flagArray[surroundingPoint.X, surroundingPoint.Y] = true;
                      surroundingPoint.StepCount = pt.StepCount + 1;
                      if (surroundingPoint.X == endX && surroundingPoint.Y == endY)
                        flag = true;
                    }
                  }
                  pointList1.Add(pt);
                  flagArray[pt.X, pt.Y] = true;
                }
              }
            }
          }
          pointList2 = pointList5;
          if (pointList2.Count < 1)
            return new Point[0];
        }
        Point pt1 = this.Tiles[endX, endY];
        pointList3.Add(this.Tiles[endX, endY]);
        while (pt1.StepCount > 1)
        {
          foreach (Point surroundingPoint in this.SurroundingPoints(pt1))
          {
            if (surroundingPoint != null)
            {
              flagArray[surroundingPoint.X, surroundingPoint.Y] = false;
              if (surroundingPoint.StepCount == pt1.StepCount - 1)
              {
                pointList3.Add(surroundingPoint);
                pt1 = surroundingPoint;
                break;
              }
            }
          }
        }
        pointList3.Reverse();
      }
      catch
      {
        return new Point[0];
      }
      return pointList3.ToArray();
    }

    public Point this[int x, int y] => (x + 1) * (y + 1) > this.Tiles.Length ? new Point(-1, -1) : this.Tiles[x, y];
  }
}
