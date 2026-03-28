using Flintstones;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace slowpoke.Types
{
  internal class Walking
  {
    
    private Client _client;

    private string walkMapFilePath = Path.Combine(Program.StartupPath, "Settings", "WalkPaths.xml");
    public static Dictionary<int, MappedMaps> AutoWalkMaps;

    public Walking(Client client)
    {
      _client = client;

      if (AutoWalkMaps == null) AutoWalkMaps = LoadAutoWalkMaps();
    }

    public void Run(CancellationToken token)
    {
      Console.WriteLine($"Walking thread of {_client.Name} Started");

      while (!token.IsCancellationRequested)
      {
        if (_client.refreshdelay != DateTime.MinValue && DateTime.UtcNow.Subtract(_client.refreshdelay).TotalMilliseconds < 1200.0)
        {
          Thread.Sleep(200);
          continue;
        }

        if (_client.refreshdelay != DateTime.MinValue && DateTime.UtcNow.Subtract(_client.refreshdelay).TotalMilliseconds >= 1200.0)
          _client.refreshdelay = DateTime.MinValue;

        if (_client.pause || _client.pausewalk || _client.donotwalk)
        {
          Thread.Sleep(200);
          continue;
        }

        if (_client.IsSkulled 
            && _client.IsSuained 
            && _client.IsStunned 
            && _client.SpellBar.Contains((ushort)90) 
            && _client.SpellBar.Contains((ushort)97) 
            && _client.SpellBar.Contains((ushort)101))
        {
          Thread.Sleep(200);
          continue;
        }

        if (_client.Tab.wayregionson.Checked &&
            _client.laststep != DateTime.MinValue &&
            (DateTime.UtcNow - _client.laststep).TotalMilliseconds > 6000 &&
            _client.lastsuccessfulcast != DateTime.MinValue &&
            (DateTime.UtcNow - _client.lastsuccessfulcast).TotalMilliseconds > 10000)
        {
          _client.Refresh();
          Thread.Sleep(1200);
          _client.laststep = DateTime.UtcNow;
        }
        if (_client.Tab.vredaislings && _client.Tab.walktored.Checked && !_client.IsSurrounded(_client.ServerLocation))
        {
          foreach (Player e in (IEnumerable<Player>)((IEnumerable<Player>)_client.NearbyPlayer()).OrderBy<Player, int>((Func<Player, int>)(e => e.Location.DistanceFrom(_client.ServerLocation))))
          {
            if (e != null && (int)e.ID != (int)_client.PlayerID && (Server.StaticCharacters[e.ID].isskulled || Server.StaticCharacters[e.ID].IsSkulled) && _client.IsClosestToYou(e.Location) && !_client.IsSurrounded(e.Location) && (_client.GroupMembers.Contains(e.Name) || Server.Alts.ContainsKey(e.Name.ToLower()) || Server.friendlist != null && Server.friendlist.Contains(e.Name.ToLower())) && e.IsOnScreen && (!Server.Alts.ContainsKey(e.Name.ToLower()) || Server.Alts[e.Name.ToLower()].IsSkulled) && _client.HasItem("Komadium"))
            {
              _client.Red(e);
              break;
            }
          }
        }
        if (_client.oktofollow)
        {
          if (!_client.disstopwalk)
          {
            if (_client.autowalkon)
            {
              _client.walkaround = false;
              if (DateTime.UtcNow.Subtract(_client.laststep).TotalSeconds > 2.0 && (_client.ServerLocation.X != _client.ClientLocation.X || _client.ServerLocation.Y != _client.ClientLocation.Y))
              {
                _client.Refresh();
                Thread.Sleep(1200);
                _client.laststep = DateTime.MinValue;
              }
              _client.AutoWalker();
              _client.AWTest();
            }
            else if (_client.Tab.walkeverytile.Checked)
            {
              if (!_client.walkaround)
                _client.walkaround = true;
              if (_client.Tab.vactonlyinmobs)
              {
                if (_client.Mobbed)
                  continue;
              }
              if (_client.Tab.vwalktoloot && _client.loot && _client.walktoloot)
              {
                Npc i = _client.NearestItem();
                if (i != null && i.IsOnScreen && !_client.ServerLocation.WithinSquare(i.Location, 2))
                {
                  Point[] path = _client.MapInfo.FindPath(_client.ClientLocation.X, _client.ClientLocation.Y, i.Location.X, i.Location.Y, false);
                  if (path.Length == 0)
                    i.OutofReach = true;
                  if (path.Length != 0 && path.Length < i.DistanceFrom(_client.ServerLocation) * 2)
                  {
                    _client.WalkToLoot(i);
                    continue;
                  }
                  if (_client.walktoloot)
                    _client.walktoloot = false;
                }
                else if (i == null && _client.walktoloot)
                  _client.walktoloot = false;
              }
              if (!_client.Tab.topx.Text.Equals("") && !_client.Tab.topy.Text.Equals("") && !_client.Tab.bottomx.Text.Equals("") && !_client.Tab.bottomy.Text.Equals(""))
                _client.SearchAllTiles(int.Parse(_client.Tab.topx.Text), int.Parse(_client.Tab.topy.Text), int.Parse(_client.Tab.bottomx.Text), int.Parse(_client.Tab.bottomy.Text));
            }
            else if (_client.Tab.vwayregionson)
            {
              if (DateTime.UtcNow.Subtract(_client.laststep).TotalSeconds > 2.0 && (_client.ServerLocation.X != _client.ClientLocation.X || _client.ServerLocation.Y != _client.ClientLocation.Y))
              {
                _client.Refresh();
                Thread.Sleep(1200);
                _client.laststep = DateTime.MinValue;
              }
              if (!_client.Tab.haltwalknonfriends.Checked || _client.SafeToWalkFast || _client.MapInfo.Number == 2141)
                _client.WayRegion();
            }
            else if (_client.Tab.vfollowplayer && _client.Tab.vfollowtarget != string.Empty && _client.follow_walk != 1)
            {
              if (DateTime.UtcNow.Subtract(_client.laststep).TotalSeconds > 2.0 && (_client.ServerLocation.X != _client.ClientLocation.X || _client.ServerLocation.Y != _client.ClientLocation.Y))
              {
                _client.Refresh();
                Thread.Sleep(1200);
                _client.laststep = DateTime.MinValue;
              }
              else
                _client.Follow(_client.Tab.vfollowtarget, _client.Tab.vfollowdist);
            }
            if (_client.Tab.pigwalk.Checked)
            {
              if (_client.MainTarget != null && _client.MainTarget.IsOnScreen && _client.MapInfo.Number == 2141)
              {
                if (DateTime.UtcNow.Subtract(_client.laststep).TotalSeconds > 2.0 && (_client.ServerLocation.X != _client.ClientLocation.X || _client.ServerLocation.Y != _client.ClientLocation.Y))
                {
                  _client.Refresh();
                  Thread.Sleep(1200);
                  _client.laststep = DateTime.MinValue;
                }
                _client.WalkOnTarget();
              }
              else if (_client.HasMPig())
              {
                if (_client.HasFPig())
                {
                  _client.Tab.autowalker_locales.SelectedItem = (object)"Loures";
                  _client.Tab.walklocaleslist.SelectedItem = (object)"Throne Room";
                  _client.Tab.autowalker_button.Text = "Stop";
                  _client.autowalkon = true;
                  _client.Tab.pigwalk.Checked = false;
                }
              }
            }
            else if (_client.Tab.walktowards.Checked && !_client.walktoloot && (_client.WaitOnBlankNames() || !_client.Tab.vactonlyinmobs || _client.Tab.vactonlyinmobs && _client.Mobbed))
            {
              if (_client.itemdroppeddelay != DateTime.MinValue)
              {
                if (DateTime.UtcNow.Subtract(_client.itemdroppeddelay).TotalSeconds <= 3.0)
                  goto label_92;
              }
              if (_client.Tab.vwalktoloot && _client.loot && _client.walktoloot)
              {
                Npc i = _client.NearestItem();
                if (i != null && i.IsOnScreen && !_client.ServerLocation.WithinSquare(i.Location, 2))
                {
                  Point[] path = _client.MapInfo.FindPath(_client.ClientLocation.X, _client.ClientLocation.Y, i.Location.X, i.Location.Y, false);
                  if (path.Length == 0)
                    i.OutofReach = true;
                  if (path.Length != 0 && path.Length < i.DistanceFrom(_client.ServerLocation) * 2)
                  {
                    _client.WalkToLoot(i);
                    continue;
                  }
                  if (_client.walktoloot)
                    _client.walktoloot = false;
                }
                else if (i == null && _client.walktoloot)
                  _client.walktoloot = false;
              }
              _client.WalkTowardsNearestMonster();
            }
            else if (_client.Tab.walktomonster.Checked)
            {
              if (!_client.SpellBar.Contains((ushort)10))
              {
                if (_client.Tab.vactonlyinmobs)
                {
                  if (_client.Tab.vactonlyinmobs)
                  {
                    if (!_client.Mobbed)
                      goto label_92;
                  }
                  else
                    goto label_92;
                }
                if (_client.MainTarget != null)
                {
                  if (_client.MainTarget.IsOnScreen)
                  {
                    if (_client.MainTarget.Map == _client.MapInfo.Number)
                    {
                      if (DateTime.UtcNow.Subtract(_client.laststep).TotalSeconds > 2.0 && (_client.ServerLocation.X != _client.ClientLocation.X || _client.ServerLocation.Y != _client.ClientLocation.Y))
                      {
                        _client.Refresh();
                        Thread.Sleep(1200);
                        _client.laststep = DateTime.MinValue;
                      }
                      if (_client.SurroundedCount == 0)
                        _client.WalkToTarget();
                      else if (_client.SurroundedCount != 4)
                      {
                        if (_client.Tab.attackleaderstarget.Checked)
                          _client.WalkToTarget();
                      }
                    }
                  }
                }
              }
            }
          }
        }
      label_92:
        Thread.Sleep(200);
      }

      Console.WriteLine($"Walk thread of {_client.Name} stopped.");
    }

    public void Red(Player e)
    {
      _client.oktofollow = false;
      try
      {
        bool flag = Server.StaticCharacters[e.ID].IsSkulled;
        int num1;
        do
        {
          _client.MapInfo.UpdateBlocks(_client);
          DateTime utcNow;
          if (Server.StaticCharacters[e.ID] != null && (int)e.ID != (int)_client.PlayerID && (Server.StaticCharacters[e.ID].isskulled || Server.StaticCharacters[e.ID].IsSkulled) && _client.ClientLocation.DistanceFrom(e.Location) > 1 && _client.IsClosestToYou(e.Location) && !_client.IsSurrounded(e.Location))
          {
            Point[] path = _client.MapInfo.FindPath(_client.ClientLocation.X, _client.ClientLocation.Y, e.Location.X, e.Location.Y, false);
            for (int index = 0; index < path.Length; ++index)
            {
              int num2;
              if (_client.refreshdelay != DateTime.MinValue)
              {
                utcNow = DateTime.UtcNow;
                num2 = utcNow.Subtract(_client.refreshdelay).TotalMilliseconds < 1200.0 ? 1 : 0;
              }
              else
                num2 = 0;
              if (num2 == 0)
              {
                if (Server.StaticCharacters[e.ID].IsSkulled)
                  flag = true;
                if (_client.Tab.walktored.Checked && !_client.pause && !_client.pausewalk && e.IsOnScreen && !_client.IsSkulled && !_client.IsStunned && !_client.IsSuained && !_client.SpellBar.Contains((ushort)90) && !_client.SpellBar.Contains((ushort)97) && !_client.SpellBar.Contains((ushort)101) && _client.Tab.vredaislings && path[index].Passable && _client.IsClosestToYou(e.Location) && !_client.IsSurrounded(e.Location) && Math.Abs(_client.ClientLocation.X - path[index].X) + Math.Abs(_client.ClientLocation.Y - path[index].Y) == 1)
                {
                  _client.Walk(_client.ClientLocation - new Location(path[index].X, path[index].Y));
                  _client.MapInfo.UpdateBlocks(_client);
                  Thread.Sleep(_client.WalkSpeed());
                  if ((!flag || Server.StaticCharacters[e.ID].IsSkulled) && Server.StaticCharacters[e.ID].isskulled)
                  {
                    if (_client.ClientLocation.DistanceFrom(e.Location) == 1)
                    {
                      _client.Refresh();
                      Thread.Sleep(1200);
                    }
                  }
                  else
                    break;
                }
                else
                  break;
              }
              else
                break;
            }
          }
          if (flag && !Server.StaticCharacters[e.ID].IsSkulled && _client.redwaittime == DateTime.MinValue)
            _client.redwaittime = DateTime.UtcNow;
          if (!Server.StaticCharacters[e.ID].isskulled && _client.redwaittime == DateTime.MinValue)
            _client.redwaittime = DateTime.UtcNow;
          Thread.Sleep(200);
          if (!(_client.redwaittime == DateTime.MinValue))
          {
            utcNow = DateTime.UtcNow;
            num1 = utcNow.Subtract(_client.redwaittime).TotalSeconds <= 3.0 ? 1 : 0;
          }
          else
            num1 = 1;
        }
        while (num1 != 0);
        _client.redwaittime = DateTime.MinValue;
        _client.oktofollow = true;
      }
      catch
      {
        _client.oktofollow = true;
      }
      finally
      {
        _client.oktofollow = true;
      }
    }

    private Dictionary<int, MappedMaps> LoadAutoWalkMaps()
    {
      var result = new Dictionary<int, MappedMaps>();
      var doc = XDocument.Load(walkMapFilePath);

      foreach (var mapElement in doc.Root.Elements("From"))
      {
        int fromId = (int)mapElement.Attribute("id");

        var toRoom = new MappedMaps();

        foreach (var toElement in mapElement.Elements("To"))
        {
          int toId = (int)toElement.Attribute("id");
          int x = (int)toElement.Attribute("x");
          int y = (int)toElement.Attribute("y");

          toRoom.ConnectedTo[toId] = new Location(x, y);
        }

        result[fromId] = toRoom;
      }

      Console.WriteLine($"Loaded {result.Count} autowalk maps.");
      return result;
    }
  }
}
