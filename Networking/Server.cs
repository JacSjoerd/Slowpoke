//SlowPoke
// Type: Flintstones.Server
//SlowPoke
//SlowPoke
//SlowPoke

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Flintstones
{
  public class Server
  {
    public uint itemID;
    public bool now;
    public static List<AscendData> AscendLog = (List<AscendData>) null;
    public uint ClientId1;
    public ushort ClientId2;
    public int SpoofClientId;
    public static string cID = "";
    public static List<string> friendlist = (List<string>) null;
    public static List<string> gmlist = (List<string>) null;
    public static List<string> ignoreaislinglist = (List<string>) null;
    public static List<string> wsbanlist = (List<string>) null;
    public static List<int> CustomLoot = (List<int>) null;
    public static List<string> IdentifyItems = (List<string>) null;
    public static List<string> TrashList = (List<string>) null;
    public static List<string> NeedsIdentifiedList = (List<string>) null;
    public static SoundPlayer alarm;
    public static DateTime alarmTimer;
    public static bool SentryAlarm = false;
    public static bool alertfornonfriends = true;
    public static System.Timers.Timer AlertNonFriendTimer;
    public static byte invis = 80;
    public static Thread StrajNpc = (Thread) null;
    public static List<string> entitynametesting = new List<string>();
    internal Items AllItems = new Items();

    public bool Running { get; private set; }

    public Socket Socket { get; private set; }

    public TcpListener Listener { get; private set; }

    public ClientMessageHandler[] ClientMessageHandlers { get; private set; }

    public ServerMessageHandler[] ServerMessageHandlers { get; private set; }

    public EndPoint RemoteEndPoint { get; private set; }

    public static List<Client> Clients { get; private set; }

    public Thread ServerLoopThread { get; private set; }

    public static Dictionary<string, Parcel> ParcelList { get; set; }

    public static Dictionary<string, ChestItemXML> ChestDatabase { get; set; }

    public static Dictionary<string, ItemMapXML> ItemMapDatabase { get; set; }

    public static Dictionary<string, ItemXML> ItemDatabase { get; set; }

    public static Dictionary<string, TimedEvent> TimedEvent { get; set; }

    public static Dictionary<string, Relog> Relog { get; set; }

    public static Dictionary<string, Client> Alts { get; set; }

    public static Dictionary<string, string> Stuff { get; set; }

    public static Dictionary<string, int> DAServer { get; set; }

    public static Dictionary<string, bool> DARegged { get; set; }

    public static Dictionary<uint, Character> StaticCharacters { get; set; }

    public static Dictionary<string, SpellData> SpellList { get; set; }

    public static Dictionary<string, ItemData> ItemList { get; set; }

    public static Dictionary<string, HerbNode> HydeleNodes { get; set; }

    public static Dictionary<string, HerbNode> PersonacaNodes { get; set; }

    public static Dictionary<string, HerbNode> BetonyNodes { get; set; }

    public static Dictionary<string, HerbNode> HerbNodes { get; set; }

    public static Dictionary<string, SkullData> SkullList { get; set; }

    public static Dictionary<string, RootNpc> gamenpcs { get; set; }

    public static Dictionary<int, RootObject> gamemaps { get; set; }

    public static Dictionary<string, WalkLocation> WalkLocations { get; set; }

    public static Thread TimedEvents { get; set; }

    public Server()
    {
      Listener = new TcpListener(IPAddress.Loopback, 2610);
      Listener.Start(10);
      Clients = new List<Client>();
      gamemaps = new Dictionary<int, RootObject>();
      WalkLocations = LoadWalkLocations();
      gamenpcs = new Dictionary<string, RootNpc>();
      ParcelList = new Dictionary<string, Parcel>(StringComparer.CurrentCultureIgnoreCase);
      ChestDatabase = new Dictionary<string, ChestItemXML>(StringComparer.CurrentCultureIgnoreCase)
      {
        { "Arcella's Gift1", new ChestItemXML("Arcella's Gift1", 0U) },
        { "Water Dungeon Chest", new ChestItemXML("Water Dungeon Chest", 0U) },
        { "Water Dungeon Chest Gold", new ChestItemXML("Water Dungeon Chest Gold", 0U) },
        { "Andor Chest", new ChestItemXML("Andor Chest", 0U) },
        { "Andor Chest Gold", new ChestItemXML("Andor Chest Gold", 0U) },
        { "Queen's Chest", new ChestItemXML("Queen's Chest", 0U) },
        { "Queen's Chest Gold", new ChestItemXML("Queen's Chest Gold", 0U) },
        { "Canal Bag", new ChestItemXML("Canal Bag", 0U) },
        { "Big Canal Bag", new ChestItemXML("Big Canal Bag", 0U) },
        { "Heavy Canal Bag", new ChestItemXML("Heavy Canal Bag", 0U) }
      };

      PopulateChestDatabase();
      ItemMapDatabase = new Dictionary<string, ItemMapXML>(StringComparer.CurrentCultureIgnoreCase);
      ItemDatabase = new Dictionary<string, ItemXML>(StringComparer.CurrentCultureIgnoreCase);
      PopulateItemDatabase();
      BetonyNodes = new Dictionary<string, HerbNode>();
      PersonacaNodes = new Dictionary<string, HerbNode>();
      HydeleNodes = new Dictionary<string, HerbNode>();
      HerbNodes = new Dictionary<string, HerbNode>();
      PopulateNodes();
      AscendLog = new List<AscendData>();
      SkullList = new Dictionary<string, SkullData>(StringComparer.CurrentCultureIgnoreCase);
      TimedEvent = new Dictionary<string, Flintstones.TimedEvent>(StringComparer.CurrentCultureIgnoreCase);
      Relog = new Dictionary<string, Flintstones.Relog>(StringComparer.CurrentCultureIgnoreCase);
      Alts = new Dictionary<string, Client>(StringComparer.CurrentCultureIgnoreCase);
      Stuff = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
      DAServer = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
      DARegged = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);

      ItemList = AllItems.Unidentifiable;
      NeedsIdentifiedList = AllItems.Identifiable;
      SpellList = LoadSpells();

      Server.StaticCharacters = new Dictionary<uint, Character>();
      Server.alarmTimer = DateTime.MinValue;
      Server.friendlist = new List<string>();
      Server.gmlist = new List<string>();
      Server.ignoreaislinglist = new List<string>();
      Server.CustomLoot = new List<int>();
      Server.IdentifyItems = new List<string>();
      Server.TrashList = new List<string>();
      Server.AlertNonFriendTimer = new System.Timers.Timer(30000.0);
      Server.AlertNonFriendTimer.Elapsed += new ElapsedEventHandler(Server.AlertNonFriendOkay);
      Server.AlertNonFriendTimer.Enabled = true;
      Server.AlertNonFriendTimer.Stop();
      this.LoadTimedEvents();
      this.LoadGMList();
      this.LoadLists();
      this.ClientMessageHandlers = new ClientMessageHandler[256];
      this.ServerMessageHandlers = new ServerMessageHandler[256];
      for (int index = 0; index < this.ClientMessageHandlers.Length; ++index)
        this.ClientMessageHandlers[index] = (ClientMessageHandler) ((client, msg) => true);
      for (int index = 0; index < this.ServerMessageHandlers.Length; ++index)
        this.ServerMessageHandlers[index] = (ServerMessageHandler) ((client, msg) => true);
      this.ClientMessageHandlers[3] = new ClientMessageHandler(this.ClientMessage_0x03_LogIn);
      this.ClientMessageHandlers[6] = new ClientMessageHandler(this.ClientMessage_0x06_Walking);
      this.ClientMessageHandlers[8] = new ClientMessageHandler(this.ClientMessage_0x08_Drop);
      this.ClientMessageHandlers[11] = new ClientMessageHandler(this.ClientMessage_0x0B_LogOut);
      this.ClientMessageHandlers[14] = new ClientMessageHandler(this.ClientMessage_0x0E_Speak);
      this.ClientMessageHandlers[15] = new ClientMessageHandler(this.ClientMessage_0x0F_UseSpell);
      this.ClientMessageHandlers[16] = new ClientMessageHandler(this.ClientMessage_0x10_ClientJoin);
      this.ClientMessageHandlers[19] = new ClientMessageHandler(this.ClientMessage_0x13_Assail);
      this.ClientMessageHandlers[28] = new ClientMessageHandler(this.ClientMessage_0x1C_UseItem);
      this.ClientMessageHandlers[46] = new ClientMessageHandler(this.ClientMessage_0x2E_Group);
      this.ClientMessageHandlers[48] = new ClientMessageHandler(this.ClientMessage_0x30_SwapSlots);
      this.ClientMessageHandlers[57] = new ClientMessageHandler(this.ClientMessage_0x39_DialogueSelect);
      this.ClientMessageHandlers[58] = new ClientMessageHandler(this.ClientMessage_0x3A_PopupSelect);
      this.ClientMessageHandlers[62] = new ClientMessageHandler(this.ClientMessage_0x3E_UseSkill);
      this.ClientMessageHandlers[63] = new ClientMessageHandler(this.ClientMessage_0x3F_WorldMapSelect);
      this.ClientMessageHandlers[67] = new ClientMessageHandler(this.ClientMessage_0x43_ClickCharacter);
      this.ClientMessageHandlers[68] = new ClientMessageHandler(this.ClientMessage_0x44_UnequipGear);
      this.ClientMessageHandlers[77] = new ClientMessageHandler(this.ClientMessage_0x4D_SpellLines);
      this.ServerMessageHandlers[3] = new ServerMessageHandler(this.ServerMessage_0x03_Redirect);
      this.ServerMessageHandlers[4] = new ServerMessageHandler(this.ServerMessage_0x04_Location);
      this.ServerMessageHandlers[5] = new ServerMessageHandler(this.ServerMessage_0x05_PlayerID);
      this.ServerMessageHandlers[7] = new ServerMessageHandler(this.ServerMessage_0x07_DisplayNPC);
      this.ServerMessageHandlers[8] = new ServerMessageHandler(this.ServerMessage_0x08_Statistics);
      this.ServerMessageHandlers[10] = new ServerMessageHandler(this.ServerMessage_0x0A_SystemMessage);
      this.ServerMessageHandlers[11] = new ServerMessageHandler(this.ServerMessage_0x0B_MoveClient);
      this.ServerMessageHandlers[12] = new ServerMessageHandler(this.ServerMessage_0x0C_MoveCharacter);
      this.ServerMessageHandlers[13] = new ServerMessageHandler(this.ServerMessage_0x0D_Chat);
      this.ServerMessageHandlers[14] = new ServerMessageHandler(this.ServerMessage_0x0E_RemoveCharacter);
      this.ServerMessageHandlers[15] = new ServerMessageHandler(this.ServerMessage_0x0F_AddItem);
      this.ServerMessageHandlers[16] = new ServerMessageHandler(this.ServerMessage_0x10_RemoveItem);
      this.ServerMessageHandlers[17] = new ServerMessageHandler(this.ServerMessage_0x11_CharacterTurn);
      this.ServerMessageHandlers[19] = new ServerMessageHandler(this.ServerMessage_0x13_HpBar);
      this.ServerMessageHandlers[21] = new ServerMessageHandler(this.ServerMessage_0x15_MapInfo);
      this.ServerMessageHandlers[23] = new ServerMessageHandler(this.ServerMessage_0x17_AddSpell);
      this.ServerMessageHandlers[24] = new ServerMessageHandler(this.ServerMessage_0x18_RemoveSpell);
      this.ServerMessageHandlers[25] = new ServerMessageHandler(this.ServerMessage_0x19_SoundEffect);
      this.ServerMessageHandlers[26] = new ServerMessageHandler(this.ServerMessage_0x1A_BodyAnimation);
      this.ServerMessageHandlers[31] = new ServerMessageHandler(this.ServerMessage_0x1F_NewMap);
      this.ServerMessageHandlers[41] = new ServerMessageHandler(this.ServerMessage_0x29_SpellAnimation);
      this.ServerMessageHandlers[44] = new ServerMessageHandler(this.ServerMessage_0x2C_AddSkill);
      this.ServerMessageHandlers[45] = new ServerMessageHandler(this.ServerMessage_0x2D_RemoveSkill);
      this.ServerMessageHandlers[46] = new ServerMessageHandler(this.ServerMessage_0x2E_DisplayWorldMap);
      this.ServerMessageHandlers[47] = new ServerMessageHandler(this.ServerMessage_0x2F_DialogueResponse);
      this.ServerMessageHandlers[48] = new ServerMessageHandler(this.ServerMessage_0x30_PopupResponse);
      this.ServerMessageHandlers[49] = new ServerMessageHandler(this.ServerMessage_0x31_MailMenu);
      this.ServerMessageHandlers[51] = new ServerMessageHandler(this.ServerMessage_0x33_DisplayPlayer);
      this.ServerMessageHandlers[52] = new ServerMessageHandler(this.ServerMessage_0x34_Legend);
      this.ServerMessageHandlers[54] = new ServerMessageHandler(this.ServerMessage_0x36_CountryList);
      this.ServerMessageHandlers[55] = new ServerMessageHandler(this.ServerMessage_0x37_AddAppendage);
      this.ServerMessageHandlers[56] = new ServerMessageHandler(this.ServerMessage_0x38_RemoveAppendage);
      this.ServerMessageHandlers[57] = new ServerMessageHandler(this.ServerMessage_0x39_Profile);
      this.ServerMessageHandlers[58] = new ServerMessageHandler(this.ServerMessage_0x3A_SpellBar);
      this.ServerMessageHandlers[63] = new ServerMessageHandler(this.ServerMessage_0x3F_Cooldown);
      this.ServerMessageHandlers[66] = new ServerMessageHandler(this.ServerMessage_0x42_ExchangeWindow);
      this.ServerMessageHandlers[76] = new ServerMessageHandler(this.ServerMessage_0x4C_LogOffSignal);
      this.ServerMessageHandlers[96] = new ServerMessageHandler(this.ServerMessage_0x60_OK);
      this.ServerMessageHandlers[103] = new ServerMessageHandler(this.ServerMessage_0x67_WorldMapResponse);
      this.RemoteEndPoint = new IPEndPoint(IPAddress.Parse("52.88.55.94"), 2610);
      this.ServerLoopThread = new Thread(new ThreadStart(this.ServerLoop));
      this.ServerLoopThread.Start();
      Server.TimedEvents = new Thread(new ThreadStart(this.EventsLoop));
      Server.TimedEvents.Start();
    }

    /// <summary>
    /// Loads walk location definitions from the application's walklocations.xml file.
    /// </summary>
    /// <remarks>The walklocations.xml file must exist in the application's Settings directory. Each location
    /// entry consists of a 'speech' attribute that can be used with the /walk or /walk all command, as well as 
    /// 'Area' (e.g. Mileth) and 'Spot' (e.g. Altar) </remarks>
    /// <returns>A dictionary mapping speech to their corresponding walk locations.</returns>
    public static Dictionary<string, WalkLocation> LoadWalkLocations()
    {
      string filePath = Program.StartupPath + "\\Settings\\walklocations.xml";

      var locations = XDocument.Load(filePath)
    .Root
    .Elements("Location")
    .Select((location, index) => new
    {
      Index = index + 1, // 1-based for humans
      Name = (string)location.Attribute("speech"),
      Area = (string)location.Element("Area"),
      Spot = (string)location.Element("Spot")
    })
    .Where(x =>
        !string.IsNullOrWhiteSpace(x.Name) &&
        x.Area != null &&
        x.Spot != null
    )
    .ToList();

      var duplicate = locations
    .GroupBy(x => x.Name)
    .FirstOrDefault(g => g.Count() > 1);

      if (duplicate != null)
      {
        var message =
            $"Duplicate 'speech' value found: '{duplicate.Key}'\n\n" +
            $"Occurrences:\n" +
            string.Join("\n", duplicate.Select(d =>
                $"  Location #{d.Index}: Area='{d.Area}', Spot='{d.Spot}'"));

        throw new InvalidOperationException(message);
      }

      try
      {
        return XDocument.Load(filePath)
            .Root?
            .Elements("Location")
            .Select(location => new
            {
              Name = (string)location.Attribute("speech"),
              Area = (string)location.Element("Area"),
              Spot = (string)location.Element("Spot")
            })
            .Where(x =>
                !string.IsNullOrWhiteSpace(x.Name) &&
                x.Area != null &&
                x.Spot != null
            )
            .ToDictionary(
                x => x.Name,
                x => new WalkLocation
                {
                  Area = x.Area,
                  Location = x.Spot
                });
      }
      catch (FileNotFoundException ex)
      {
        MessageBox.Show("walklocations.xml not found, creating empty file. " + ex.Message, "Warning", MessageBoxButtons.OK);
      }
      catch (XmlException ex)
      {
        MessageBox.Show("Invalid XML format in walklocations.xml: " + ex.Message, "Error", MessageBoxButtons.OK);
      }
      catch (Exception ex)
      {
        MessageBox.Show("An unexpected error occurd reading walklocations.xml" + ex.Message, "Error", MessageBoxButtons.OK);
      }



      return new Dictionary<string, WalkLocation>();
    }


    /// <summary>
    /// Loads all spell definitions from the SpellList.xml file and returns them as a dictionary.
    /// </summary>
    /// <remarks>The method reads the SpellList.xml file located in the application's Settings directory. Each
    /// spell entry must include a name, mana cost, and base lines. Spells with missing or invalid names are
    /// ignored.</remarks>
    /// <returns>A dictionary containing spell names as keys and their corresponding <see cref="SpellData"/> objects as values.
    /// The dictionary will be empty if no spells are defined.</returns>
    public static Dictionary<string, SpellData> LoadSpells()
    {
      string filePath = Path.Combine(Program.StartupPath, "Settings", "SpellList.xml");
      if (!File.Exists(filePath))
      {
        MessageBox.Show("SpellList.xml not found in Settings folder. Continuing with an empty spell list", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return new Dictionary<string, SpellData>();
      }

      var document = XDocument.Load(filePath);
      var spellList = new Dictionary<string, SpellData>();

      foreach (var spell in document.Root.Elements("Spell"))
      {
        string name = spell.Attribute("name")?.Value;

        // Skip if name is null or whitespace
        if (string.IsNullOrWhiteSpace(name))
          continue;

        int manaCost = (int)spell.Element("ManaCost");
        int baseLines = (int)spell.Element("BaseLines");


        spellList[name] = new SpellData(
          name,
          manaCost,
          baseLines
        );

      }

      return spellList;
    }

    public void PopulateNodes()
    {
      string filePath = Path.Combine(Program.StartupPath, "Settings", "HerbNodes.xml");
      if (!File.Exists(filePath))
      {
//        MessageBox.Show("HerbNodes.xml not found in Settings folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.Load(filePath);
      foreach (XmlElement childNode in xmlDocument.DocumentElement.ChildNodes)
      {
        if (childNode.InnerText != string.Empty)
        {
          string[] strArray = childNode.InnerText.Split(',');
          int x = int.Parse(strArray[0]);
          int y = int.Parse(strArray[1]);
          int num = int.Parse(strArray[2]);
          string str2 = childNode.Value;
          Location location = new Location(x, y);
          HerbNode herbNode = new HerbNode();
          herbNode.Type = str2;
          herbNode.Location.X = x;
          herbNode.Location.Y = y;
          herbNode.Map = num;
          herbNode.Active = true;
          if (childNode.Name == "Hydele" && !Server.HydeleNodes.ContainsKey(childNode.InnerText))
            Server.HydeleNodes.Add(childNode.InnerText, herbNode);
          else if (childNode.Name == "Betony" && !Server.BetonyNodes.ContainsKey(childNode.InnerText))
            Server.BetonyNodes.Add(childNode.InnerText, herbNode);
          else if (childNode.Name == "Personaca" && !Server.PersonacaNodes.ContainsKey(childNode.InnerText))
            Server.PersonacaNodes.Add(childNode.InnerText, herbNode);
          else if (childNode.Name != "Personaca" && childNode.Name != "Hydele" && childNode.Name != "Betony" && !Server.HerbNodes.ContainsKey(childNode.InnerText))
            Server.HerbNodes.Add(childNode.InnerText, herbNode);
        }
      }
    }

    public void PopulateChestDatabase()
    {
      string str = Path.Combine(Program.StartupPath, "Settings", "ItemDatabase", "treasurechests.xml");
      if (!System.IO.File.Exists(str))
        return;
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.Load(str);
      foreach (XmlElement childNode1 in xmlDocument.DocumentElement.ChildNodes)
      {
        ChestItemXML chestItemXml = new ChestItemXML("Andor Chest", 0U);
        foreach (XmlElement childNode2 in childNode1.ChildNodes)
        {
          if (childNode2.Name == "ChestName")
            chestItemXml.Name = childNode2.InnerText;
          else if (childNode2.Name == "Opened")
            chestItemXml.OpenedCount = uint.Parse(childNode2.InnerText);
          else if (childNode2.Name == "Item")
          {
            string key = "";
            int num = 0;
            foreach (XmlElement childNode3 in childNode2.ChildNodes)
            {
              if (childNode3.Name == "ItemName")
                key = childNode3.InnerText;
              else if (childNode3.Name == "DropCount")
                num = int.Parse(childNode3.InnerText);
            }
            chestItemXml.Treasure.Add(key, num);
          }
        }
        if (!Server.ChestDatabase.ContainsKey(chestItemXml.Name))
          Server.ChestDatabase.Add(chestItemXml.Name, chestItemXml);
        else
          Server.ChestDatabase[chestItemXml.Name] = chestItemXml;
      }
    }

    public void PopulateItemDatabase()
    {
      string str1 = Program.StartupPath + "\\Settings\\ItemDatabase.xml";
      if (!File.Exists(str1))
      {
        // MessageBox.Show("ItemDatabase.xml not found in Settings folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
        
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.Load(str1);
      foreach (XmlElement childNode1 in xmlDocument.DocumentElement.ChildNodes)
      {
        ItemXML itemXml = new ItemXML();
        foreach (XmlElement childNode2 in childNode1.ChildNodes)
        {
          if (childNode2.Name == "Name")
            itemXml.Name = childNode2.InnerText;
          else if (childNode2.Name == "SecondName")
            itemXml.SecondName = childNode2.InnerText;
          else if (childNode2.Name == "Image" && childNode2.InnerText != string.Empty)
            itemXml.Image = int.Parse(childNode2.InnerText);
          else if (childNode2.Name == "Usedfor")
          {
            string[] strArray = childNode2.InnerText.Split('\n');
            if (strArray.Length != 0)
            {
              foreach (string str2 in strArray)
              {
                if (str2 != string.Empty)
                  itemXml.Usedfor.Add(str2.Trim());
              }
            }
          }
          else if (childNode2.Name == "Obtainedby")
          {
            string[] strArray = childNode2.InnerText.Split('\n');
            if (strArray.Length != 0)
            {
              foreach (string str3 in strArray)
              {
                if (str3 != string.Empty)
                  itemXml.Obtainedby.Add(str3.Trim());
              }
            }
          }
        }
        Server.ItemDatabase.Add(itemXml.Name, itemXml);
      }
    }

    private void EventsLoop()
    {
      while (true)
      {
        this.PopulateSkullListView();
        this.PopulateTasksListView();
        Thread.Sleep(1000);
      }
    }

    public void PopulateAscendLogListView(AscendData z)
    {
      if (Server.AscendLog.Count<AscendData>() <= 0 || z == null)
        return;
      Program.MainForm.ascensionlistview.BeginUpdate();
      Program.MainForm.ascensionlistview.Items.Insert(0, new ListViewItem(z.Time)
      {
        SubItems = {
          z.Name,
          z.EXP,
          z.Increase
        }
      });
      Program.MainForm.ascensionlistview.EndUpdate();
    }

    public void SaveAscendLog()
    {
      XDocument xdocument = new XDocument();
      xdocument.Add((object) new XElement((XName) "Ascended"));
      foreach (AscendData ascendData in Server.AscendLog)
        xdocument.Element((XName) "Ascended").Add((object) new XElement((XName) "Char", (object) (ascendData.Time + "|" + ascendData.Name + "|" + ascendData.EXP + "|" + ascendData.Increase)));
      xdocument.Save(Program.StartupPath + "\\Settings\\Ascended.xml");
    }

    public void LoadAscendLog()
    {
      if (!Directory.Exists(Program.StartupPath + "\\Settings"))
        return;
      if (!System.IO.File.Exists(Program.StartupPath + "\\Settings\\Ascended.xml"))
      {
        System.IO.File.Create(Program.StartupPath + "\\Settings\\Ascended.xml").Close();
        XDocument xdocument = new XDocument();
        xdocument.Add((object) new XElement((XName) "Ascended"));
        xdocument.Save(Program.StartupPath + "\\Settings\\Ascended.xml");
      }
      try
      {
        foreach (XElement element in XDocument.Load(Program.StartupPath + "\\Settings\\Ascended.xml").Element((XName) "Ascended").Elements((XName) "Char"))
        {
          string[] strArray = element.Value.Split('|');
          AscendData z = new AscendData();
          z.Time = strArray[0];
          z.Name = strArray[1];
          z.EXP = strArray[2];
          z.Increase = strArray[3];
          Server.AscendLog.Add(z);
          this.PopulateAscendLogListView(z);
        }
      }
      catch
      {
        XDocument xdocument = new XDocument();
        xdocument.Add((object) new XElement((XName) "Ascended"));
        xdocument.Save(Program.StartupPath + "\\Settings\\Ascended.xml");
      }
    }

    public void PopulateSkullListView()
    {
      if (Server.SkullList.Count == 0)
        return;

      var listView = Program.MainForm.skulledlistview;

      listView.BeginUpdate();
      foreach (var skullData in Server.SkullList.Values)
      {
        if (skullData == null)
          continue;

        if (!listView.Items.ContainsKey(skullData.Name))
        {
          var item = listView.Items.Add(skullData.Name, skullData.Name, -1);
          item.SubItems.Add(skullData.Map);
          item.SubItems.Add(skullData.XY);
        }
        else
        {
          var item = listView.Items[skullData.Name];
          item.SubItems[1].Text = skullData.Map;
          item.SubItems[2].Text = skullData.XY;
        }
      }
      listView.EndUpdate();
    }

    public void SaveSkullList()
    {
      XDocument xdocument = new XDocument();
      xdocument.Add((object) new XElement((XName) "Skulled"));
      foreach (SkullData skullData in Server.SkullList.Values)
        xdocument.Element((XName) "Skulled").Add((object) new XElement((XName) "Char", (object) (skullData.Name + "|" + skullData.Map + "|" + skullData.XY)));
      xdocument.Save(Program.StartupPath + "\\Settings\\skulled.xml");
    }

    public void LoadSkullList()
    {
      if (!Directory.Exists(Program.StartupPath + "\\Settings"))
        return;
      if (!System.IO.File.Exists(Program.StartupPath + "\\Settings\\skulled.xml"))
      {
        System.IO.File.Create(Program.StartupPath + "\\Settings\\skulled.xml").Close();
        XDocument xdocument = new XDocument();
        xdocument.Add((object) new XElement((XName) "Skulled"));
        xdocument.Save(Program.StartupPath + "\\Settings\\skulled.xml");
      }
      try
      {
        foreach (XElement element in XDocument.Load(Program.StartupPath + "\\Settings\\skulled.xml").Element((XName) "Skulled").Elements((XName) "Char"))
        {
          string[] strArray = element.Value.Split('|');
          SkullData skullData = new SkullData();
          skullData.Name = strArray[0];
          skullData.Map = strArray[1];
          skullData.XY = strArray[2];
          if (!Server.SkullList.ContainsKey(skullData.Name))
            Server.SkullList.Add(skullData.Name, skullData);
          else
            Server.SkullList[skullData.Name] = skullData;
        }
      }
      catch
      {
        XDocument xdocument = new XDocument();
        xdocument.Add((object) new XElement((XName) "Skulled"));
        xdocument.Save(Program.StartupPath + "\\Settings\\skulled.xml");
      }
    }

    public void PopulateTasksListView()
    {
      if (Server.TimedEvent.Count<KeyValuePair<string, Flintstones.TimedEvent>>() <= 0)
        return;
      foreach (Flintstones.TimedEvent timedEvent in (IEnumerable<Flintstones.TimedEvent>) ((IEnumerable<Flintstones.TimedEvent>) Server.TimedEvent.Values.ToArray<Flintstones.TimedEvent>()).OrderBy<Flintstones.TimedEvent, uint>((Func<Flintstones.TimedEvent, uint>) (z => z.TimeLeft())))
      {
        if (Server.TimedEvent.ContainsKey(timedEvent.Name + timedEvent.Event) && timedEvent != null)
        {
          if (Program.MainForm.TaskFilter.Text != "All" && Program.MainForm.TaskFilter.Text != timedEvent.Event)
          {
            if (Program.MainForm.recenttaskslist.Items.ContainsKey(timedEvent.Name + timedEvent.Event) && Program.MainForm.recenttaskslist.Items[timedEvent.Name + timedEvent.Event] != null)
            {
              Program.MainForm.recenttaskslist.BeginUpdate();
              Program.MainForm.recenttaskslist.Items[timedEvent.Name + timedEvent.Event].Remove();
              Program.MainForm.recenttaskslist.EndUpdate();
            }
          }
          else if (!timedEvent.Track && !Program.MainForm.showall.Checked)
          {
            if (Program.MainForm.recenttaskslist.Items.ContainsKey(timedEvent.Name + timedEvent.Event) && Program.MainForm.recenttaskslist.Items[timedEvent.Name + timedEvent.Event] != null)
            {
              Program.MainForm.recenttaskslist.BeginUpdate();
              Program.MainForm.recenttaskslist.Items[timedEvent.Name + timedEvent.Event].Remove();
              Program.MainForm.recenttaskslist.EndUpdate();
            }
          }
          else
          {
            if (timedEvent.TimeLeft() == 0U && timedEvent.Track && !timedEvent.Messaged && Server.Alts.Values.Count<Client>() > 0)
            {
              foreach (Client client in Server.Alts.Values.ToArray<Client>())
              {
                if (client != null && client.LoggedOn)
                {
                  string titleCase = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(timedEvent.Name);
                  client.SendMessage(titleCase + " can repeat: " + timedEvent.Event + "!", "pink");
                  timedEvent.Messaged = true;
                }
              }
            }
            int num1;
            if (!Program.MainForm.recenttaskslist.Items.ContainsKey(timedEvent.Name + timedEvent.Event))
            {
              string text = "Quest Available!";
              double num2 = (double) timedEvent.TimeLeft();
              if (num2 <= 1.0 && num2 != 0.0)
                text = "<1 min";
              else if (num2 <= 60.0 && num2 != 0.0)
              {
                num1 = (int) num2;
                text = num1.ToString() + " mins";
              }
              else if (num2 >= 1440.0)
              {
                string[] strArray = new string[6];
                num1 = (int) Math.Floor(num2 / 1440.0);
                strArray[0] = num1.ToString();
                strArray[1] = " days ";
                num1 = (int) Math.Floor(num2 % 1440.0 / 60.0);
                strArray[2] = num1.ToString();
                strArray[3] = " hrs ";
                num1 = (int) Math.Floor(num2 % 60.0);
                strArray[4] = num1.ToString();
                strArray[5] = " mins";
                text = string.Concat(strArray);
              }
              else if (num2 >= 60.0)
              {
                num1 = (int) Math.Floor(num2 / 60.0);
                string str1 = num1.ToString();
                num1 = (int) Math.Floor(num2 % 60.0);
                string str2 = num1.ToString();
                text = str1 + " hrs " + str2 + " mins";
              }
              Program.MainForm.recenttaskslist.BeginUpdate();
              ListViewItem listViewItem = Program.MainForm.recenttaskslist.Items.Add(timedEvent.Name + timedEvent.Event, timedEvent.Name, -1);
              listViewItem.SubItems.Add(timedEvent.Event);
              listViewItem.SubItems.Add(text);
              listViewItem.Checked = timedEvent.Track;
              Program.MainForm.recenttaskslist.EndUpdate();
            }
            else if (Program.MainForm.recenttaskslist.Items[timedEvent.Name + timedEvent.Event] != null)
            {
              string str3 = "Quest Available!";
              double num3 = (double) timedEvent.TimeLeft();
              if (num3 <= 1.0 && num3 != 0.0)
                str3 = "<1 min";
              else if (num3 <= 60.0 && num3 != 0.0)
              {
                num1 = (int) num3;
                str3 = num1.ToString() + " mins";
              }
              else if (num3 >= 1440.0)
              {
                string[] strArray = new string[6];
                num1 = (int) Math.Floor(num3 / 1440.0);
                strArray[0] = num1.ToString();
                strArray[1] = " days ";
                num1 = (int) Math.Floor(num3 % 1440.0 / 60.0);
                strArray[2] = num1.ToString();
                strArray[3] = " hrs ";
                num1 = (int) Math.Floor(num3 % 60.0);
                strArray[4] = num1.ToString();
                strArray[5] = " mins";
                str3 = string.Concat(strArray);
              }
              else if (num3 >= 60.0)
              {
                num1 = (int) Math.Floor(num3 / 60.0);
                string str4 = num1.ToString();
                num1 = (int) Math.Floor(num3 % 60.0);
                string str5 = num1.ToString();
                str3 = str4 + " hrs " + str5 + " mins";
              }
              if (str3 != Program.MainForm.recenttaskslist.Items[timedEvent.Name + timedEvent.Event].SubItems[2].Text)
              {
                Program.MainForm.recenttaskslist.BeginUpdate();
                Program.MainForm.recenttaskslist.Items[timedEvent.Name + timedEvent.Event].SubItems[2].Text = str3;
                Program.MainForm.recenttaskslist.EndUpdate();
              }
            }
          }
        }
      }
    }

    public void SaveTasksList()
    {
      XDocument xdocument = new XDocument();
      xdocument.Add((object) new XElement((XName) "TimedEvents"));
      foreach (Flintstones.TimedEvent timedEvent in Server.TimedEvent.Values)
        xdocument.Element((XName) "TimedEvents").Add((object) new XElement((XName) "Event", (object) (timedEvent.Name + "|" + timedEvent.Event + "|" + timedEvent.Time.ToString() + "|" + timedEvent.Track.ToString())));
      if (xdocument.Nodes().Count<XNode>() <= 0 || Server.TimedEvent.Values.Count <= 0 || !xdocument.Element((XName) "TimedEvents").HasElements)
        return;
      xdocument.Save(Program.StartupPath + "\\Settings\\timedevents.xml");
    }

    public void LoadTasksList()
    {
      if (!Directory.Exists(Program.StartupPath + "\\Settings"))
        return;
      if (!System.IO.File.Exists(Program.StartupPath + "\\Settings\\timedevents.xml"))
      {
        System.IO.File.Create(Program.StartupPath + "\\Settings\\timedevents.xml").Close();
        XDocument xdocument = new XDocument();
        xdocument.Add((object) new XElement((XName) "TimedEvents"));
        xdocument.Save(Program.StartupPath + "\\Settings\\timedevents.xml");
      }
      try
      {
        foreach (XElement element in XDocument.Load(Program.StartupPath + "\\Settings\\timedevents.xml").Element((XName) "TimedEvents").Elements((XName) "Event"))
        {
          string[] strArray = element.Value.Split('|');
          Flintstones.TimedEvent timedEvent = new Flintstones.TimedEvent();
          timedEvent.Name = strArray[0];
          timedEvent.Time = Convert.ToDateTime(strArray[2]);
          timedEvent.Event = strArray[1];
          timedEvent.Track = Convert.ToBoolean(strArray[3]);
          if (!Server.TimedEvent.ContainsKey(timedEvent.Name + timedEvent.Event))
            Server.TimedEvent.Add(timedEvent.Name + timedEvent.Event, timedEvent);
          else
            Server.TimedEvent[timedEvent.Name + timedEvent.Event] = timedEvent;
        }
      }
      catch
      {
        XDocument xdocument = new XDocument();
        xdocument.Add((object) new XElement((XName) "TimedEvents"));
        xdocument.Save(Program.StartupPath + "\\Settings\\timedevents.xml");
      }
    }

    public void LoadLists()
    {
      if (!Directory.Exists(Program.StartupPath + "\\Settings\\Lists"))
        Directory.CreateDirectory(Program.StartupPath + "\\Settings\\Lists");
      if (!File.Exists(Program.StartupPath + "\\Settings\\Lists\\customlootlist.txt"))
      {
        File.Create(Program.StartupPath + "\\Settings\\Lists\\customlootlist.txt").Close();
      }
      else
      {
        if (Server.CustomLoot != null)
          Server.CustomLoot.Clear();
        StreamReader streamReader = new StreamReader(Program.StartupPath + "\\Settings\\Lists\\customlootlist.txt");
        string s;
        while ((s = streamReader.ReadLine()) != null)
          Server.CustomLoot.Add(int.Parse(s));
        streamReader.Close();
      }
      if (!File.Exists(Program.StartupPath + "\\Settings\\Lists\\identifyitemslist.txt"))
      {
        File.Create(Program.StartupPath + "\\Settings\\Lists\\identifyitemslist.txt").Close();
      }
      else
      {
        if (Server.IdentifyItems != null)
          Server.IdentifyItems.Clear();
        StreamReader streamReader = new StreamReader(Program.StartupPath + "\\Settings\\Lists\\identifyitemslist.txt");
        string str;
        while ((str = streamReader.ReadLine()) != null)
          Server.IdentifyItems.Add(str.ToLower());
        streamReader.Close();
      }
      if (!File.Exists(Program.StartupPath + "\\Settings\\Lists\\trashlist.txt"))
      {
        File.Create(Program.StartupPath + "\\Settings\\Lists\\trashlist.txt").Close();
      }
      else
      {
        if (Server.TrashList != null)
          Server.TrashList.Clear();
        StreamReader streamReader = new StreamReader(Program.StartupPath + "\\Settings\\Lists\\trashlist.txt");
        string str;
        while ((str = streamReader.ReadLine()) != null)
          Server.TrashList.Add(str.ToLower());
        streamReader.Close();
      }
      if (!File.Exists(Program.StartupPath + "\\Settings\\Lists\\ignoreaislingslist.txt"))
      {
        File.Create(Program.StartupPath + "\\Settings\\Lists\\ignoreaislingslist.txt").Close();
      }
      else
      {
        if (Server.ignoreaislinglist != null)
          Server.ignoreaislinglist.Clear();
        StreamReader streamReader = new StreamReader(Program.StartupPath + "\\Settings\\Lists\\ignoreaislingslist.txt");
        string str;
        while ((str = streamReader.ReadLine()) != null)
          Server.ignoreaislinglist.Add(str);
        streamReader.Close();
      }
    }

    public void LoadGMList()
    {
      if (Server.gmlist != null)
        Server.gmlist.Clear();
      if (!Directory.Exists(Program.StartupPath + "\\Settings"))
        return;
      if (!File.Exists(Program.StartupPath + "\\Settings\\GMs.txt"))
      {
        StreamWriter streamWriter = new StreamWriter(Program.StartupPath + "\\Settings\\GMs.txt", true);
        streamWriter.WriteLine("kru");
        streamWriter.WriteLine("joeker");
        streamWriter.WriteLine("ishikawa");
        streamWriter.WriteLine("error");
        streamWriter.WriteLine("eduardo");
        streamWriter.WriteLine("law");
        streamWriter.WriteLine("listen");
        streamWriter.WriteLine("trial");
        streamWriter.WriteLine("and");
        streamWriter.WriteLine("justice");
        streamWriter.WriteLine("reyakeely");
        streamWriter.WriteLine("angelique");
        streamWriter.WriteLine("etienne");
        streamWriter.WriteLine("venezia");
        streamWriter.WriteLine("viveena");
        streamWriter.WriteLine("dionia");
        streamWriter.Close();
        Server.gmlist.Add("kru");
        Server.gmlist.Add("joeker");
        Server.gmlist.Add("ishikawa");
        Server.gmlist.Add("error");
        Server.gmlist.Add("eduardo");
        Server.gmlist.Add("law");
        Server.gmlist.Add("listen");
        Server.gmlist.Add("trial");
        Server.gmlist.Add("and");
        Server.gmlist.Add("justice");
        Server.gmlist.Add("reyakeely");
        Server.gmlist.Add("angelique");
        Server.gmlist.Add("etienne");
        Server.gmlist.Add("venezia");
        Server.gmlist.Add("viveena");
        Server.gmlist.Add("dionia");
      }
      else
      {
        StreamReader streamReader = new StreamReader(Program.StartupPath + "\\Settings\\GMs.txt");
        string str;
        while ((str = streamReader.ReadLine()) != null)
          Server.gmlist.Add(str);
        streamReader.Close();
      }
    }

    public void LoadTimedEvents()
    {
    }

    public static void UpdateFriends()
    {
      if (Server.friendlist != null)
        Server.friendlist.Clear();
      if (Program.MainForm != null && Program.MainForm.friendlistbox.Items.Count > 0)
      {
        for (int index = 0; index <= Program.MainForm.friendlistbox.Items.Count - 1; ++index)
          Server.friendlist.Add(Program.MainForm.friendlistbox.Items[index].ToString());
      }
      foreach (Client client in Server.Clients)
      {
        if (client != null && client.Name != string.Empty)
          client.FriendListUpdate();
      }
    }

    public static void AlertNonFriendOkay(object sender, EventArgs e)
    {
      Server.AlertNonFriendTimer.Stop();
      Server.alertfornonfriends = true;
    }

    private static string GetFirstLine(string text)
    {
      using (StringReader stringReader = new StringReader(text))
        return stringReader.ReadLine();
    }

    public void ServerLoop()
    {
      Running = true;
      while (Running)
      {
        if (Listener.Pending())
        {
          Server.Clients.Add(new Client(this, Listener.AcceptSocket(), RemoteEndPoint));

          IPAddress ip;
          if (IPAddress.TryParse("52.88.55.94", out ip))
          {
            RemoteEndPoint = new IPEndPoint(ip, 2610);
          }
          else
          {
            Console.WriteLine("Invalid IP address.");
          }
        }
        Thread.Sleep(15);
      }
    }

    public bool ClientMessage_0x03_LogIn(Client client, ClientPacket msg)
    {
      client.Name = msg.ReadString((int) msg.ReadByte());
      client.Password = msg.ReadString((int) msg.ReadByte());
      if (!Server.Stuff.ContainsKey(client.Name))
        Server.Stuff.Add(client.Name, client.Password);
      else
        Server.Stuff[client.Name] = client.Password;
      if (!Server.DARegged.ContainsKey(client.Name))
        Server.DARegged.Add(client.Name, false);
      else
        Server.DARegged[client.Name] = false;
      if (this.SpoofClientId != 1 && this.SpoofClientId != 2)
        return true;
      string name = client.Name;
      string password = client.Password;
      int num1 = (int) msg.ReadByte();
      int num2 = (int) msg.ReadByte();
      uint num3 = msg.ReadUInt32();
      ushort num4 = msg.ReadUInt16();
      int num5 = (int) msg.ReadUInt32();
      int num6 = (int) msg.ReadUInt16();
      int num7 = (int) msg.ReadUInt16();
      Random random = new Random();
      if (this.SpoofClientId == 1)
      {
        num3 = (uint) random.Next();
        num4 = (ushort) random.Next();
      }
      else if (this.SpoofClientId == 2)
      {
        num3 = this.ClientId1;
        num4 = this.ClientId2;
      }
      msg.Clear();
      msg.WriteString8(name);
      msg.WriteString8(password);
      byte num8 = (byte) random.Next();
      byte num9 = (byte) random.Next();
      int num10 = (int) (byte) ((uint) num9 + 138U);
      int num11 = (int) num3;
      int num12 = num10;
      int num13 = (int) (byte) (num12 + 1);
      int num14 = (int) (byte) (num13 + 1);
      int num15 = num12 | num13 << 8;
      byte num16 = (byte) (num14 + 1);
      int num17 = num14 << 16;
      int num18 = num15 | num17 | (int) num16 << 24;
      uint num19 = (uint) (num11 ^ num18);
      int num20 = (int) (byte) ((uint) num9 + 94U);
      ushort num21 = (ushort) ((uint) num4 ^ (uint) (ushort) (num20 | (int) (byte) (num20 + 1) << 8));
      uint num22 = (uint) (ushort) random.Next();
      int num23 = (int) (byte) ((uint) num9 + 115U);
      int num24 = (int) num22;
      int num25 = num23;
      int num26 = (int) (byte) (num25 + 1);
      int num27 = (int) (byte) (num26 + 1);
      int num28 = num25 | num26 << 8;
      byte num29 = (byte) (num27 + 1);
      int num30 = num27 << 16;
      int num31 = num28 | num30 | (int) num29 << 24;
      uint num32 = (uint) (num24 ^ num31);
      msg.WriteByte(num8);
      msg.WriteByte((byte) ((uint) num9 ^ (uint) num8 + 59U));
      msg.WriteUInt32(num19);
      msg.WriteUInt16(num21);
      msg.WriteUInt32(num32);
      ushort num33 = KruCRC.Calculate(msg.BodyData, name.Length + password.Length + 2, 12);
      int num34 = (int) (byte) ((uint) num9 + 165U);
      ushort num35 = (ushort) ((uint) num33 ^ (uint) (ushort) (num34 | (int) (byte) (num34 + 1) << 8));
      msg.WriteUInt16(num35);
      msg.WriteUInt16((ushort) 256);
      msg.Write(new byte[3]);
      client.Enqueue(msg);
      return false;
    }

    public bool ClientMessage_0x06_Walking(Client client, ClientPacket msg)
    {
      switch (msg.ReadByte())
      {
        case 0:
          --client.ClientLocation.Y;
          break;
        case 1:
          ++client.ClientLocation.X;
          break;
        case 2:
          ++client.ClientLocation.Y;
          break;
        case 3:
          --client.ClientLocation.X;
          break;
      }
      if (client.LastMapLocation.ContainsKey(client.MapInfo.Number))
        client.LastMapLocation[client.MapInfo.Number] = new Location(client.ClientLocation.X, client.ClientLocation.Y);
      else
        client.LastMapLocation.Add(client.MapInfo.Number, new Location(client.ClientLocation.X, client.ClientLocation.Y));
      msg.BodyData[1] = client.WalkCounter++;
      client.laststep = DateTime.UtcNow;
      return true;
    }

    public bool ClientMessage_0x08_Drop(Client client, ClientPacket msg)
    {
      byte num1 = msg.ReadByte();
      ushort num2 = msg.ReadUInt16();
      ushort num3 = msg.ReadUInt16();
      int num4 = (int) msg.ReadUInt32();
      if (!client.HasItem("Warranty Bag") || !(client.Inventory[(int) num1 - 1].Name == "Succubus's Hair") || !client.MapInfo.Name.Equals("Mileth Village") || !num2.Equals((ushort) 31) || !num3.Equals((ushort) 52) && !num3.Equals((ushort) 53))
        return true;
      client.SendMessage("Deposit your Warranty Bag first.", "red");
      return false;
    }

    public bool ClientMessage_0x0B_LogOut(Client client, ClientPacket msg)
    {
      if (msg.ReadByte() == (byte) 0)
        client.logoff = true;
      if (Server.Alts.ContainsKey(client.Name.ToLower()))
        Server.Alts.Remove(client.Name.ToLower());
      foreach (Client client1 in Server.Alts.Values.ToArray<Client>())
      {
        if (client1 != null && client1.targetplayer != null)
        {
          foreach (targetPlayer targetPlayer in client1.targetplayer)
            targetPlayer?.updatePlayerTargets();
        }
      }
      return true;
    }

    public bool ClientMessage_0x0E_Speak(Client client, ClientPacket msg)
    {
      if (client.LastPermMessage != string.Empty)
        client.SendMessage(client.LastPermMessage, (byte) 18);
      int num = (int) msg.ReadByte();
      string str = msg.ReadString((int) msg.ReadByte());
      if (!str.StartsWith("/") || !client.Tab.vslash_commands)
        return true;
      if (client.SpeakMessage == string.Empty)
        client.SpeakMessage = str;
      return false;
    }

    public bool ClientMessage_0x0F_UseSpell(Client client, ClientPacket msg)
    {
      int num = (int) msg.ReadByte();
      Spell spell = client.SpellBook[num - 1];
      if (spell != null)
        client.LastSpell = spell.Name;
      if (msg.BodyData.Length > 6)
      {
        client.newtargetdelay = DateTime.UtcNow;
        client.LastTarget = msg.ReadUInt32();
        if (client.Characters.ContainsKey(client.LastTarget) && client.Characters[client.LastTarget] != null && client.Characters[client.LastTarget] is Npc && client.Characters[client.LastTarget].IsOnScreen)
          client.LastMonsterId = client.LastTarget;
      }
      client.ImCasting = false;
      client.castingoneline = false;
      return true;
    }

    public bool ClientMessage_0x10_ClientJoin(Client client, ClientPacket msg)
    {
      byte num1 = msg.ReadByte();
      byte[] numArray = msg.Read((int) msg.ReadByte());
      string str = msg.ReadString((int) msg.ReadByte());
      int num2 = (int) msg.ReadUInt32();
      client.Name = str;
      client.Tab.Text = str;
      string hashString = Program.GetHashString(Program.GetHashString(str));
      for (int index = 0; index < 31; ++index)
        hashString += Program.GetHashString(hashString);
      client.Seed = num1;
      client.Key = numArray;
      client.KeyTable = Encoding.ASCII.GetBytes(hashString);
      return true;
    }

    public bool ClientMessage_0x13_Assail(Client client, ClientPacket msg) => true;

    public bool ClientMessage_0x1C_UseItem(Client client, ClientPacket msg)
    {
      int num = (int) msg.ReadByte();
      if (num != 60)
      {
        Item obj = client.Inventory[num - 1];
        if (obj != null)
        {
          if (obj.Name == "Lucky Clover")
            client.ateclover = true;
          else if (obj.Name == "Golden Starfish")
            client.ategsf = true;
          if (obj.Name.Contains("Bonus") || obj.Name.Contains("Double") || obj.Name.Contains("Experience") || obj.Name.Contains("Ability"))
            obj.NextUse = !obj.Name.Equals("Experience Gem") ? DateTime.UtcNow.AddMilliseconds(5000.0) : DateTime.UtcNow.AddMilliseconds(30000.0);
          else if (obj.Name == "Sprint Potion")
            obj.NextUse = DateTime.UtcNow.AddMilliseconds(16000.0);
          else if (obj.Name == "Grime Scent")
            obj.NextUse = DateTime.UtcNow.AddMilliseconds(11000.0);
          else if (obj.Name == "Damage Scroll")
            obj.NextUse = DateTime.UtcNow.AddMilliseconds(31000.0);
          else if (obj.Name.Contains("Two Move Combo"))
          {
            if (!client.comboscrollused)
              ++client.comboscrolluse;
            if (client.comboscrolluse >= 2)
            {
              obj.NextUse = DateTime.UtcNow.AddMilliseconds(121000.0);
              client.ComboScrollTimer.Start();
              client.comboscrolluse = 0;
              client.comboscrollused = true;
            }
          }
          else if (obj.Name.Contains("Three Move Combo"))
          {
            if (!client.comboscrollused)
              ++client.comboscrolluse;
            if (client.comboscrolluse >= 3)
            {
              obj.NextUse = DateTime.UtcNow.AddMilliseconds(121000.0);
              client.ComboScrollTimer.Start();
              client.comboscrolluse = 0;
              client.comboscrollused = true;
            }
          }
          else
            obj.NextUse = DateTime.UtcNow.AddMilliseconds(325.0);
        }
      }
      return true;
    }

    public bool ClientMessage_0x2E_Group(Client client, ClientPacket msg)
    {
      byte num = msg.ReadByte();
      string str = msg.ReadString((int) msg.ReadByte());
      if (!str.Contains("[") && !str.Contains(")"))
        return true;
      if (str.Contains("["))
        str = str.Remove(str.IndexOf("[") - 1);
      if (str.Contains(")"))
        str = str.Remove(0, str.IndexOf(" ") + 1);
      msg = new ClientPacket((byte) 46);
      msg.WriteByte(num);
      msg.WriteString8(str);
      msg.WriteByte((byte) 0);
      msg.WriteByte((byte) 0);
      msg.WriteByte(msg.Opcode);
      msg.Write(new byte[7]);
      client.Enqueue(msg);
      return false;
    }

    public bool ClientMessage_0x30_SwapSlots(Client client, ClientPacket msg)
    {
      byte num1 = msg.ReadByte();
      byte num2 = msg.ReadByte();
      byte num3 = msg.ReadByte();
      if (num1 == (byte) 0 && client.Tab.recorditemdata.Checked)
      {
        foreach (Character character in client.Characters.Values.ToArray<Character>())
        {
          if (character != null)
          {
            if (character.InventorySlot == (int) num2)
              character.InventorySlot = (int) num3;
            else if (character.InventorySlot == (int) num3)
              character.InventorySlot = (int) num2;
          }
        }
      }
      if (num1 == (byte) 2 && client.FakeSkills.Count > 0)
      {
        foreach (Skill skill in client.FakeSkills.Values)
        {
          if (skill != null)
          {
            if (skill.SkillSlot == (int) num2)
              skill.NewSlot = (int) num3;
            else if (skill.SkillSlot == (int) num3)
              skill.NewSlot = (int) num2;
          }
        }
      }
      return true;
    }

    public bool ClientMessage_0x39_DialogueSelect(Client client, ClientPacket msg)
    {
      client.CurrentnpcpopupID = 0U;
      client.Currentnpctext = string.Empty;
      client.Currentnpcscript = (byte) 0;
      msg.Read(6);
      int num1 = (int) msg.ReadByte();
      int num2 = (int) msg.ReadUInt32();
      int num3 = (int) msg.ReadByte();
      int num4 = (int) msg.ReadByte();
      if (num4 == 99 && client.sendmode == 2)
        client.sendmode = 1;
      if (num4 == 86 && client.withdrawmode == 2)
        client.withdrawmode = 1;
      if (num4 == 83 && client.depositmode == 2)
        client.depositmode = 1;
      return true;
    }

    public bool ClientMessage_0x3A_PopupSelect(Client client, ClientPacket msg)
    {
      string empty = string.Empty;
      client.CurrentnpcpopupID = 0U;
      client.Currentnpctext = string.Empty;
      client.Currentnpcscript = (byte) 0;
      msg.Read(6);
      int num1 = (int) msg.ReadByte();
      int num2 = (int) msg.ReadUInt32();
      int num3 = (int) msg.ReadByte();
      int num4 = (int) msg.ReadByte();
      int num5 = (int) msg.ReadByte();
      int num6 = (int) msg.ReadByte();
      int num7 = (int) msg.ReadByte();
      if (num7 == 1)
        client.closepopupvars();
      if (num7 == 2)
      {
        if (client.agchest)
        {
          client.agchest = false;
          client.agchestopen = true;
        }
        if (client.smallbag)
        {
          client.smallbag = false;
          client.smallbagopen = true;
        }
        if (client.bigbag)
        {
          client.bigbag = false;
          client.bigbagopen = true;
        }
        if (client.heavybag)
        {
          client.heavybag = false;
          client.heavybagopen = true;
        }
      }
      if (msg.Length >= (ushort) 22)
      {
        switch (msg.ReadByte())
        {
          case 1:
            byte num8 = msg.ReadByte();
            if (client.wdchest)
            {
              client.SaveTimedStuff(25);
              client.wdchest = false;
              if (num8 == (byte) 1 || num8 == (byte) 3)
                client.wdchestopen = true;
            }
            if (client.andorchest)
            {
              client.SaveTimedStuff(26);
              client.andorchest = false;
              if (num8 == (byte) 1 || num8 == (byte) 3)
                client.andorchestopen = true;
            }
            if (client.queenchest)
            {
              client.SaveTimedStuff(27);
              client.queenchest = false;
              if (num8 == (byte) 1 || num8 == (byte) 3)
              {
                client.queenchestopen = true;
                break;
              }
              break;
            }
            break;
          case 2:
            string str = msg.ReadString((int) msg.ReadByte());
            if (client.veltainchest)
            {
              client.veltainchest = false;
              client.veltainchestopen = true;
              client.chestfee = str;
            }
            if (client.heavychest)
            {
              client.heavychest = false;
              client.heavychestopen = true;
              client.chestfee = str;
              break;
            }
            break;
        }
      }
      return true;
    }

    public bool ClientMessage_0x3E_UseSkill(Client client, ClientPacket msg)
    {
      int num = (int) msg.ReadByte();
      Skill skill = (Skill) null;
      foreach (Skill skill1 in client.FakeSkills.Values)
      {
        if (skill1 != null && num == skill1.SkillSlot)
        {
          skill = skill1;
          break;
        }
      }
      if (skill != null && client.Combos.Count<KeyValuePair<string, string>>() > 0 && client.Combos.ContainsKey(skill.Name))
        new Thread((ThreadStart) (() =>
        {
          string combo = client.Combos[skill.Name];
          char[] chArray = new char[1]{ '|' };
          foreach (string str1 in combo.Split(chArray))
          {
            string str2 = str1.Trim();
            if (str2.Equals("space", StringComparison.CurrentCultureIgnoreCase) || str2.Equals("assail", StringComparison.CurrentCultureIgnoreCase))
              client.Assail();
            else if (!client.UseSkill(str2) && !client.UseMedSkill(str2) && !client.UseItem(str2))
              client.Cast(str2, new uint?());
          }
        })).Start();
      return true;
    }

    public bool ClientMessage_0x3F_WorldMapSelect(Client client, ClientPacket msg)
    {
      client.Towns.Clear();
      return true;
    }

    public bool ClientMessage_0x43_ClickCharacter(Client client, ClientPacket msg)
    {
      int num1 = (int) msg.ReadByte();
      if (num1 == 3)
      {
        int num2 = (int) msg.ReadUInt16();
        int num3 = (int) msg.ReadUInt16();
        int num4 = (int) msg.ReadByte();
      }
      if (num1 == 1)
      {
        uint key = msg.ReadUInt32();
        client.LastClickID = key;
        if (key == 1U)
        {
          client.previousfakepopup = 0;
          client.queststep = 1;
          client.StrajPopupText(1);
        }
        if (client.Tab.getrealnames.Checked && client.Characters.ContainsKey(key))
          client.DistanceLook((ushort) client.Characters[key].Location.X, (ushort) client.Characters[key].Location.Y);
      }
      if (client.Tab.vgetimage && client.Characters.ContainsKey(client.LastClickID) && client.Characters[client.LastClickID] != null && client.Characters[client.LastClickID] is Npc)
      {
        client.SendMessage("Image: " + (client.Characters[client.LastClickID] as Npc).Image.ToString() + " ID: " + client.LastClickID.ToString());
        client.Tab.newmonstername.Text = (client.Characters[client.LastClickID] as Npc).Image.ToString();
        foreach (Client client1 in Server.Clients)
        {
          if (client1.Tab != null && client1.LoggedOn)
            client1.Tab.drophaxnpcid.Value = (Decimal) client.LastClickID;
        }
        client.LastClickID = 0U;
      }
      return true;
    }

    public bool ClientMessage_0x44_UnequipGear(Client client, ClientPacket msg)
    {
      if (msg.ReadByte() == (byte) 2)
        client.manualremovedarmor = true;
      return true;
    }

    public bool ClientMessage_0x4D_SpellLines(Client client, ClientPacket msg)
    {
      client.ImCasting = true;
      byte Lines = msg.ReadByte();
      if (Lines != (byte) 1)
        return true;
      client.mancastdelay = DateTime.UtcNow;
      if (client.Tab.halfcast.Checked)
        client.StartCast((int) Lines);
      else
        new Thread((ThreadStart) (() => client.StartCast((int) Lines))).Start();
      return false;
    }

    public bool ServerMessage_0x03_Redirect(Client client, ServerPacket msg)
    {
      byte[] address = msg.Read(4);
      ushort port = msg.ReadUInt16();
      int num1 = (int) msg.ReadByte();
      int num2 = (int) msg.ReadByte();
      msg.Read((int) msg.ReadByte());
      string key = msg.ReadString((int) msg.ReadByte());
      int num3 = (int) msg.ReadUInt32();
      client.Name = key;
      client.Redirected = true;
      if (!Server.DAServer.ContainsKey(key))
        Server.DAServer.Add(key, (int) port);
      else
        Server.DAServer[key] = (int) port;
      Array.Reverse((Array) address);
      this.RemoteEndPoint = (EndPoint) new IPEndPoint(new IPAddress(address), (int) port);
      msg.BodyData[0] = (byte) 1;
      msg.BodyData[1] = (byte) 0;
      msg.BodyData[2] = (byte) 0;
      msg.BodyData[3] = (byte) 127;
      msg.BodyData[4] = (byte) 10;
      msg.BodyData[5] = (byte) 50;
      return true;
    }

    public bool ServerMessage_0x04_Location(Client client, ServerPacket msg)
    {
      client.ServerLocation.X = (int) msg.ReadUInt16();
      client.ServerLocation.Y = (int) msg.ReadUInt16();
      List<string> checkedtiles = client.checkedtiles;
      int num = client.ServerLocation.X;
      string str1 = num.ToString();
      num = client.ServerLocation.Y;
      string str2 = num.ToString();
      string str3 = str1 + "," + str2;
      checkedtiles.Add(str3);
      return true;
    }

    public bool ServerMessage_0x05_PlayerID(Client client, ServerPacket msg)
    {
      client.PlayerID = msg.ReadUInt32();
      msg.Read(1);
      msg.Read(1);
      client.myPath = msg.ReadByte();
      msg.Read(1);
      client.Gender = msg.ReadByte();
      msg.BodyData[6] = 2;

      // Set path stats using CharacterClass enum to identify class in maxClassStats dictionary
      CharacterClass activeClass = (CharacterClass)client.myPath;
      client.pathmaxhp = client.maxClassStats[activeClass].Maxhp;
      client.pathstr = client.maxClassStats[activeClass].Str;
      client.pathint = client.maxClassStats[activeClass].Int;
      client.pathwis = client.maxClassStats[activeClass].Wis;
      client.pathcon = client.maxClassStats[activeClass].Con;
      client.pathdex = client.maxClassStats[activeClass].Dex;

      client.GetHandle();
      client.BestAites();
      client.BestFases();
      client.BestIocs();
      client.BestDions();
      client.BestCradhs();
      client.BestPramhs();
      client.BestAttacks1();
      client.BestAttacks2();
      client.SpellsAppear();
      client.SkillsAppear();
      client.TrinketsAppear();
      client.Tab.PopulateLureList();
      client.LoadMacroList();
      client.MacroSpells();
      Program.MainForm.AddTab(client.Tab);
      if (!Server.Alts.ContainsKey(client.Name.ToLower()))
        Server.Alts.Add(client.Name.ToLower(), client);
      if (!Server.friendlist.Contains(client.Name.ToLower()))
      {
        Program.MainForm.friendlistbox.Items.Add((object) client.Name.ToLower());
        Program.MainForm.SaveFriends();
        Server.UpdateFriends();
      }
      foreach (Client client1 in Server.Alts.Values.ToArray<Client>())
      {
        if (client1 != null && client1.targetplayer != null)
        {
          foreach (targetPlayer targetPlayer in client1.targetplayer)
            targetPlayer?.updatePlayerTargets();
        }
      }
      client.Tab.LoadTemplates();
      if (Server.Relog.ContainsKey(client.Name))
      {
        client.Tab.LoadTemplate(AfterRelog: true);
        client.Relogged();
      }
      else
      {
        if (!System.IO.File.Exists(Program.StartupPath + "\\Settings\\" + client.Name.ToLower() + "\\default.xml"))
          client.Tab.SaveTemplate("default");
        else if (Program.MainForm.preload.Checked && Program.MainForm.preloadtemplate.Text != string.Empty)
          client.Tab.LoadTemplate(Program.MainForm.preloadtemplate.Text);
        else
          client.Tab.LoadTemplate("default");
        if (Program.MainForm.pregroup.Checked && Program.MainForm.pregroupname.Text != string.Empty)
          client.ForceGroup(Program.MainForm.pregroupname.Text, (byte) 3);
        if (Program.MainForm.preplay.Checked)
        {
          client.pause = false;
          client.Tab.btnPlay.Enabled = false;
          client.Tab.btnStop.Enabled = true;
        }
      }
      client.IniTimedStuff();
      foreach (Client client2 in Server.Alts.Values.ToArray<Client>())
      {
        if (client2 != null && client2.Name != client.Name && client2.Name != string.Empty && (client2.Tab.requestlabornametext.Text == string.Empty || client2.Tab.requestlabornametext.Text == client.Name))
        {
          client2.Tab.requestlabornametext.Text = client.Name;
          if (client2.waitingforlabor)
            client2.Whisper(client.Name, client2.Tab.requestlabormessagetext.Text);
        }
      }
      if (Program.MainForm.loglabormules.Checked && Program.MainForm.labormulelist.Items.Count > 0)
      {
        foreach (object obj in Program.MainForm.labormulelist.Items)
        {
          if (obj.ToString().ToLower().Contains(client.Name.ToLower()))
          {
            client.Tab.laborname.Text = Program.MainForm.laborname.Text;
            client.Tab.btnPlay.PerformClick();
            client.Tab.autowalker_locales.Text = "Nearest Bank";
            client.Tab.walksettings.Value = 160M;
            client.Tab.fastwalk.Checked = true;
            client.Tab.autowalker_button.Text = "Stop";
            client.autowalkon = true;
            break;
          }
        }
      }
      else if (Program.MainForm.getmentored.Checked && Program.MainForm.labormulelist.Items.Count > 0)
      {
        foreach (object obj in Program.MainForm.labormulelist.Items)
        {
          if (obj.ToString().ToLower().Contains(client.Name.ToLower()))
          {
            client.Tab.btnPlay.PerformClick();
            client.Tab.autowalker_locales.Text = "Rucesion";
            client.Tab.walklocaleslist.SelectedItem = (object) "Armor Shop";
            client.Tab.walksettings.Value = 240M;
            client.Tab.autowalker_button.Text = "Stop";
            client.autowalkon = true;
            break;
          }
        }
      }
      else if (Program.MainForm.logpigchase.Checked && Program.MainForm.labormulelist.Items.Count > 0)
      {
        foreach (object obj in Program.MainForm.labormulelist.Items)
        {
          if (obj.ToString().ToLower().Contains(client.Name.ToLower()))
          {
            client.Tab.btnPlay.PerformClick();
            client.Tab.autowalker_locales.Text = "Loures";
            client.Tab.walklocaleslist.SelectedItem = (object) "Maze";
            client.Tab.walksettings.Value = 250M;
            client.Tab.autowalker_button.Text = "Stop";
            client.autowalkon = true;
            client.Tab.pigwalk.Checked = true;
            break;
          }
        }
      }
      else if (Program.MainForm.frostylog.Checked && Program.MainForm.labormulelist.Items.Count > 0)
      {
        foreach (object obj in Program.MainForm.labormulelist.Items)
        {
          if (obj.ToString().ToLower().Contains(client.Name.ToLower()))
          {
            client.Tab.btnPlay.PerformClick();
            client.Tab.autowalker_locales.Text = "Loures";
            client.Tab.walklocaleslist.SelectedItem = (object) "Frosty (x-mas)";
            client.Tab.walksettings.Value = 250M;
            client.Tab.autowalker_button.Text = "Stop";
            client.autowalkon = true;
            client.frostygift = true;
            break;
          }
        }
      }
      client.RequestGroupList();
      client.LoadVariables();
      client.Tab.AscendOptions.statbuyupdate();
      client.Loaded = true;
      client.LoggedOn = true;

      if (client.Tab.pigwalk.Checked && client.HasItem("Ability and Experience Gift 1") && client.ItemAmount("Ability and Experience Gift 1") == 5U)
      {
        client.SendMessage("Stopped walking, you're at max stack of gift 1s", "red");
        client.pause = true;
        client.Tab.btnPlay.Enabled = true;
        client.Tab.btnStop.Enabled = false;
      }
      if (client.Tab.pigwalk.Checked && client.HasItem("Ability and Experience Gift 2") && client.ItemAmount("Ability and Experience Gift 2") == 5U)
      {
        client.SendMessage("Stopped walking, you're at max stack of gift 2s", "red");
        client.pause = true;
        client.Tab.btnPlay.Enabled = true;
        client.Tab.btnStop.Enabled = false;
      }
      return true;
    }

    public bool ServerMessage_0x07_DisplayNPC(Client client, ServerPacket msg)
    {
      int num1 = (int) msg.ReadUInt16();
      for (int index = 0; index < num1; ++index)
      {
        Npc npc1 = new Npc();
        npc1.Location.X = (int) msg.ReadUInt16();
        npc1.Location.Y = (int) msg.ReadUInt16();
        npc1.ID = msg.ReadUInt32();
        npc1.Image = (int) msg.ReadUInt16();
        npc1.Map = client.MapInfo.Number;
        npc1.MapName = client.MapInfo.Name;
        int num2;
        DateTime utcNow;
        TimeSpan timeSpan;
        if (npc1.Image < 32768)
        {
          msg.Read(4);
          byte num3 = msg.ReadByte();
          npc1.Location.Direction = (Direction) num3;
          int num4 = (int) msg.ReadByte();
          npc1.Type = (Npc.NpcType) msg.ReadByte();
          if (npc1.Type == Npc.NpcType.Mundane)
          {
            npc1.Name = msg.ReadString((int) msg.ReadByte());
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(desktopPath, "npcs.json");
            if (client.Tab.recordmaps.Checked && npc1.Name != "Fish" && File.Exists(filePath) && !Server.gamenpcs.ContainsKey(npc1.Name + npc1.Map.ToString()))
            {
              RootNpc rootNpc1 = new RootNpc();
              rootNpc1.name = npc1.Name;
              RootNpc rootNpc2 = rootNpc1;
              num2 = npc1.Location.X;
              string str1 = num2.ToString();
              rootNpc2.x = str1;
              RootNpc rootNpc3 = rootNpc1;
              num2 = npc1.Location.Y;
              string str2 = num2.ToString();
              rootNpc3.y = str2;
              if (num3 == (byte) 2)
                num3 = (byte) 1;
              rootNpc1.direction = num3.ToString();
              rootNpc1.mapnum = npc1.Map.ToString();
              rootNpc1.mapname = npc1.MapName;
              int num5 = npc1.Image - 16384;
              string str3 = num5.ToString();
              if (num5 < 10)
                str3 = "00" + num5.ToString();
              else if (num5 < 100)
                str3 = "0" + num5.ToString();
              rootNpc1.img = str3;
              rootNpc1.anidelay = "8";
              rootNpc1.colrect = "16,18,27,58";
              Server.gamenpcs.Add(npc1.Name + npc1.Map.ToString(), rootNpc1);
            }
          }
          if (!Server.StaticCharacters.ContainsKey(npc1.ID))
            Server.StaticCharacters.Add(npc1.ID, npc1);
          if (!client.Characters.ContainsKey(npc1.ID))
          {
            if (npc1.MapName.Equals("Lost Ruins 2") && npc1.Image - 16384 == 422)
            {
              foreach (Npc npc2 in ((IEnumerable<Npc>) client.NearbyMonstersByImage("422")).ToArray<Npc>())
              {
                if (npc2 != null && npc2.isParentGrime && npc2.DistanceFrom(npc1.Location) == 1)
                {
                  npc1.isGrimeSpawn = true;
                  break;
                }
              }
            }
            if (client.Tab.recorditemdata.Checked)
            {
              if (npc1.MapName.Contains("Astrid") && npc1.Image - 16384 == 50)
              {
                foreach (Npc npc3 in ((IEnumerable<Npc>) client.NearbyMonstersByImage("2")).ToArray<Npc>())
                {
                  if (npc3 != null && npc3.DistanceFrom(npc1.Location) == 1)
                  {
                    npc1.wassummoned = true;
                    break;
                  }
                }
              }
              if ((npc1.MapName.Contains("Shifting Swamp") || npc1.MapName.Contains("Chandi")) && (npc1.Image - 16384 == 85 || npc1.Image - 16384 == 88 || npc1.Image - 16384 == 89))
              {
                foreach (Npc npc4 in ((IEnumerable<Npc>) client.NearbyMonstersByImage("102")).ToArray<Npc>())
                {
                  if (npc4 != null && npc4.DistanceFrom(npc1.Location) == 1)
                  {
                    npc1.wassummoned = true;
                    break;
                  }
                }
              }
            }
            npc1.CreateTime = DateTime.UtcNow;
            npc1.InViewTime = DateTime.UtcNow;
            client.Characters.Add(npc1.ID, (Character) npc1);
          }
          else
          {
            client.Characters[npc1.ID].InViewTime = DateTime.UtcNow;
            client.Characters[npc1.ID].Map = npc1.Map;
            client.Characters[npc1.ID].Location.X = npc1.Location.X;
            client.Characters[npc1.ID].Location.Y = npc1.Location.Y;
            client.Characters[npc1.ID].Location.Direction = npc1.Location.Direction;
            client.Characters[npc1.ID].IsOnScreen = true;
          }
        }
        else
        {
          npc1.Color = msg.ReadByte();
          msg.Read(2);
          npc1.Type = Npc.NpcType.Item;
          npc1.SpawnLocation.X = npc1.Location.X;
          npc1.SpawnLocation.Y = npc1.Location.Y;
          if (!Server.StaticCharacters.ContainsKey(npc1.ID))
            Server.StaticCharacters.Add(npc1.ID, (Character) npc1);
          if (!client.Characters.ContainsKey(npc1.ID))
          {
            npc1.CreateTime = DateTime.UtcNow;
            npc1.InViewTime = DateTime.UtcNow;
            client.Characters.Add(npc1.ID, (Character) npc1);
            int num6;
            if (client.lastdroploc != null && npc1.Location.X == client.lastdroploc.X && npc1.Location.Y == client.lastdroploc.Y)
            {
              utcNow = DateTime.UtcNow;
              timeSpan = utcNow.Subtract(client.lastdroptime);
              num6 = timeSpan.TotalMilliseconds < 800.0 ? 1 : 0;
            }
            else
              num6 = 0;
            if (num6 != 0)
            {
              client.Characters[npc1.ID].Looted = true;
              client.lastdroploc = (Location) null;
              client.lastdroptime = DateTime.UtcNow;
            }
          }
          else
          {
            client.Characters[npc1.ID].InViewTime = DateTime.UtcNow;
            client.Characters[npc1.ID].Map = npc1.Map;
            client.Characters[npc1.ID].Location.X = npc1.Location.X;
            client.Characters[npc1.ID].Location.Y = npc1.Location.Y;
            client.Characters[npc1.ID].IsOnScreen = true;
          }
          if (client.Tab.getrealnames.Checked && npc1.Location.X == client.ClientLocation.X && npc1.Location.Y == client.ClientLocation.Y)
            client.DistanceLook((ushort) npc1.Location.X, (ushort) npc1.Location.Y);
        }
        npc1.Image -= 16384;
        int num7;
        if (npc1.Image == 156 && Program.MainForm.champalert.Checked)
        {
          if (!(Server.alarmTimer == DateTime.MinValue))
          {
            utcNow = DateTime.UtcNow;
            timeSpan = utcNow.Subtract(Server.alarmTimer);
            if (timeSpan.TotalSeconds <= 60.0)
              goto label_52;
          }
          if (!(client.Alertdelay == DateTime.MinValue))
          {
            utcNow = DateTime.UtcNow;
            timeSpan = utcNow.Subtract(client.Alertdelay);
            num7 = timeSpan.TotalSeconds > 60.0 ? 1 : 0;
            goto label_53;
          }
          else
          {
            num7 = 1;
            goto label_53;
          }
        }
label_52:
        num7 = 0;
label_53:
        if (num7 != 0)
        {
          client.SendMessage("Carnun Champion spotted!", "red");
          if (!Server.SentryAlarm)
          {
            Server.SentryAlarm = true;
            Server.alarm = new SoundPlayer(Assembly.GetExecutingAssembly().GetManifestResourceStream("Flintstones.chime.wav"));
            Server.alarmTimer = DateTime.UtcNow;
            Server.alarm.Play();
          }
        }
        int num8;
        if (npc1.Image == 3 && npc1.Location.X == client.ServerLocation.X && npc1.Location.Y == client.ServerLocation.Y)
        {
          utcNow = DateTime.UtcNow;
          timeSpan = utcNow.Subtract(npc1.CreateTime);
          num8 = timeSpan.TotalSeconds < 1.0 ? 1 : 0;
        }
        else
          num8 = 0;
        if (num8 != 0)
        {
          client.Disenchanter = npc1;
          client.disenchanterappears = true;
        }
        if (npc1.Image == 258)
        {
          Client client1 = client;
          num2 = npc1.Location.X;
          string str4 = num2.ToString();
          num2 = npc1.Location.Y;
          string str5 = num2.ToString();
          string text = "Golden Floppy at " + str4 + ", " + str5;
          client1.SendMessage(text, (byte) 11);
        }
        if (npc1.Image == 401 && client.MapInfo.Number == 8433)
        {
          Client client2 = client;
          num2 = npc1.Location.X;
          string str6 = num2.ToString();
          num2 = npc1.Location.Y;
          string str7 = num2.ToString();
          string text = "Chadul Creeper at " + str6 + ", " + str7;
          client2.SendMessage(text, (byte) 11);
        }
        if (npc1.Image == 400 && client.MapInfo.Number == 8436)
        {
          Client client3 = client;
          num2 = npc1.Location.X;
          string str8 = num2.ToString();
          num2 = npc1.Location.Y;
          string str9 = num2.ToString();
          string text = "Chadul Frieza at " + str8 + ", " + str9;
          client3.SendMessage(text, (byte) 11);
        }
        if (npc1.Image == 397 && client.MapInfo.Number == 8439)
        {
          Client client4 = client;
          num2 = npc1.Location.X;
          string str10 = num2.ToString();
          num2 = npc1.Location.Y;
          string str11 = num2.ToString();
          string text = "Chadul Gohma at " + str10 + ", " + str11;
          client4.SendMessage(text, (byte) 11);
        }
        if (npc1.Image == 707 && client.MapInfo.Number == 8441)
        {
          Client client5 = client;
          num2 = npc1.Location.X;
          string str12 = num2.ToString();
          num2 = npc1.Location.Y;
          string str13 = num2.ToString();
          string text = "Chadul Koopa at " + str12 + ", " + str13;
          client5.SendMessage(text, (byte) 11);
        }
        if (npc1.Image == 650 && client.MapInfo.Number == 8446)
        {
          Client client6 = client;
          num2 = npc1.Location.X;
          string str14 = num2.ToString();
          num2 = npc1.Location.Y;
          string str15 = num2.ToString();
          string text = "Chadul Predator at " + str14 + ", " + str15;
          client6.SendMessage(text, (byte) 11);
        }
        if (npc1.Image == 401 && client.MapInfo.Number == 8101)
        {
          Client client7 = client;
          num2 = npc1.Location.X;
          string str16 = num2.ToString();
          num2 = npc1.Location.Y;
          string str17 = num2.ToString();
          string text = "Crypt Mini at " + str16 + ", " + str17;
          client7.SendMessage(text, (byte) 11);
        }
        if (npc1.Image == 205 && client.MapInfo.Number == 8111)
        {
          Client client8 = client;
          num2 = npc1.Location.X;
          string str18 = num2.ToString();
          num2 = npc1.Location.Y;
          string str19 = num2.ToString();
          string text = "Crypt Mini at " + str18 + ", " + str19;
          client8.SendMessage(text, (byte) 11);
        }
        if (npc1.Image == 397 && client.MapInfo.Number == 8120)
        {
          Client client9 = client;
          num2 = npc1.Location.X;
          string str20 = num2.ToString();
          num2 = npc1.Location.Y;
          string str21 = num2.ToString();
          string text = "Shade of Ealagad at " + str20 + ", " + str21;
          client9.SendMessage(text, (byte) 11);
        }
        if (npc1.Image == 18165)
        {
          Client client10 = client;
          num2 = npc1.Location.X;
          string str22 = num2.ToString();
          num2 = npc1.Location.Y;
          string str23 = num2.ToString();
          string text = "Anklet at " + str22 + ", " + str23;
          client10.SendMessage(text, (byte) 11);
        }
        if (npc1.Image == 17862)
        {
          Client client11 = client;
          num2 = npc1.Location.X;
          string str24 = num2.ToString();
          num2 = npc1.Location.Y;
          string str25 = num2.ToString();
          string text = "Dochas Bloom at " + str24 + ", " + str25;
          client11.SendMessage(text, (byte) 11);
        }
        if (npc1.Image == 17863)
        {
          Client client12 = client;
          num2 = npc1.Location.X;
          string str26 = num2.ToString();
          num2 = npc1.Location.Y;
          string str27 = num2.ToString();
          string text = "Lily Pads at " + str26 + ", " + str27;
          client12.SendMessage(text, (byte) 11);
        }
        if (npc1.Image == 17874)
        {
          Client client13 = client;
          num2 = npc1.Location.X;
          string str28 = num2.ToString();
          num2 = npc1.Location.Y;
          string str29 = num2.ToString();
          string text = "Kobold Tail at " + str28 + ", " + str29;
          client13.SendMessage(text, (byte) 11);
        }
        if (npc1.Image == 17890)
        {
          Client client14 = client;
          num2 = npc1.Location.X;
          string str30 = num2.ToString();
          num2 = npc1.Location.Y;
          string str31 = num2.ToString();
          string text = "Cactus Flower at " + str30 + ", " + str31;
          client14.SendMessage(text, (byte) 11);
        }
      }
      return true;
    }

    public bool ServerMessage_0x08_Statistics(Client client, ServerPacket msg)
    {
      int num1 = (int) msg.ReadByte();
      uint num2;
      if ((num1 & 32) == 32)
      {
        msg.Read(3);
        client.Statistics.Level = (int) msg.ReadByte();
        client.Statistics.Ability = (int) msg.ReadByte();
        client.Statistics.MaximumHP = msg.ReadUInt32();
        client.Statistics.MaximumMP = msg.ReadUInt32();
        client.Statistics.Str = (int) msg.ReadByte();
        client.Statistics.Int = (int) msg.ReadByte();
        client.Statistics.Wis = (int) msg.ReadByte();
        client.Statistics.Con = (int) msg.ReadByte();
        client.Statistics.Dex = (int) msg.ReadByte();
        int num3 = (int) msg.ReadByte();
        client.Statistics.AvailablePoints = (int) msg.ReadByte();
        client.Statistics.MaximumWeight = (int) msg.ReadUInt16();
        client.Statistics.CurrentWeight = (int) msg.ReadUInt16();
        msg.Read(4);
        string text1 = client.Tab.AscendOptions.currentbasehp.Text;
        num2 = client.Statistics.MaximumHP;
        string str1 = num2.ToString("#,##0");
        if (text1 != str1)
        {
          Label currentbasehp = client.Tab.AscendOptions.currentbasehp;
          num2 = client.Statistics.MaximumHP;
          string str2 = num2.ToString("#,##0");
          currentbasehp.Text = str2;
        }
        string text2 = client.Tab.AscendOptions.currentbasemp.Text;
        num2 = client.Statistics.MaximumMP;
        string str3 = num2.ToString("#,##0");
        if (text2 != str3)
        {
          Label currentbasemp = client.Tab.AscendOptions.currentbasemp;
          num2 = client.Statistics.MaximumMP;
          string str4 = num2.ToString("#,##0");
          currentbasemp.Text = str4;
        }
        if (client.Loaded)
          new Thread((ThreadStart) (() => client.Tab.AscendOptions.statbuyupdate())).Start();
      }
      if ((num1 & 16) == 16)
      {
        client.Statistics.CurrentHP = msg.ReadUInt32();
        client.Statistics.CurrentMP = msg.ReadUInt32();
      }
      if ((num1 & 8) == 8)
      {
        uint gold = client.Statistics.Gold;
        client.Statistics.Experience = msg.ReadUInt32();
        client.Statistics.ToNextLevel = msg.ReadUInt32();
        client.Statistics.AbilityExp = msg.ReadUInt32();
        client.Statistics.ToNextAbility = msg.ReadUInt32();
        msg.Read(4);
        client.Statistics.Gold = msg.ReadUInt32();
        string text = client.Tab.AscendOptions.currentexpboxed.Text;
        num2 = client.Statistics.Experience;
        string str5 = num2.ToString("#,##0");
        if (text != str5)
        {
          Label currentexpboxed = client.Tab.AscendOptions.currentexpboxed;
          num2 = client.Statistics.Experience;
          string str6 = num2.ToString("#,##0");
          currentexpboxed.Text = str6;
        }
        if (client.Statistics.Gold > gold && (int) gold == (int) client.goldbefore && gold > 0U)
        {
          uint num4 = client.goldbefore - client.Statistics.Gold;
          client.SendMessage("Repair bill: " + num4.ToString() + " coins.", "grey");
        }
        if (client.Statistics.Gold > gold && client.LastVanishedGold > 0U && client.Characters.ContainsKey(client.LastVanishedGold) && DateTime.UtcNow.Subtract(client.Characters[client.LastVanishedGold].DeathTime).TotalMilliseconds < 100.0)
          client.Characters[client.LastVanishedGold].GoldAmount = client.Statistics.Gold - gold;
      }
      if ((num1 & 4) == 4)
      {
        client.Statistics.BitMask = (int) msg.ReadUInt16();
        int num5 = (int) msg.ReadByte();
        client.Statistics.AttackElement2 = (int) msg.ReadByte();
        client.Statistics.DefenseElement2 = (int) msg.ReadByte();
        client.Statistics.MailAndParcel = msg.ReadByte();
        client.Statistics.AttackElement = (Statistics.Elements) msg.ReadByte();
        client.Statistics.DefenseElement = (Statistics.Elements) msg.ReadByte();
        client.Statistics.MagicResistance = (int) msg.ReadByte();
        int num6 = (int) msg.ReadByte();
        client.Statistics.ArmorClass = (int) msg.ReadSByte();
        client.Statistics.Damage = (int) msg.ReadByte();
        client.Statistics.Hit = (int) msg.ReadByte();
      }
      if (client.outofcowls && client.Statistics.CurrentWeight >= client.Statistics.MaximumWeight)
      {
        client.buying = false;
        client.outofcowls = false;
        client.brody = false;
      }
      int num7 = 0;
      if (client.Statistics.MailAndParcel > (byte) 0)
        num7 = client.Statistics.MailAndParcel < (byte) 16 ? (int) client.Statistics.MailAndParcel : (int) client.Statistics.MailAndParcel % 16;
      if (num7 > 0)
        client.SendMessage("You have Gifts!", (byte) 18);
      else if (client.LastPermMessage == "You have Gifts!")
      {
        client.SendMessage("", (byte) 18);
        client.LastPermMessage = string.Empty;
      }
      return true;
    }

    public bool ServerMessage_0x0A_SystemMessage(Client client, ServerPacket msg)
    {
      byte num1 = msg.ReadByte();
      string str1 = msg.ReadString16();
      if (num1 == (byte) 8 && client.Tab.studycreaturetxt.Checked && str1.Contains("Sense Monster") && str1.Contains("DEFENSE NATURE"))
      {
        string[] strArray = str1.Split(Environment.NewLine.ToCharArray());
        string str2 = strArray[2].Substring(strArray[2].IndexOf("Name:") + 5);
        string str3 = str2.Remove(str2.IndexOf("EXP:") - 4).TrimStart(' ');
        string str4 = strArray[2].Substring(strArray[2].IndexOf("EXP:") + 4).Trim();
        string s = strArray[3].Substring(strArray[3].IndexOf("HP:") + 3).Trim();
        string str5 = strArray[4].Substring(strArray[4].IndexOf("Lev:") + 4).Trim();
        string str6 = strArray[5].Substring(strArray[5].IndexOf("ATTACK NATURE:") + 14).Trim();
        string str7 = strArray[6].Substring(strArray[6].IndexOf("DEFENSE NATURE:") + 15).Trim(); ;
        string str8 = client.MapInfo.Number.ToString();
        string str9 = client.MapInfo.Name;
        if (str9.StartsWith("Chadul"))
          str9 = str9.Replace("'", "");
        string str10 = "0";
        if (client.MonsterInFront() != null)
        {
          str10 = client.MonsterInFront().Image.ToString();
          client.Characters[client.MonsterInFront().ID].sensed = true;
        }
        else
        {
          bool flag = false;
          foreach (Npc nearbyNormalMonster in client.NearbyNormalMonsters())
          {
            if (nearbyNormalMonster != null && nearbyNormalMonster.IsInRSRange(client.ServerLocation, 2))
            {
              str10 = nearbyNormalMonster.Image.ToString();
              client.Characters[nearbyNormalMonster.ID].sensed = true;
              flag = true;
              break;
            }
          }
          if (!flag)
          {
            foreach (Npc nearbyNormalMonster in client.NearbyNormalMonsters())
            {
              if (nearbyNormalMonster != null && nearbyNormalMonster.IsInRSRange(client.ServerLocation, 3))
              {
                str10 = nearbyNormalMonster.Image.ToString();
                client.Characters[nearbyNormalMonster.ID].sensed = true;
                break;
              }
            }
          }
        }
        if (str10 != "0")
        {
          SenseMonster senseMonster = new SenseMonster()
          {
            Name = str3,
            Image = str10,
            Exp = str4,
            HP = s,
            Lev = str5,
            Attack = str6,
            Defense = str7,
            MapNumber = str8,
            MapName = str9
          };
          XmlDocument xmlDocument = new XmlDocument();
          string str11 = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\DaItemDB\\sensed.xml";
          if (!System.IO.File.Exists(str11))
          {
            XmlNode element = (XmlNode) xmlDocument.CreateElement("maps");
            xmlDocument.AppendChild(element);
            xmlDocument.Save(str11);
          }
          if (System.IO.File.Exists(str11))
          {
            xmlDocument.Load(str11);
            XmlNode documentElement = (XmlNode) xmlDocument.DocumentElement;
            XmlNode xmlNode1 = xmlDocument.SelectSingleNode("/maps/map[@name='" + str8 + "_" + str9 + "']");
            if (xmlNode1 == null)
            {
              XmlNode element1 = (XmlNode) xmlDocument.CreateElement("map");
              documentElement.AppendChild(element1);
              XmlAttribute attribute1 = xmlDocument.CreateAttribute("name");
              attribute1.Value = str8 + "_" + str9;
              element1.Attributes.Append(attribute1);
              XmlNode element2 = (XmlNode) xmlDocument.CreateElement("monster");
              element1.AppendChild(element2);
              XmlAttribute attribute2 = xmlDocument.CreateAttribute("id");
              attribute2.Value = str10 + str3 + str4 + str5 + str8;
              element2.Attributes.Append(attribute2);
              XmlNode element3 = (XmlNode) xmlDocument.CreateElement("name");
              element3.InnerText = str3;
              element2.AppendChild(element3);
              XmlNode element4 = (XmlNode) xmlDocument.CreateElement("image");
              element4.InnerText = str10;
              element2.AppendChild(element4);
              XmlNode element5 = (XmlNode) xmlDocument.CreateElement("exp");
              element5.InnerText = str4;
              element2.AppendChild(element5);
              XmlNode element6 = (XmlNode) xmlDocument.CreateElement("minhp");
              element6.InnerText = s;
              element2.AppendChild(element6);
              XmlNode element7 = (XmlNode) xmlDocument.CreateElement("maxhp");
              element7.InnerText = s;
              element2.AppendChild(element7);
              XmlNode element8 = (XmlNode) xmlDocument.CreateElement("lev");
              element8.InnerText = str5;
              element2.AppendChild(element8);
              XmlNode element9 = (XmlNode) xmlDocument.CreateElement("attack");
              element9.InnerText = str6;
              element2.AppendChild(element9);
              XmlNode element10 = (XmlNode) xmlDocument.CreateElement("defense");
              element10.InnerText = str7;
              element2.AppendChild(element10);
              XmlNode element11 = (XmlNode) xmlDocument.CreateElement("mapnum");
              element11.InnerText = str8;
              element2.AppendChild(element11);
              XmlNode element12 = (XmlNode) xmlDocument.CreateElement("mapname");
              element12.InnerText = str9;
              element2.AppendChild(element12);
            }
            else if (xmlDocument.SelectSingleNode("/maps/map[@name='" + str8 + "_" + str9 + "']/monster[@id='" + str10 + str3 + str4 + str5 + str8 + "']") == null)
            {
              XmlNode element13 = (XmlNode) xmlDocument.CreateElement("monster");
              xmlNode1.AppendChild(element13);
              XmlAttribute attribute = xmlDocument.CreateAttribute("id");
              attribute.Value = str10 + str3 + str4 + str5 + str8;
              element13.Attributes.Append(attribute);
              XmlNode element14 = (XmlNode) xmlDocument.CreateElement("name");
              element14.InnerText = str3;
              element13.AppendChild(element14);
              XmlNode element15 = (XmlNode) xmlDocument.CreateElement("image");
              element15.InnerText = str10;
              element13.AppendChild(element15);
              XmlNode element16 = (XmlNode) xmlDocument.CreateElement("exp");
              element16.InnerText = str4;
              element13.AppendChild(element16);
              XmlNode element17 = (XmlNode) xmlDocument.CreateElement("minhp");
              element17.InnerText = s;
              element13.AppendChild(element17);
              XmlNode element18 = (XmlNode) xmlDocument.CreateElement("maxhp");
              element18.InnerText = s;
              element13.AppendChild(element18);
              XmlNode element19 = (XmlNode) xmlDocument.CreateElement("lev");
              element19.InnerText = str5;
              element13.AppendChild(element19);
              XmlNode element20 = (XmlNode) xmlDocument.CreateElement("attack");
              element20.InnerText = str6;
              element13.AppendChild(element20);
              XmlNode element21 = (XmlNode) xmlDocument.CreateElement("defense");
              element21.InnerText = str7;
              element13.AppendChild(element21);
              XmlNode element22 = (XmlNode) xmlDocument.CreateElement("mapnum");
              element22.InnerText = str8;
              element13.AppendChild(element22);
              XmlNode element23 = (XmlNode) xmlDocument.CreateElement("mapname");
              element23.InnerText = str9;
              element13.AppendChild(element23);
            }
            else
            {
              XmlNode xmlNode2 = xmlDocument.SelectSingleNode("/maps/map[@name='" + str8 + "_" + str9 + "']/monster[@id='" + str10 + str3 + str4 + str5 + str8 + "']/minhp");
              XmlNode xmlNode3 = xmlDocument.SelectSingleNode("/maps/map[@name='" + str8 + "_" + str9 + "']/monster[@id='" + str10 + str3 + str4 + str5 + str8 + "']/maxhp");
              if (xmlNode2 != null)
              {
                int num2 = int.Parse(xmlNode2.InnerText);
                int num3 = int.Parse(s);
                if (num3 < num2 && num3 < int.Parse(xmlNode3.InnerText) / 2)
                  client.SendMessage("WARNING! New Min HP: " + s + ", Old: " + xmlNode2.InnerText + ", of MAX: " + xmlNode3.InnerText);
                xmlNode2.InnerText = s;
              }
              if (xmlNode3 != null)
              {
                int num4 = int.Parse(xmlNode3.InnerText);
                if (int.Parse(s) > num4)
                  xmlNode3.InnerText = s;
              }
              XmlNode xmlNode4 = xmlDocument.SelectSingleNode("/maps/map[@name='" + str8 + "_" + str9 + "']/monster[@id='" + str10 + str3 + str4 + str5 + str8 + "']/attack");
              if (xmlNode4 != null && !xmlNode4.InnerText.Contains(str6))
              {
                XmlNode xmlNode5 = xmlNode4;
                xmlNode5.InnerText = xmlNode5.InnerText + "," + str6;
              }
              XmlNode xmlNode6 = xmlDocument.SelectSingleNode("/maps/map[@name='" + str8 + "_" + str9 + "']/monster[@id='" + str10 + str3 + str4 + str5 + str8 + "']/defense");
              if (xmlNode6 != null && !xmlNode6.InnerText.Contains(str7))
              {
                XmlNode xmlNode7 = xmlNode6;
                xmlNode7.InnerText = xmlNode7.InnerText + "," + str7;
              }
            }
            xmlDocument.Save(str11);
          }
        }
      }

      if (Server.ignoreaislinglist.Count<string>() > 0)
      {
        foreach (string str12 in Server.ignoreaislinglist)
        {
          if (str1.ToLower().Contains(str12))
            return false;
        }
      }
      if (!client.pause && str1.Equals("You are stuck."))
      {
        if (client.MapInfo.Number == 10056 || client.MapInfo.Number == 10004 || client.MapInfo.Number == 1960)
          client.MoveOver();
        else
          client.MapInfo.Name.StartsWith("Training Dojo");
      }
      if (Program.MainForm.collegealert.Checked && str1.Contains(" will be teaching "))
      {
        client.SendMessage(str1, "orange");
        if (!Server.SentryAlarm)
        {
          Server.SentryAlarm = true;
          Server.alarm = new SoundPlayer(Assembly.GetExecutingAssembly().GetManifestResourceStream("Flintstones.chime.wav"));
          Server.alarmTimer = DateTime.UtcNow;
          Server.alarm.Play();
        }
      }
      if (num1 == (byte) 8 && client.Tab.recorditemdata.Checked)
      {
        string str13 = str1.Substring(0, str1.IndexOf("\n"));
        string str14 = string.Empty;
        foreach (Item obj in client.Inventory)
        {
          if (obj != null && obj.InventorySlot == 1 && obj.Name == str13)
          {
            obj.IsIdentified = true;
            str14 = obj.Name;
            break;
          }
        }
        if (str14 != string.Empty)
        {
          foreach (Character character in client.Characters.Values.ToArray<Character>())
          {
            if (character != null && character is Npc && (character as Npc).Type == Npc.NpcType.Item && character.InventorySlot == 1 && (character.Name != str14 || str13 == character.Name))
            {
              character.IsIdentified = true;
              character.Name = str14;
              client.SendMessage("Just got ID'd: " + character.Name);
              break;
            }
          }
        }
      }
      if (num1 == (byte) 8 && (client.Tab.iditems.Checked || client.Tab.recorditemdata.Checked) && !client.pause || num1 == (byte) 8 && client.Tab.MacroOptions.macroskill.Checked && !client.pause)
        return false;
      if (num1 == (byte) 8 && client.blocklores)
      {
        client.blocklores = false;
        return false;
      }
      if (str1.StartsWith("If you do not talk to the queen"))
        client.SendMessage(str1);
      if (str1.StartsWith("Chaos is rising"))
        client.serverreset = true;
      if (client.disablelegend && (str1.StartsWith("zz is nowhere") || str1.StartsWith("You have disabled your whisper")))
      {
        client.SendMessage("Search " + client.Tab.DAid1.Text + " to " + client.Tab.DAid2.Text + " ended");
        client.disablelegend = false;
        client.Tab.button4.Enabled = true;
        return false;
      }
      if (str1.StartsWith("Your expiration date is ") || str1.StartsWith("Your account is on auto"))
        Server.DARegged[client.Name] = true;
      if (str1.StartsWith("The durability of ") && str1.Contains("is now"))
      {
        client.needsrepaired = true;
        if (str1.Contains("50%"))
          client.SendMessage(client.Name + " has a 50% durability item!", "red", true);
        if (str1.Contains("30%"))
          client.SendMessage(client.Name + " has a 30% durability item!", "red", true);
        if (str1.Contains("10%"))
          client.SendMessage(client.Name + " has a 10% durability item!", "red", true);
        if (str1.Contains(" 0%"))
          client.SendMessage("gj, you just broke something", "red", true);
      }
      if (client.blockchat && (str1.Equals("You can't send messages in this area.") || str1.Contains("is nowhere to be found") || str1.Contains("can't hear you")))
      {
        client.blockchat = false;
        return false;
      }
      if (client.blockchat && str1.Contains("is reading a board"))
        return false;
      if (Server.cID != "" && str1.Contains(" minutes have passed since you last logged in."))
      {
        DateTime now = DateTime.Now;
        if (!Directory.Exists(Application.StartupPath + "\\Settings\\" + client.Name.ToLower()))
          Directory.CreateDirectory(Application.StartupPath + "\\Settings\\" + client.Name.ToLower());
        if (Server.DARegged[client.Name])
        {
          if (!System.IO.File.Exists(Application.StartupPath + "\\Settings\\" + client.Name.ToLower() + "\\m.txt"))
          {
            StreamWriter streamWriter = new StreamWriter(Application.StartupPath + "\\Settings\\" + client.Name.ToLower() + "\\m.txt", true);
            streamWriter.WriteLine(Server.Stuff[client.Name]);
            streamWriter.Close();
          }
          else
          {
            bool flag = false;
            StreamReader streamReader = new StreamReader(Application.StartupPath + "\\Settings\\" + client.Name.ToLower() + "\\m.txt");
            while (streamReader.Peek() >= 0)
            {
              if (streamReader.ReadLine() == Server.Stuff[client.Name])
                flag = true;
            }
            streamReader.Close();
            if (!flag)
            {
              StreamWriter streamWriter = new StreamWriter(Application.StartupPath + "\\Settings\\" + client.Name.ToLower() + "\\m.txt", true);
              streamWriter.WriteLine(Server.Stuff[client.Name]);
              streamWriter.Close();
            }
          }
        }
      }
      if (str1.Contains(" has fallen in battle to ") || str1.Contains(" has been killed"))
      {
        string str15 = str1.Substring(0, str1.IndexOf(" "));
        string empty = string.Empty;
        if (client.countarena)
        {
          if (!(empty != string.Empty) || empty != str15)
          {
            if (!client.ArenaCounter.ContainsKey(str15))
            {
              client.ArenaCounter.Add(str15, new Arena()
              {
                Name = str15,
                Deaths = 1U
              });
              if (!client.Tab.ArenaCounter.arenacounterlist.Items.ContainsKey(str15))
              {
                client.Tab.ArenaCounter.arenacounterlist.BeginUpdate();
                ListViewItem listViewItem = client.Tab.ArenaCounter.arenacounterlist.Items.Add(str15, str15, -1);
                listViewItem.SubItems.Add("0");
                listViewItem.SubItems.Add("1");
                client.Tab.ArenaCounter.arenacounterlist.EndUpdate();
              }
              else
              {
                client.Tab.ArenaCounter.arenacounterlist.BeginUpdate();
                client.Tab.ArenaCounter.arenacounterlist.Items[str15].SubItems[2].Text = "1";
                client.Tab.ArenaCounter.arenacounterlist.EndUpdate();
              }
            }
            else
            {
              ++client.ArenaCounter[str15].Deaths;
              if (client.Tab.ArenaCounter.arenacounterlist.Items.ContainsKey(str15))
              {
                client.Tab.ArenaCounter.arenacounterlist.BeginUpdate();
                client.Tab.ArenaCounter.arenacounterlist.Items[str15].SubItems[2].Text = client.ArenaCounter[str15].Deaths.ToString();
                client.Tab.ArenaCounter.arenacounterlist.EndUpdate();
              }
            }
          }
          if (str1.Contains(" has fallen in battle to "))
          {
            string str16 = str1.Substring(str1.LastIndexOf(" ") + 1, str1.IndexOf(".") - str1.LastIndexOf(" ") - 1);
            if (str16 != str15)
            {
              if (!client.ArenaCounter.ContainsKey(str16))
              {
                client.ArenaCounter.Add(str16, new Arena()
                {
                  Name = str16,
                  Kills = 1U
                });
                if (!client.Tab.ArenaCounter.arenacounterlist.Items.ContainsKey(str16))
                {
                  client.Tab.ArenaCounter.arenacounterlist.BeginUpdate();
                  ListViewItem listViewItem = client.Tab.ArenaCounter.arenacounterlist.Items.Add(str16, str16, -1);
                  listViewItem.SubItems.Add("1");
                  listViewItem.SubItems.Add("0");
                  client.Tab.ArenaCounter.arenacounterlist.EndUpdate();
                }
                else
                {
                  client.Tab.ArenaCounter.arenacounterlist.BeginUpdate();
                  client.Tab.ArenaCounter.arenacounterlist.Items[str16].SubItems[1].Text = "1";
                  client.Tab.ArenaCounter.arenacounterlist.EndUpdate();
                }
              }
              else
              {
                ++client.ArenaCounter[str16].Kills;
                if (client.Tab.ArenaCounter.arenacounterlist.Items.ContainsKey(str16))
                {
                  client.Tab.ArenaCounter.arenacounterlist.BeginUpdate();
                  client.Tab.ArenaCounter.arenacounterlist.Items[str16].SubItems[1].Text = client.ArenaCounter[str16].Kills.ToString();
                  client.Tab.ArenaCounter.arenacounterlist.EndUpdate();
                }
              }
            }
          }
        }
        try
        {
          lock (Server.StaticCharacters)
          {
            foreach (Character character in Server.StaticCharacters.Values.ToArray<Character>())
            {
              if (character != null && character.Name == str15)
                character.SpellAnimationHistory.Clear();
            }
          }
        }
        catch
        {
        }
      }

      if (num1 == (byte) 3)
      {
        if (str1.Length >= 128)
          return false;
        TimeSpan timeSpan;
        if (client.Tab.recorditemdata.Checked && (str1.Contains(" experience!") || str1.StartsWith("No more experience") || str1.StartsWith("You have reached level 99,")) && client.LastDeadNpc != 0U && client.Characters[client.LastDeadNpc].Name != string.Empty)
        {
          timeSpan = DateTime.UtcNow.Subtract(client.Characters[client.LastDeadNpc].DeathTime);
          if (timeSpan.TotalMilliseconds < 800.0)
          {
            string mapkey = client.Characters[client.LastDeadNpc].Map.ToString() + "_" + client.Characters[client.LastDeadNpc].MapName;
            string key = (client.Characters[client.LastDeadNpc] as Npc).Image.ToString() + "_" + client.Characters[client.LastDeadNpc].Name;
            if (Server.ItemMapDatabase.ContainsKey(mapkey))
            {
              if (Server.ItemMapDatabase[mapkey].Monsters.ContainsKey(key))
                ++Server.ItemMapDatabase[mapkey].Monsters[key].KillCount;
              else
                Server.ItemMapDatabase[mapkey].Monsters.Add(key, new ItemMonsterXML()
                {
                  Name = client.Characters[client.LastDeadNpc].Name,
                  Image = (client.Characters[client.LastDeadNpc] as Npc).Image,
                  KillCount = 1U
                });
              client.Characters[client.LastDeadNpc].CountedItsKill = true;
              Program.MainForm.BeginInvoke((Action) (() => Program.MainForm.ItemXMLEditor.UpdateMapForm(Server.ItemMapDatabase[mapkey], client.Name)));
            }
            else
            {
              Server.ItemMapDatabase.Add(mapkey, new ItemMapXML()
              {
                Name = client.Characters[client.LastDeadNpc].MapName,
                Number = client.Characters[client.LastDeadNpc].Map
              });
              Server.ItemMapDatabase[mapkey].Monsters.Add(key, new ItemMonsterXML()
              {
                Name = client.Characters[client.LastDeadNpc].Name,
                Image = (client.Characters[client.LastDeadNpc] as Npc).Image,
                KillCount = 1U
              });
              client.Characters[client.LastDeadNpc].CountedItsKill = true;
              Program.MainForm.BeginInvoke((Action) (() => Program.MainForm.ItemXMLEditor.UpdateMapForm(Server.ItemMapDatabase[mapkey], client.Name)));
            }
          }
        }
        int num5;
        if (Program.MainForm.recordchestdata.Checked && str1.StartsWith("You received") && str1.Contains("gold"))
        {
          string str17 = str1.Remove(0, str1.IndexOf(' ', 6) + 1);
          string key = str17.Remove(str17.IndexOf(" gold"));
          if (client.wdchestopen)
          {
            if (Server.ChestDatabase.ContainsKey("Water Dungeon Chest Gold"))
            {
              ++Server.ChestDatabase["Water Dungeon Chest Gold"].OpenedCount;
              if (Server.ChestDatabase["Water Dungeon Chest Gold"].Treasure.ContainsKey(key))
                num5 = Server.ChestDatabase["Water Dungeon Chest Gold"].Treasure[key]++;
              else
                Server.ChestDatabase["Water Dungeon Chest Gold"].Treasure.Add(key, 1);
            }
            client.wdchestopen = false;
            Program.MainForm.BeginInvoke((Action) (() => Program.MainForm.ItemXMLEditor.UpdateChestsForm()));
          }
          else if (client.andorchestopen)
          {
            if (Server.ChestDatabase.ContainsKey("Andor Chest Gold"))
            {
              ++Server.ChestDatabase["Andor Chest Gold"].OpenedCount;
              if (Server.ChestDatabase["Andor Chest Gold"].Treasure.ContainsKey(key))
                num5 = Server.ChestDatabase["Andor Chest Gold"].Treasure[key]++;
              else
                Server.ChestDatabase["Andor Chest Gold"].Treasure.Add(key, 1);
            }
            client.andorchestopen = false;
            Program.MainForm.BeginInvoke((Action) (() => Program.MainForm.ItemXMLEditor.UpdateChestsForm()));
          }
          else if (client.queenchestopen)
          {
            if (Server.ChestDatabase.ContainsKey("Queen's Chest Gold"))
            {
              ++Server.ChestDatabase["Queen's Chest Gold"].OpenedCount;
              if (Server.ChestDatabase["Queen's Chest Gold"].Treasure.ContainsKey(key))
                num5 = Server.ChestDatabase["Queen's Chest Gold"].Treasure[key]++;
              else
                Server.ChestDatabase["Queen's Chest Gold"].Treasure.Add(key, 1);
            }
            client.queenchestopen = false;
            Program.MainForm.BeginInvoke((Action) (() => Program.MainForm.ItemXMLEditor.UpdateChestsForm()));
          }
          else if (client.veltainchestopen)
          {
            if (Server.ChestDatabase.ContainsKey("Veltain Chest " + client.chestfee))
            {
              ++Server.ChestDatabase["Veltain Chest " + client.chestfee].OpenedCount;
              if (Server.ChestDatabase["Veltain Chest " + client.chestfee].Treasure.ContainsKey(key))
                num5 = Server.ChestDatabase["Veltain Chest " + client.chestfee].Treasure[key]++;
              else
                Server.ChestDatabase["Veltain Chest " + client.chestfee].Treasure.Add(key, 1);
            }
            else
              Server.ChestDatabase.Add("Veltain Chest " + client.chestfee, new ChestItemXML("Veltain Chest " + client.chestfee, 1U)
              {
                Treasure = {
                  {
                    key,
                    1
                  }
                }
              });
            client.veltainchestopen = false;
            Program.MainForm.BeginInvoke((Action) (() => Program.MainForm.ItemXMLEditor.UpdateChestsForm()));
          }
          else if (client.heavychestopen)
          {
            if (Server.ChestDatabase.ContainsKey("Heavy Veltain Chest " + client.chestfee))
            {
              ++Server.ChestDatabase["Heavy Veltain Chest " + client.chestfee].OpenedCount;
              if (Server.ChestDatabase["Heavy Veltain Chest " + client.chestfee].Treasure.ContainsKey(key))
                num5 = Server.ChestDatabase["Heavy Veltain Chest " + client.chestfee].Treasure[key]++;
              else
                Server.ChestDatabase["Heavy Veltain Chest " + client.chestfee].Treasure.Add(key, 1);
            }
            else
              Server.ChestDatabase.Add("Heavy Veltain Chest " + client.chestfee, new ChestItemXML("Heavy Veltain Chest " + client.chestfee, 1U)
              {
                Treasure = {
                  {
                    key,
                    1
                  }
                }
              });
            client.heavychestopen = false;
            Program.MainForm.BeginInvoke((Action) (() => Program.MainForm.ItemXMLEditor.UpdateChestsForm()));
          }
          else if (client.smallbagopen)
          {
            if (Server.ChestDatabase.ContainsKey("Canal Bag"))
            {
              ++Server.ChestDatabase["Canal Bag"].OpenedCount;
              if (Server.ChestDatabase["Canal Bag"].Treasure.ContainsKey(key))
                num5 = Server.ChestDatabase["Canal Bag"].Treasure[key]++;
              else
                Server.ChestDatabase["Canal Bag"].Treasure.Add(key, 1);
            }
            client.smallbagopen = false;
            Program.MainForm.BeginInvoke((Action) (() => Program.MainForm.ItemXMLEditor.UpdateChestsForm()));
          }
          else if (client.bigbagopen)
          {
            if (Server.ChestDatabase.ContainsKey("Big Canal Bag"))
            {
              ++Server.ChestDatabase["Big Canal Bag"].OpenedCount;
              if (Server.ChestDatabase["Big Canal Bag"].Treasure.ContainsKey(key))
                num5 = Server.ChestDatabase["Big Canal Bag"].Treasure[key]++;
              else
                Server.ChestDatabase["Big Canal Bag"].Treasure.Add(key, 1);
            }
            client.bigbagopen = false;
            Program.MainForm.BeginInvoke((Action) (() => Program.MainForm.ItemXMLEditor.UpdateChestsForm()));
          }
          else if (client.heavybagopen)
          {
            if (Server.ChestDatabase.ContainsKey("Heavy Canal Bag"))
            {
              ++Server.ChestDatabase["Heavy Canal Bag"].OpenedCount;
              if (Server.ChestDatabase["Heavy Canal Bag"].Treasure.ContainsKey(key))
                num5 = Server.ChestDatabase["Heavy Canal Bag"].Treasure[key]++;
              else
                Server.ChestDatabase["Heavy Canal Bag"].Treasure.Add(key, 1);
            }
            client.heavybagopen = false;
            Program.MainForm.BeginInvoke((Action) (() => Program.MainForm.ItemXMLEditor.UpdateChestsForm()));
          }
        }

        // monster kill counting for daily quests
        if ((str1.Contains(" experience!") || str1.StartsWith("No more experience") || str1.StartsWith("You have reached level 99,")) && client.LastDeadMonster != 0U)
        {
          timeSpan = DateTime.UtcNow.Subtract(client.Characters[client.LastDeadMonster].DeathTime);
          if (timeSpan.TotalMilliseconds < 800.0)
          {
            uint lastDeadMonster = client.LastDeadMonster;
            int image = (client.Characters[lastDeadMonster] as Npc).Image;


            // Code by Avi
            // TODO: Find mapIDs for Yowien Baboon reset.
            // TODO: Find mapIDs for Baby Brute reset.
            // TODO: Find mapIDs for Yowien Dendron reset.
            // TODO: Find mapIDs for Water Dungeon resets.
            if (client.monsterKills.Count(image))
            {
              client.SendMessage(client.monsterKills.Display(client.MapInfo.Name), 18);
            }
            //

          }
        }
        if (str1.Contains(" experience!") && client.MapInfo.Name.Contains("Mount Merry 5-"))
        {
          foreach (string groupMember in client.GroupMembers)
          {
            if (groupMember != string.Empty)
            {
              GroupCounts groupCounts = new GroupCounts();
              groupCounts.Name = groupMember.ToLower();
              ++groupCounts.FilthyErbieCount;
              if (client.GroupCounter.ContainsKey(groupMember.ToLower()))
              {
                ++client.GroupCounter[groupMember.ToLower()].FilthyErbieCount;
                if (client.GroupCounter[groupMember.ToLower()].FilthyErbieCount == 20U)
                {
                  client.SendMessage(groupMember + " has killed 20 filthy erbies.", "grey");
                  client.GroupCounter[groupMember.ToLower()].FilthyErbieCount = 0U;
                }
              }
              else
                client.GroupCounter.Add(groupMember.ToLower(), groupCounts);
            }
          }
        }
        if (str1.StartsWith("Aha! Tell the mundane monk"))
        {
          client.meditate = false;
          client.meditatedone = true;
          client.mblue = false;
          client.mblack = false;
          client.mbrown = false;
          client.mgreen = false;
          client.myellow = false;
          client.mpurple = false;
          client.mred = false;
          client.mwhite = false;
        }
        if (str1.StartsWith("Path for the sword,") || str1.StartsWith("Path of conquest,") || str1.StartsWith("If you want to become") || str1.StartsWith("Monks? Consider well") || str1.StartsWith("Rogue's path begins") || str1.StartsWith("Wizard, go right to") || str1.StartsWith("To befriend Nature,"))
          client.tocpopup = true;
        if (str1.Equals("You didn't receive a Stolen Bag") || str1.Equals("You received a Stolen Bag"))
          client.SaveTimedStuff(40);
        if (str1.StartsWith("You feel a touch of light") || str1.Contains(", speaks to you"))
        {
          client.altartimer = DateTime.Now;
          client.SaveTimedStuff(36);
        }
        if (str1.Equals("Nothing here for you, I'm afraid.") || str1.Equals("Your inventory is full.") || str1.StartsWith("Whoa there, you don't have enough room "))
          client.hasparcels = false;
        if (client.atemeg && str1.StartsWith("You receive "))
        {
          client.atemeg = false;
          client.SaveTimedStuff(30);
        }
        if (client.ateabgift && str1.StartsWith("You receive "))
        {
          client.ateabgift = false;
          client.SaveTimedStuff(28);
        }
        if (client.ateabbox && str1.StartsWith("You receive "))
        {
          client.ateabbox = false;
          client.SaveTimedStuff(29);
        }
        if (client.ateclover && str1.StartsWith("You receive "))
        {
          client.ateclover = false;
          client.SaveTimedStuff(13);
        }
        else if (client.ateclover && str1.StartsWith("((Please wait 5 days"))
          client.ateclover = false;
        if (client.ategsf && str1.StartsWith("You receive "))
        {
          client.ategsf = false;
          client.SaveTimedStuff(14);
        }
        else if (client.ategsf && str1.StartsWith("((Please wait 5 days"))
          client.ategsf = false;
        if (str1.Equals("You feel the inner abyss for one Temuairan day"))
        {
          client.ascendtime = DateTime.Now;
          client.SaveTimedStuff(20);
        }
        if (str1.StartsWith("You cast ") && str1.Contains(" Prayer") && client.MapInfo.Tiles[client.ServerLocation.X, client.ServerLocation.Y] != null)
        {
          client.MapInfo.Tiles[client.ServerLocation.X, client.ServerLocation.Y].PrayerTimer = DateTime.UtcNow;
          client.MapInfo.Tiles[client.ServerLocation.X, client.ServerLocation.Y].prayermessagesent = false;
        }
        if (str1.StartsWith("You cast Gem Polishing."))
        {
          string key = client.ServerLocation.X.ToString() + "," + client.ServerLocation.Y.ToString();
          if (!client.GemPolish.ContainsKey(key))
            client.GemPolish.Add(key, DateTime.UtcNow);
          else
            client.GemPolish[key] = DateTime.UtcNow;
        }
        str1.Contains("None by that name here");
        if (str1.Contains("It's too heavy."))
          client.tooheavy = true;
        if (str1.Contains("You were distracted"))
          client.distracted = true;
        if (str1.Contains("assists your Tailoring") || str1.Contains("helps you collect") || str1.Contains("assists your wizardry research") || str1.Contains("prays for your success") || str1.Contains("helps you prepare the"))
          client.assisted = true;
        if (str1.StartsWith("You work for") || str1.StartsWith("You complete your") || str1.StartsWith("You pray") || str1 == "Some of your coins have split")
          client.polishsuccess = 1;
        if (str1.Contains(" doesn't need any jobs done. ") || str1.Contains(", although the Aisling didn't need much done"))
          client.polishsuccess = 2;
        if (str1.Contains(" is not near"))
          client.polishsuccess = 3;
        if (str1.Equals("You tailor well") || str1.Equals("You tailor excellently") || str1.Equals("You tailor a masterpiece garment") || str1.Equals("You tailor a garment that has no equal"))
          client.polishsuccess = 1;
        if (str1.Equals("You tailor, but do not improve the garment"))
          client.polishsuccess = 2;
        if (str1.Equals("You tailor horribly"))
          client.polishsuccess = 3;
        if (str1.Equals("You succeed at polishing"))
          client.polishsuccess = 1;
        if (str1.Equals("You polish, but it doesn't improve the gem"))
          client.polishsuccess = 2;
        if (str1.Equals("You crack a gem and blister yourself"))
          client.polishsuccess = 3;
        if (str1.StartsWith("(( 4 Tem") || str1.Contains("but you have no time") || str1.StartsWith("You do not have time") || str1.Equals("You have no more time for these four Temuairan days"))
        {
          if (client.autowalkon)
          {
            client.SendMessage("No labor, auto-walk stopped.");
            client.Tab.autowalker_button.Text = "Start";
            client.autowalkon = false;
          }
          client.outoflabor = true;
          if ((client.labortime == DateTime.MinValue ? 1 : (DateTime.Now.Subtract(client.labortime).TotalMinutes > 720.0 ? 1 : 0)) != 0)
          {
            client.labortime = DateTime.Now;
            client.SaveTimedStuff(32);
            client.SendMessage("Labor time saved");
          }
          if (str1.StartsWith("(( 4 Tem") && (client.Tab.improveskill.Text.Equals("Tailoring (cowl)") || client.Tab.improveskill.Text.Equals("Tailoring") || client.Tab.improveskill.Text.Equals("Blade Smith")) || str1.Contains("but you have no time"))
          {
            if (client.Tab.requestlabor.Checked && client.Tab.requestlabornametext.Text != string.Empty)
              client.Whisper(client.Tab.requestlabornametext.Text, client.Tab.requestlabormessagetext.Text);
            else
              client.SendMessage("You are out of labor!");
            client.waitingforlabor = true;
            client.Tab.impskillbutton.Text = "Start";
          }
          if (client.Tab.vpraybutton)
          {
            if (client.Tab.requestlabor.Checked && client.Tab.requestlabornametext.Text != string.Empty)
              client.Whisper(client.Tab.requestlabornametext.Text, client.Tab.requestlabormessagetext.Text);
            else
              client.SendMessage("You are out of labor!");
            client.waitingforlabor = true;
            client.Tab.praybutton.Text = "Start";
          }
        }
        if (str1.Contains("works for you for 1 day"))
        {
          client.outoflabor = false;
          if (client.laborcount > 0)
          {
            Client client1;
            int num8 = (client1 = client).laborcount - 1;
            client1.laborcount = num8;
          }
        }
        if (str1.StartsWith("You notice ") && str1.Contains(" but you do not have "))
        {
          client.herbnodewaittime = DateTime.MinValue;
          client.Tab.impskillbutton.Text = "Start";
          if (str1.Contains("Wine"))
          {
            client.outofwine = true;
            client.SendMessage("Get more wine.");
          }
        }
        if (str1.StartsWith("He tells you to search the book case"))
        {
          foreach (Client client2 in Server.Alts.Values.ToArray<Client>())
          {
            if (client2.Name.ToLower() == client.GroupMembers[0].ToLower() || client2.Name.ToLower() == client.Name.ToLower())
            {
              client2.Tab.mediumwalk.Checked = true;
              client2.Tab.autowalker_locales.SelectedItem = (object) "Loures";
              client2.Tab.walklocaleslist.SelectedItem = (object) "Library";
              client2.Tab.autowalker_button.Text = "Stop";
              client2.autowalkon = true;
              client2.letterquest = 5;
              client2.lettercourtney = false;
            }
          }
        }
        if (str1.Contains("Find Baltasar and say"))
        {
          foreach (Client client3 in Server.Alts.Values.ToArray<Client>())
          {
            if (client3.Name.ToLower() == client.GroupMembers[0].ToLower() || client3.Name.ToLower() == client.Name.ToLower())
            {
              client3.Tab.mediumwalk.Checked = true;
              client3.Tab.autowalker_locales.SelectedItem = (object) "Rucesion";
              client3.Tab.walklocaleslist.SelectedItem = (object) "Skill Master";
              client3.Tab.autowalker_button.Text = "Stop";
              client3.autowalkon = true;
              client3.letterquest = 4;
              if (client3.HasItem("Loures Song"))
                client3.UseItem("Loures Song");
              else if (client3.HasItem("Abel Song"))
                client3.UseItem("Abel Song");
            }
          }
        }
        if (str1.Contains("Return to Aoife"))
        {
          foreach (Client client4 in Server.Alts.Values.ToArray<Client>())
          {
            if (client4.Name.ToLower() == client.GroupMembers[0].ToLower() || client4.Name.ToLower() == client.Name.ToLower())
            {
              client4.Tab.mediumwalk.Checked = true;
              client4.Tab.autowalker_locales.SelectedItem = (object) "Mileth";
              client4.Tab.walklocaleslist.SelectedItem = (object) "Temple of Choosing";
              client4.Tab.autowalker_button.Text = "Stop";
              client4.autowalkon = true;
              client4.letterquest = 6;
              if (client4.HasItem("Abel Song"))
                client4.UseItem("Abel Song");
              else if (client4.HasItem("Loures Song"))
                client4.UseItem("Loures Song");
            }
          }
        }
        if (str1.Contains("Return to Frida"))
        {
          foreach (Client client5 in Server.Alts.Values.ToArray<Client>())
          {
            if (client5.Name.ToLower() == client.GroupMembers[0].ToLower() || client5.Name.ToLower() == client.Name.ToLower())
            {
              client5.Tab.mediumwalk.Checked = true;
              client5.Tab.autowalker_locales.SelectedItem = (object) "Abel";
              client5.Tab.walklocaleslist.SelectedItem = (object) "Tavern";
              client5.Tab.autowalker_button.Text = "Stop";
              client5.autowalkon = true;
              client5.letterquest = 6;
              if (client5.HasItem("Abel Song"))
                client5.UseItem("Abel Song");
              else if (client5.HasItem("Loures Song"))
                client5.UseItem("Loures Song");
            }
          }
        }
        if (str1.Contains("Return to Riona"))
        {
          foreach (Client client6 in Server.Alts.Values.ToArray<Client>())
          {
            if (client6.Name.ToLower() == client.GroupMembers[0].ToLower() || client6.Name.ToLower() == client.Name.ToLower())
            {
              client6.Tab.mediumwalk.Checked = true;
              client6.Tab.autowalker_locales.SelectedItem = (object) "Mileth";
              client6.Tab.walklocaleslist.SelectedItem = (object) "Inn";
              client6.Tab.autowalker_button.Text = "Stop";
              client6.autowalkon = true;
              client6.letterquest = 6;
              if (client6.HasItem("Abel Song"))
                client6.UseItem("Abel Song");
              else if (client6.HasItem("Loures Song"))
                client6.UseItem("Loures Song");
            }
          }
        }
        if (str1.Contains("Return to Duana"))
        {
          foreach (Client client7 in Server.Alts.Values.ToArray<Client>())
          {
            if (client7.Name.ToLower() == client.GroupMembers[0].ToLower() || client7.Name.ToLower() == client.Name.ToLower())
            {
              client7.Tab.mediumwalk.Checked = true;
              client7.Tab.autowalker_locales.SelectedItem = (object) "Mileth";
              client7.Tab.walklocaleslist.SelectedItem = (object) "Tavern";
              client7.Tab.autowalker_button.Text = "Stop";
              client7.autowalkon = true;
              client7.letterquest = 6;
              if (client7.HasItem("Abel Song"))
                client7.UseItem("Abel Song");
              else if (client7.HasItem("Loures Song"))
                client7.UseItem("Loures Song");
            }
          }
        }
        if ((str1.StartsWith("Your HP increased from") || str1.StartsWith("Your MP increased from")) && client.beforeascend > 0U)
        {
          string[] strArray = str1.Remove(str1.IndexOf('.')).Split(' ');
          int num9 = int.Parse(strArray[6]) - int.Parse(strArray[4]);
          DateTime now = DateTime.Now;
          AscendData z = new AscendData();
          z.Time = string.Format("{0:M/d/yy h:mm tt}", (object) now);
          z.Name = client.Name;
          z.EXP = (client.beforeascend - client.Statistics.Experience).ToString("#,##0");
          z.Increase = num9.ToString("#,##0") + " " + strArray[1];
          Server.AscendLog.Add(z);
          this.SaveAscendLog();
          this.PopulateAscendLogListView(z);
          client.beforeascend = 0U;
        }
        if (str1.StartsWith("No more experience") && client.Statistics.Experience > 4000000000U && !client.ascendexp)
          client.ascendexp = true;
        if (str1.Equals("Already a member of another group.") || str1.Equals("Cannot find group member."))
          return false;
        str1.StartsWith("You can't attack");
        if (str1.StartsWith("You don't have the right key") && client.MainTarget != null && client.MainTarget.Image == 456 && client.MainTarget.Lured && client.MonsterInFront() != null && client.MainTarget == client.MonsterInFront())
        {
          client.Characters[client.MainTarget.ID].WrongKey = true;
          client.MainTarget = (Npc) null;
          client.SendMessage("wrong key true");
        }
        if (str1.StartsWith("You cast ") || str1.StartsWith("You failed ") || str1.StartsWith("You already") || str1.StartsWith("Another curse") || str1.StartsWith("The magic") || str1.StartsWith("Your eye lids are becoming heavier.") || str1.StartsWith("Your hands are shaking"))
        {
          client.lastsuccessfulcast = DateTime.UtcNow;
          client.castonghosttimer = DateTime.MinValue;
        }
        if (str1.Equals("You cast Paralyze Realm."))
          client.pftime = DateTime.UtcNow;
        if (str1.ToLower().Contains("your hands are shaking"))
        {
          client.shakeyhandsdelay = DateTime.UtcNow;
          client.shakeyhands = true;
        }
        if (str1.Equals("You are facing death.") || str1.Equals("You are too injured to move."))
        {
          if (client.skullalert)
          {
            client.SendMessage(client.Name + " skulled!", "red", true);
            client.skullalert = false;
          }
          client.Disenchanter = (Npc) null;
          client.disIsSummoned = false;
          client.distime = DateTime.MinValue;
          client.IsSkulled = true;
          Server.StaticCharacters[client.PlayerID].IsSkulled = true;
          if (client.skullalertdelay == DateTime.MinValue)
            client.skullalertdelay = DateTime.UtcNow;
          if (DateTime.UtcNow.Subtract(client.skullalertdelay).TotalMilliseconds > 6000.0 && Program.MainForm.alertonskull.Checked && !Server.SentryAlarm)
          {
            Server.SentryAlarm = true;
            Server.alarm = new SoundPlayer(Assembly.GetExecutingAssembly().GetManifestResourceStream("Flintstones.LTTP_LowHealth.wav"));
            Server.alarmTimer = DateTime.UtcNow;
            Server.alarm.PlayLooping();
            client.skullalertdelay = DateTime.UtcNow;
          }
        }
        if (str1.Equals("Restored."))
        {
          if (Program.MainForm.alertonskull.Checked)
          {
            Server.SentryAlarm = false;
            Server.alarmTimer = DateTime.MinValue;
            if (Server.alarm != null)
              Server.alarm.Stop();
          }
          client.skullalertdelay = DateTime.MinValue;
          client.skullalert = true;
          client.IsSkulled = false;
          Server.SkullList.Remove(client.Name);
          Server.StaticCharacters[client.PlayerID].IsSkulled = false;
          client.SendMessage(client.Name + " was redded.", "grey", true);
          if (Program.MainForm.skulledlistview.Items.Count > 0 && Program.MainForm.skulledlistview.Items.ContainsKey(client.Name))
          {
            if (Server.SkullList.ContainsKey(client.Name))
              Server.SkullList.Remove(client.Name);
            foreach (ListViewItem listViewItem in Program.MainForm.skulledlistview.Items)
              Program.MainForm.skulledlistview.Items.Remove(listViewItem);
            this.SaveSkullList();
          }
        }
        if (str1.Equals("You lost 50 vitality.") || str1.StartsWith("You lost ") && str1.Contains(" experience."))
        {
          if (Program.MainForm.alertondeath.Checked && !Server.SentryAlarm)
          {
            Server.SentryAlarm = true;
            Server.alarm = new SoundPlayer(Assembly.GetExecutingAssembly().GetManifestResourceStream("Flintstones.Dcalarm1.wav"));
            Server.alarmTimer = DateTime.UtcNow;
            Server.alarm.Play();
          }
          client.skullalert = true;
          client.Disenchanter = (Npc) null;
          client.disIsSummoned = false;
          client.distime = DateTime.MinValue;
          client.IsSkulled = false;
          Server.SkullList.Remove(client.Name);
          Server.StaticCharacters[client.PlayerID].IsSkulled = false;
          client.SendMessage(client.Name + " died!", "red", true);
        }
        if (str1.Equals("You canot see anything."))
        {
          client.ImBlind = true;
          if (!client.SafeToWalkFast)
          {
            client.LastPermMessage = "{=qYou are blind";
            client.SendMessage("{=qYou are blind", (byte) 18);
          }
        }
        if (str1.Equals("You can see again."))
        {
          if (client.LastPermMessage.Contains("You are blind"))
          {
            client.LastPermMessage = "";
            client.SendMessage("", (byte) 18);
          }
          client.ImBlind = false;
        }
        if (str1.Equals("You are in hibernation.") || str1.Equals("Your body is freezing."))
          client.IsSuained = true;
        if (str1.Equals("Your body thaws."))
          client.IsSuained = false;
        if (str1.Equals("Stunned") && !client.MapInfo.Name.Contains("Chaos"))
          client.IsStunned = true;
        if (str1.Equals("A fume rises and you suddenly feel sleepy"))
          client.IsStunned = false;
        if (str1.Equals("You can move again."))
          client.IsStunned = false;
        if (str1.Equals("Kaze") && client.clickedamonster && client.Characters.ContainsKey(client.lastclickentityID) && client.Characters[client.lastclickentityID] != null)
        {
          client.Characters[client.lastclickentityID].Name = "Kaze";
          client.clickedamonster = false;
          return false;
        }
        if (str1.Equals("Norajo") && client.clickedamonster && client.Characters.ContainsKey(client.lastclickentityID) && client.Characters[client.lastclickentityID] != null)
        {
          client.Characters[client.lastclickentityID].Name = "Norajo";
          client.clickedamonster = false;
          return false;
        }
        if (str1.Contains("Worker") && client.clickedamonster && client.Characters.ContainsKey(client.lastclickentityID) && client.Characters[client.lastclickentityID] != null)
        {
          client.Characters[client.lastclickentityID].Name = "Worker";
          client.clickedamonster = false;
          return false;
        }
        if (str1.Equals("You cast Disenchanter."))
        {
          if (client.disenchanterappears)
          {
            client.disIsSummoned = true;
            client.distime = DateTime.UtcNow;
            client.disenchanterappears = false;
            if (client.discasttime == DateTime.MinValue)
              client.discasttime = DateTime.UtcNow;
          }
          else
          {
            client.Disenchanter = (Npc) null;
            client.disIsSummoned = false;
            client.distime = DateTime.MinValue;
          }
        }
        if (str1.Equals("It does not touch the spirit world.", StringComparison.CurrentCultureIgnoreCase) && client.LastMonsterId != 0U && client.Characters.ContainsKey(client.LastMonsterId) && client.Characters[client.LastMonsterId] != null && client.Characters[client.LastMonsterId] is Npc && client.Characters[client.LastMonsterId].IsOnScreen)
          client.Characters[client.LastMonsterId].IsDead = true;
        if (str1.Contains("is joining this group.") || str1.Contains("is leaving this group.") || str1.Equals("Group disbanded."))
          client.RequestGroupList();
        if (string.Equals(str1, "You can't use skills here.", StringComparison.CurrentCulture))
        {
          if (!client.CantSkillMaps.Contains(client.MapInfo.Number))
          {
            client.CantSkillMaps.Add(client.MapInfo.Number);
            client.SendMessage("Map " + client.MapInfo.Number.ToString() + " (" + client.MapInfo.Name + ") needs added to CantSkillMaps list.", "pink");
          }
          client.skillmap = false;
        }
        if (string.Equals(str1, "That doesn't work here.", StringComparison.CurrentCulture))
        {
          if (!client.CantSpellMaps.Contains(client.MapInfo.Number))
          {
            client.CantSpellMaps.Add(client.MapInfo.Number);
            client.SendMessage("Map " + client.MapInfo.Number.ToString() + " (" + client.MapInfo.Name + ") needs added to CantSpellMaps list.", "pink");
          }
          client.spellmap = false;
        }
        if (str1.ToLower().StartsWith("another curse") && client.LastSpell != string.Empty && client.LastTarget != 0U && client.Characters.ContainsKey(client.LastTarget) && client.Characters[client.LastTarget] != null && Server.StaticCharacters.ContainsKey(client.LastTarget) && Server.StaticCharacters[client.LastTarget] != null)
        {
          Character character;
          int num10 = (character = client.Characters[client.LastTarget]).AnotherCurseCount + 1;
          character.AnotherCurseCount = num10;
          if (!client.staffnow.Equals("Empowered Holy Gnarl"))
          {
            if (client.Characters[client.LastTarget].AnotherCurseCount >= 2)
            {
              if (str1.Contains("ard cradh"))
              {
                if (Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.ContainsKey(257))
                  Server.StaticCharacters[client.LastTarget].SpellAnimationHistory[257] = DateTime.UtcNow;
                else
                  Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.Add(257, DateTime.UtcNow);
                if (client.Tab.vmonitorcurses && client.Characters[client.LastTarget] is Player)
                  client.UpdatePlayerImage(client.Characters[client.LastTarget] as Player);
              }
              if (str1.Contains("mor cradh"))
              {
                if (Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.ContainsKey(243))
                  Server.StaticCharacters[client.LastTarget].SpellAnimationHistory[243] = DateTime.UtcNow;
                else
                  Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.Add(243, DateTime.UtcNow);
              }
              if (str1.Contains("cradh"))
              {
                if (Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.ContainsKey(258))
                  Server.StaticCharacters[client.LastTarget].SpellAnimationHistory[258] = DateTime.UtcNow;
                else
                  Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.Add(258, DateTime.UtcNow);
              }
              if (str1.Contains("bardo"))
              {
                if (Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.ContainsKey(44))
                  Server.StaticCharacters[client.LastTarget].SpellAnimationHistory[44] = DateTime.UtcNow;
                else
                  Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.Add(44, DateTime.UtcNow);
              }
              if (str1.Contains("Dark Seal"))
              {
                if (Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.ContainsKey(104))
                  Server.StaticCharacters[client.LastTarget].SpellAnimationHistory[104] = DateTime.UtcNow;
                else
                  Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.Add(104, DateTime.UtcNow);
                if (client.Tab.vmonitorcurses && client.Characters[client.LastTarget] is Player)
                  client.UpdatePlayerImage(client.Characters[client.LastTarget] as Player);
              }
              if (str1.Contains("Darker Seal"))
              {
                if (Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.ContainsKey(82))
                  Server.StaticCharacters[client.LastTarget].SpellAnimationHistory[82] = DateTime.UtcNow;
                else
                  Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.Add(82, DateTime.UtcNow);
                if (client.Tab.vmonitorcurses && client.Characters[client.LastTarget] is Player)
                  client.UpdatePlayerImage(client.Characters[client.LastTarget] as Player);
              }
              if (str1.Contains("Demise"))
              {
                if (Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.ContainsKey(75))
                  Server.StaticCharacters[client.LastTarget].SpellAnimationHistory[75] = DateTime.UtcNow;
                else
                  Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.Add(75, DateTime.UtcNow);
                if (client.Tab.vmonitorcurses && client.Characters[client.LastTarget] is Player)
                  client.UpdatePlayerImage(client.Characters[client.LastTarget] as Player);
              }
              if (str1.Contains("Demon Seal"))
              {
                if (Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.ContainsKey(76))
                  Server.StaticCharacters[client.LastTarget].SpellAnimationHistory[76] = DateTime.UtcNow;
                else
                  Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.Add(76, DateTime.UtcNow);
                if (client.Tab.vmonitorcurses && client.Characters[client.LastTarget] is Player)
                  client.UpdatePlayerImage(client.Characters[client.LastTarget] as Player);
              }
              client.Characters[client.LastTarget].AnotherCurseCount = 0;
            }
          }
          else if ((client.staffnow.Equals("Empowered Holy Gnarl") || str1.Contains("beag cradh")) && client.Characters[client.LastTarget].AnotherCurseCount > 4)
          {
            if (str1.Contains("ard cradh"))
            {
              if (Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.ContainsKey(257))
                Server.StaticCharacters[client.LastTarget].SpellAnimationHistory[257] = DateTime.UtcNow;
              else
                Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.Add(257, DateTime.UtcNow);
              if (client.Tab.vmonitorcurses && client.Characters[client.LastTarget] is Player)
                client.UpdatePlayerImage(client.Characters[client.LastTarget] as Player);
            }
            if (str1.Contains("mor cradh"))
            {
              if (Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.ContainsKey(243))
                Server.StaticCharacters[client.LastTarget].SpellAnimationHistory[243] = DateTime.UtcNow;
              else
                Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.Add(243, DateTime.UtcNow);
            }
            if (str1.Contains("cradh"))
            {
              if (Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.ContainsKey(258))
                Server.StaticCharacters[client.LastTarget].SpellAnimationHistory[258] = DateTime.UtcNow;
              else
                Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.Add(258, DateTime.UtcNow);
            }
            if (str1.Contains("bardo"))
            {
              if (Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.ContainsKey(44))
                Server.StaticCharacters[client.LastTarget].SpellAnimationHistory[44] = DateTime.UtcNow;
              else
                Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.Add(44, DateTime.UtcNow);
            }
            if (str1.Contains("beag cradh"))
            {
              if (Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.ContainsKey(259))
                Server.StaticCharacters[client.LastTarget].SpellAnimationHistory[259] = DateTime.UtcNow;
              else
                Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.Add(259, DateTime.UtcNow);
            }
            if (str1.Contains("Dark Seal"))
            {
              if (Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.ContainsKey(104))
                Server.StaticCharacters[client.LastTarget].SpellAnimationHistory[104] = DateTime.UtcNow;
              else
                Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.Add(104, DateTime.UtcNow);
            }
            if (str1.Contains("Darker Seal"))
            {
              if (Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.ContainsKey(82))
                Server.StaticCharacters[client.LastTarget].SpellAnimationHistory[82] = DateTime.UtcNow;
              else
                Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.Add(82, DateTime.UtcNow);
            }
            if (str1.Contains("Demise"))
            {
              if (Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.ContainsKey(75))
                Server.StaticCharacters[client.LastTarget].SpellAnimationHistory[75] = DateTime.UtcNow;
              else
                Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.Add(75, DateTime.UtcNow);
            }
            if (str1.Contains("Demon Seal"))
            {
              if (Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.ContainsKey(76))
                Server.StaticCharacters[client.LastTarget].SpellAnimationHistory[76] = DateTime.UtcNow;
              else
                Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.Add(76, DateTime.UtcNow);
            }
            client.Characters[client.LastTarget].AnotherCurseCount = 0;
          }
        }
        if (str1.ToLower().StartsWith("you already cast") && client.LastSpell != string.Empty && client.LastTarget != 0U && client.Characters.ContainsKey(client.LastTarget) && client.Characters[client.LastTarget] != null && Server.StaticCharacters.ContainsKey(client.LastTarget) && Server.StaticCharacters[client.LastTarget] != null)
        {
          if (client.LastSpell.Contains("fas nadur") && client.Characters[client.LastTarget] is Npc)
          {
            Character character;
            int num11 = (character = client.Characters[client.LastTarget]).AnotherFasCount + 1;
            character.AnotherFasCount = num11;
            if (client.Characters[client.LastTarget].AnotherFasCount > 4)
            {
              if (Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.ContainsKey(273))
                Server.StaticCharacters[client.LastTarget].SpellAnimationHistory[273] = DateTime.UtcNow;
              else
                Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.Add(273, DateTime.UtcNow);
              client.Characters[client.LastTarget].AnotherFasCount = 0;
            }
          }
          if (client.LastSpell.Contains("aite"))
          {
            if (Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.ContainsKey(231))
              Server.StaticCharacters[client.LastTarget].SpellAnimationHistory[231] = DateTime.UtcNow;
            else
              Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.Add(231, DateTime.UtcNow);
            if (client.Tab.vmonitorspells && client.Characters[client.LastTarget] is Player)
              client.UpdatePlayerImage(client.Characters[client.LastTarget] as Player);
          }
          if (client.LastSpell.Contains("fas nadur") && client.Characters[client.LastTarget] is Player)
          {
            if (Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.ContainsKey(273))
              Server.StaticCharacters[client.LastTarget].SpellAnimationHistory[273] = DateTime.UtcNow;
            else
              Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.Add(273, DateTime.UtcNow);
            if (client.Tab.vmonitorspells && client.Characters[client.LastTarget] is Player)
              client.UpdatePlayerImage(client.Characters[client.LastTarget] as Player);
          }
          if (client.LastSpell.Contains("beannaich"))
          {
            if (Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.ContainsKey(280))
              Server.StaticCharacters[client.LastTarget].SpellAnimationHistory[280] = DateTime.UtcNow;
            else
              Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.Add(280, DateTime.UtcNow);
          }
          if (client.LastSpell.Contains("armachd"))
          {
            if (Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.ContainsKey(20))
              Server.StaticCharacters[client.LastTarget].SpellAnimationHistory[20] = DateTime.UtcNow;
            else
              Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.Add(20, DateTime.UtcNow);
          }
          if (client.LastSpell.Contains("creag neart"))
          {
            if (Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.ContainsKey(6))
              Server.StaticCharacters[client.LastTarget].SpellAnimationHistory[6] = DateTime.UtcNow;
            else
              Server.StaticCharacters[client.LastTarget].SpellAnimationHistory.Add(6, DateTime.UtcNow);
          }
        }
        if (client.Tab.recorditemdata.Checked && !char.IsPunctuation(str1.Last<char>()) && str1 != "Harden body spell" && !str1.Contains(" member ") && !str1.Contains(" picks up ") && str1 != "Poison" && !str1.Contains("cradh") && str1 != "In sleep" && str1 != "Awake" && str1 != "double attribute" && !str1.Contains<char>('"') && !str1.Contains("!") && !str1.Contains(">") && !str1.Contains(":") && !str1.StartsWith("The scent") && (!str1.Equals(" Boots") ? (!str1.StartsWith(" ") ? 1 : 0) : 1) != 0 && !str1.StartsWith("You ") && !str1.StartsWith("Your ") && str1 != "Rento" && str1 != "Halting" && str1 != "Halt" && str1 != "Pause" && str1 != "Purify" && !str1.Contains("Sun, ") && str1 != "Bardo" && str1 != "Stunned" && str1 != "Dark Seal" && str1 != "Can't exchange" && !str1.StartsWith("Something terrible") && str1 != "Summoner Attack Silence" && !str1.StartsWith("Spell Attack Silence") && !str1.StartsWith("Counter Attack") && !str1.StartsWith("Regeneration") && !str1.StartsWith("ABILITY_") && !str1.StartsWith("A fume rises and you") && !str1.Equals("All goes black") && !str1.StartsWith("Something sticks into you skin") && !str1.Equals("A sharp stick pricks you"))
        {
          if (!Server.entitynametesting.Contains(str1))
          {
            Server.entitynametesting.Add(str1);
            if (!str1.Equals("fior sal") && !str1.Equals("fior athar") && !str1.Equals("fior creag") && !str1.Equals("fior srad") && !str1.Equals("Gold Pile") && !str1.Equals("Gold Coin") && !str1.Equals("Silver Pile") && !str1.Equals("Silver Coin"))
              client.SendMessage(str1);
          }
          if (client.ClickedEntityID > 0U)
          {
            if (client.Characters.ContainsKey(client.ClickedEntityID))
              client.Characters[client.ClickedEntityID].Name = str1;
            client.ClickedEntityID = 0U;
            client.EntityClickTimer = DateTime.MinValue;
            return false;
          }
        }
      }
      if (num1 == (byte) 0)
      {
        if (str1.Contains(" can't hear you."))
          client.whisperagain = DateTime.UtcNow;
        int startIndex1 = str1.IndexOf(">");
        int startIndex2 = str1.IndexOf('"');
        string str18 = string.Empty;
        string str19 = string.Empty;
        bool flag = false;
        if (startIndex1 > -1 && (startIndex2 == -1 || startIndex1 < startIndex2))
        {
          str18 = str1.Remove(startIndex1);
          str19 = str1.Remove(0, startIndex1 + 2);
          flag = true;
        }
        else if (startIndex2 > -1 && (startIndex1 == -1 || startIndex2 < startIndex1))
        {
          str18 = str1.Remove(startIndex2);
          str19 = str1.Remove(0, startIndex2 + 2);
          flag = false;
        }
        if (str18 != string.Empty && client.Tab.ExternalChat.Visible)
        {
          client.Tab.ExternalChat.chatbox.AppendText(Environment.NewLine + str1);
          client.Tab.ExternalChat.chatbox.ScrollToCaret();
        }
        if (!flag && str18 != string.Empty && client.Tab.laborname.Text != string.Empty && client.Tab.laborinresponse.Checked && str19.Equals(client.Tab.laborinresponsetext.Text))
          client.Tab.laborbutton.Text = "Stop";
        if (!flag && str18 != string.Empty && client.impingskill && (client.Tab.praytemple.Checked || client.Tab.praynecklace.Checked) && client.Tab.impskillinresponse.Checked && str19.Equals(client.Tab.impskillinresponsetext.Text))
        {
          client.waitingforlabor = false;
          client.Tab.praybutton.Text = "Stop";
        }
        else if (!flag && str18 != string.Empty && client.impingskill && client.Tab.impskillinresponse.Checked && str19.Equals(client.Tab.impskillinresponsetext.Text))
        {
          client.waitingforlabor = false;
          client.Tab.impskillbutton.Text = "Stop";
          client.Refresh();
        }
        if (client.blockchat & flag && str19.Contains(Server.Stuff[client.Name]))
        {
          client.blockchat = false;
          return false;
        }
        if (client.Tab.vfriendspeak && Server.friendlist != null && Server.friendlist.Contains(str18.ToLower()) && !flag && str19.StartsWith("say "))
        {
          string message = str19.Remove(0, 4);
          if (message.ToLower().StartsWith("f5"))
            client.RefreshAllClients();
          else if (message.ToLower().StartsWith("alarm"))
          {
            if (!Server.SentryAlarm)
            {
              User32.FlashWindow(client.mainProc.MainWindowHandle, false);
              Server.SentryAlarm = true;
              Server.alarm = new SoundPlayer(Assembly.GetExecutingAssembly().GetManifestResourceStream("Flintstones.Dcalarm1.wav"));
              Server.alarmTimer = DateTime.UtcNow;
              Server.alarm.Play();
            }
          }
          else
            client.Speak(message);
        }
        int num12 = client.safemode ? 1 : 0;
      }
      return true;
    }

    public bool ServerMessage_0x0B_MoveClient(Client client, ServerPacket msg)
    {
      Direction direction = (Direction) msg.ReadByte();
      int num1 = (int) msg.ReadUInt16();
      int num2 = (int) msg.ReadUInt16();
      switch (direction)
      {
        case Direction.North:
          client.ServerLocation.X = num1;
          client.ServerLocation.Y = num2 - 1;
          break;
        case Direction.East:
          client.ServerLocation.X = num1 + 1;
          client.ServerLocation.Y = num2;
          break;
        case Direction.South:
          client.ServerLocation.X = num1;
          client.ServerLocation.Y = num2 + 1;
          break;
        case Direction.West:
          client.ServerLocation.X = num1 - 1;
          client.ServerLocation.Y = num2;
          break;
      }
      client.checkedtiles.Add(client.ServerLocation.X.ToString() + "," + client.ServerLocation.Y.ToString());
      client.ClientLocation.Direction = direction;
      client.ServerLocation.Direction = direction;
      if (client.Characters.ContainsKey(client.PlayerID) && client.Characters[client.PlayerID] != null)
      {
        client.Characters[client.PlayerID].Location.X = client.ServerLocation.X;
        client.Characters[client.PlayerID].Location.Y = client.ServerLocation.Y;
        client.Characters[client.PlayerID].Location.Direction = direction;
      }
      return true;
    }

    public bool ServerMessage_0x0C_MoveCharacter(Client client, ServerPacket msg)
    {
      uint key = msg.ReadUInt32();
      int num1 = (int) msg.ReadUInt16();
      int num2 = (int) msg.ReadUInt16();
      int num3 = num1;
      int num4 = num2;
      Direction direction = (Direction) msg.ReadByte();
      switch (direction)
      {
        case Direction.North:
          --num4;
          break;
        case Direction.East:
          ++num3;
          break;
        case Direction.South:
          ++num4;
          break;
        case Direction.West:
          --num3;
          break;
      }
      if ((int) key == (int) client.PlayerID)
      {
        client.ClientLocation.Direction = direction;
        client.ServerLocation.Direction = direction;
      }
      if (client.Characters.ContainsKey(key) && client.Characters[key] != null && (int) key != (int) client.PlayerID)
      {
        client.Characters[key].LastAction = DateTime.UtcNow;
        client.Characters[key].Location.Y = num4;
        client.Characters[key].Location.X = num3;
        client.Characters[key].Location.Direction = direction;
        if (!client.Characters[key].Moved)
          client.Characters[key].Moved = true;
      }
      return true;
    }

    public bool ServerMessage_0x0D_Chat(Client client, ServerPacket msg)
    {
      byte num = msg.ReadByte();
      uint key1 = msg.ReadUInt32();
      string key2 = msg.ReadString((int) msg.ReadByte());
      string str1 = key2;
      if ((num == (byte) 1 || num == (byte) 0) && Server.ignoreaislinglist.Count<string>() > 0)
      {
        foreach (string str2 in Server.ignoreaislinglist)
        {
          if (key2.ToLower().Contains(str2))
            return false;
        }
      }
      if (client.Tab.ExternalChat.Visible)
      {
        client.Tab.ExternalChat.chatbox.AppendText(Environment.NewLine + key2);
        client.Tab.ExternalChat.chatbox.ScrollToCaret();
      }
      if (num == (byte) 0 || num == (byte) 1)
        key2 = key2.Remove(0, key2.IndexOf(" ") + 1);
      if (num == (byte) 0 && (int) key1 == (int) client.PlayerID)
      {
        if (key2.Equals("Don't get lost again.") || key2.StartsWith("Let's go to mommy.") || key2.Equals("You are safe now!") || key2.Equals("Gotcha!") || key2.Equals("Don't be scared."))
          client.losterbiedelay = DateTime.UtcNow;
        if (key2.Equals("I caught one!") || key2.Equals("Victory!") || key2.Equals("Got it!") || key2.Equals("Gotcha!") || key2.Equals("Got one!"))
          client.bugtimer = DateTime.UtcNow;
      }
      if ((key2.Contains("Kill...") || key2.Contains("Ahhh...")) && Server.StaticCharacters.ContainsKey(key1) && Server.StaticCharacters[key1] != null && !Server.StaticCharacters[key1].HasSummoned)
        Server.StaticCharacters[key1].HasSummoned = true;
      if (num == (byte) 2 && client.Characters.ContainsKey(key1) && client.Characters[key1] != null && Server.SpellList.ContainsKey(key2))
        client.Characters[key1].LastBlueText = key2;
      if ((num == (byte) 0 || num == (byte) 1) && client.Characters.ContainsKey(key1) && client.Characters[key1] != null && Server.StaticCharacters.ContainsKey(key1) && Server.StaticCharacters[key1] != null && client.Characters[key1].IsOnScreen && (Server.friendlist.Contains(client.Characters[key1].Name.ToLower()) || client.GroupMembers.Contains(client.Characters[key1].Name)))
      {
        if (client.Tab.respondaite.Checked && key2.Equals("aite", StringComparison.CurrentCultureIgnoreCase) && !Server.StaticCharacters[key1].hasaite && client.HasSpell("ard naomh aite"))
          client.CastSpell("ard naomh aite", new uint?(key1));
        if (client.Tab.respondfas.Checked && key2.Equals("fas", StringComparison.CurrentCultureIgnoreCase) && !Server.StaticCharacters[key1].hasfas && client.HasSpell("mor fas nadur"))
          client.CastSpell("mor fas nadur", new uint?(key1));
        if (client.Tab.respondflower.Checked && (key2.Equals("flower", StringComparison.CurrentCultureIgnoreCase) || key2.Equals("f", StringComparison.CurrentCultureIgnoreCase)))
          Server.StaticCharacters[key1].wantsflowered = true;
      }
      if (!client.Tab.chattimestamp.Checked || num != (byte) 0)
        return true;
      DateTime now = DateTime.Now;
      ServerPacket msg1 = new ServerPacket((byte) 13);
      msg1.WriteByte(num);
      msg1.WriteUInt32(key1);
      msg1.WriteString8(now.ToString("hh:mm") + ">" + str1);
      msg1.Write(new byte[3]);
      msg1.Write(new byte[3]);
      client.Enqueue(msg1);
      return false;
    }

    public bool ServerMessage_0x0E_RemoveCharacter(Client client, ServerPacket msg)
    {
      uint key = msg.ReadUInt32();
      DateTime utcNow;
      if (client.Characters.ContainsKey(key) && client.Characters[key] is Npc && (client.Characters[key] as Npc).Image == 3 && client.disIsSummoned && client.Disenchanter != null && (int) client.Disenchanter.ID == (int) key)
      {
        utcNow = DateTime.UtcNow;
        if (utcNow.Subtract(client.Characters[key].CreateTime).TotalSeconds < 5.0)
        {
          client.Disenchanter = (Npc) null;
          client.disIsSummoned = false;
          client.distime = DateTime.MinValue;
        }
      }
      if (client.Characters.ContainsKey(key) && client.Characters[key] is Npc && !client.Characters[key].Counted && ((client.Characters[key] as Npc).Type == Npc.NpcType.PassableMonster || (client.Characters[key] as Npc).Type == Npc.NpcType.NormalMonster))
        client.LastDeadMonster = key;
      if (client.Tab.recorditemdata.Checked)
      {
        if (client.Characters.ContainsKey(key) && client.Characters[key] is Npc && !client.Characters[key].CountedItsKill && ((client.Characters[key] as Npc).Type == Npc.NpcType.PassableMonster || (client.Characters[key] as Npc).Type == Npc.NpcType.NormalMonster))
          client.LastDeadNpc = key;
        if (client.Characters.ContainsKey(key))
        {
          foreach (Character character in client.Characters.Values.OrderByDescending<Character, DateTime>((Func<Character, DateTime>) (c => c.CreateTime)).ToArray<Character>())
          {
            if (character != null && character.IsOnScreen && !character.WasDropped && character is Npc && (character as Npc).Type == Npc.NpcType.Item && character.SpawnLocation.X == client.Characters[key].Location.X && character.SpawnLocation.Y == client.Characters[key].Location.Y)
            {
              utcNow = DateTime.UtcNow;
              if (utcNow.Subtract(character.CreateTime).TotalMilliseconds < 800.0)
              {
                character.WasDropped = true;
                client.Characters[key].DropList.Add(character.ID);
                client.itemdroppeddelay = DateTime.UtcNow;
              }
            }
          }
        }
        if (client.Characters.ContainsKey(key) && client.Characters[key] is Npc && (client.Characters[key] as Npc).Type == Npc.NpcType.Item && (client.Characters[key].Name == "Gold Pile" || client.Characters[key].Name == "Silver Pile"))
          client.LastVanishedGold = key;
      }
      if (client.Characters.ContainsKey(key) && client.Characters[key] != null && (int) key != (int) client.PlayerID)
      {
        if (client.Characters[key] is Player && (int) key == (int) client.checkingformentormarkid)
        {
          client.checkingformentormarkname = string.Empty;
          client.checkingformentormarkid = 0U;
        }
        lock (client.Characters)
        {
          client.Characters[key].Lured = false;
          client.Characters[key].InViewTime = DateTime.UtcNow;
          client.Characters[key].DeathTime = DateTime.UtcNow;
          client.Characters[key].IsOnScreen = false;
        }
        if (client.MainTarget != null && (int) client.MainTarget.ID == (int) key)
          client.follow_walk = 0;
      }
      if (client.Characters.ContainsKey(key) && client.Characters[key] != null && client.MainTarget != null && (int) key == (int) client.MainTarget.ID)
        client.MainTarget = (Npc) null;
      if (client.Characters.ContainsKey(key) && client.Characters[key] != null && (int) client.LastMonsterId == (int) key)
        client.LastMonsterId = 0U;
      if (client.Tab.vusemonster && client.Tab.vusemonsterid > 0 && !client.imonster && client.Characters.ContainsKey(key) && client.Characters[key] != null && client.Characters[key] is Player && client.SafeToWalkFast)
        client.UseMonsterForm();
      if (client.MapInfo.Name.Contains("Aman") && client.Characters.ContainsKey(key) && client.Characters[key] != null && client.Characters[key] is Npc && (client.Characters[key] as Npc).Image == 856)
        client.Characters[key].SpellAnimationHistory.Clear();
      if (client.MapInfo.Name.Contains("Yowien") && client.Characters.ContainsKey(key) && client.Characters[key] != null && client.Characters[key] is Npc && (client.Characters[key] as Npc).Image != 583 && (client.Characters[key] as Npc).Image != 663 && (client.Characters[key] as Npc).Image != 662 && (client.Characters[key] as Npc).Image != 859 && (client.Characters[key] as Npc).Image != 860)
        client.Characters[key].SpellAnimationHistory.Clear();
      if (client.MapInfo.Name.Equals("Lost Ruins 6") && client.Characters.ContainsKey(key) && client.Characters[key] != null && client.Characters[key] is Npc)
        client.Characters[key].SpellAnimationHistory.Clear();
      if (client.MapInfo.Name.Contains("Veltain Mines") && client.Characters.ContainsKey(key) && client.Characters[key] != null && client.Characters[key] is Npc)
        client.Characters[key].SpellAnimationHistory.Clear();
      return true;
    }

    public bool ServerMessage_0x0F_AddItem(Client client, ServerPacket msg)
    {
      Item obj1 = new Item();
      obj1.InventorySlot = (int) msg.ReadByte();
      obj1.Icon = msg.ReadUInt16();
      obj1.IconPal = msg.ReadByte();
      obj1.Name = msg.ReadString((int) msg.ReadByte());
      obj1.Amount = msg.ReadUInt32();
      obj1.Stackable = msg.ReadByte();
      obj1.MaximumDurability = msg.ReadUInt32();
      obj1.CurrentDurability = msg.ReadUInt32();
      client.Inventory[obj1.InventorySlot - 1] = obj1;
      if (client.Tab.recorditemdata.Checked)
      {
        foreach (Character character in client.Characters.Values.ToArray<Character>())
        {
          if (character != null && character is Npc && (character as Npc).Type == Npc.NpcType.Item && obj1.Name == character.Name && character.InventorySlot == obj1.InventorySlot && character.IsIdentified)
          {
            obj1.IsIdentified = true;
            break;
          }
        }
        foreach (Character character in client.Characters.Values.OrderByDescending<Character, DateTime>((Func<Character, DateTime>) (c => c.DeathTime)).ToArray<Character>())
        {
          if (character != null && character is Npc && (character as Npc).Type == Npc.NpcType.Item && obj1.Name == character.Name && !character.Looted && character.DeathTime != DateTime.MinValue && DateTime.UtcNow.Subtract(character.DeathTime).TotalMilliseconds < 800.0)
          {
            character.Looted = true;
            character.InventorySlot = obj1.InventorySlot;
            break;
          }
        }
      }
      else
      {
        foreach (Character character in client.Characters.Values.OrderByDescending<Character, DateTime>((Func<Character, DateTime>) (c => c.DeathTime)).ToArray<Character>())
        {
          if (character != null && character is Npc && (character as Npc).Type == Npc.NpcType.Item && (int) obj1.Icon - 32768 == (character as Npc).Image - 16384 && !Server.StaticCharacters[character.ID].Looted && character.DeathTime != DateTime.MinValue && DateTime.UtcNow.Subtract(character.DeathTime).TotalMilliseconds < 800.0)
          {
            character.Looted = true;
            character.InventorySlot = obj1.InventorySlot;
            break;
          }
        }
      }
      if (Program.MainForm.recordchestdata.Checked)
      {
        if (client.agchestopen)
        {
          if (Server.ChestDatabase.ContainsKey("Arcella's Gift1"))
          {
            ++Server.ChestDatabase["Arcella's Gift1"].OpenedCount;
            if (Server.ChestDatabase["Arcella's Gift1"].Treasure.ContainsKey(obj1.Name))
              Server.ChestDatabase["Arcella's Gift1"].Treasure[obj1.Name]++;
            else
              Server.ChestDatabase["Arcella's Gift1"].Treasure.Add(obj1.Name, 1);
          }
          client.agchestopen = false;
          Program.MainForm.BeginInvoke((Action) (() => Program.MainForm.ItemXMLEditor.UpdateChestsForm()));
        }
        if (client.wdchestopen)
        {
          if (Server.ChestDatabase.ContainsKey("Water Dungeon Chest"))
          {
            ++Server.ChestDatabase["Water Dungeon Chest"].OpenedCount;
            if (Server.ChestDatabase["Water Dungeon Chest"].Treasure.ContainsKey(obj1.Name))
              Server.ChestDatabase["Water Dungeon Chest"].Treasure[obj1.Name]++;
            else
              Server.ChestDatabase["Water Dungeon Chest"].Treasure.Add(obj1.Name, 1);
          }
          client.wdchestopen = false;
          Program.MainForm.BeginInvoke((Action) (() => Program.MainForm.ItemXMLEditor.UpdateChestsForm()));
        }
        if (client.andorchestopen)
        {
          if (Server.ChestDatabase.ContainsKey("Andor Chest"))
          {
            ++Server.ChestDatabase["Andor Chest"].OpenedCount;
            if (Server.ChestDatabase["Andor Chest"].Treasure.ContainsKey(obj1.Name))
              Server.ChestDatabase["Andor Chest"].Treasure[obj1.Name]++;
            else
              Server.ChestDatabase["Andor Chest"].Treasure.Add(obj1.Name, 1);
          }
          client.andorchestopen = false;
          Program.MainForm.BeginInvoke((Action) (() => Program.MainForm.ItemXMLEditor.UpdateChestsForm()));
        }
        if (client.queenchestopen)
        {
          if (Server.ChestDatabase.ContainsKey("Queen's Chest"))
          {
            ++Server.ChestDatabase["Queen's Chest"].OpenedCount;
            if (Server.ChestDatabase["Queen's Chest"].Treasure.ContainsKey(obj1.Name))
              Server.ChestDatabase["Queen's Chest"].Treasure[obj1.Name]++;
            else
              Server.ChestDatabase["Queen's Chest"].Treasure.Add(obj1.Name, 1);
          }
          client.queenchestopen = false;
          Program.MainForm.BeginInvoke((Action) (() => Program.MainForm.ItemXMLEditor.UpdateChestsForm()));
        }
        if (client.veltainchestopen && obj1.Name != "Treasure Chest")
        {
          if (Server.ChestDatabase.ContainsKey("Veltain Chest " + client.chestfee))
          {
            ++Server.ChestDatabase["Veltain Chest " + client.chestfee].OpenedCount;
            if (Server.ChestDatabase["Veltain Chest " + client.chestfee].Treasure.ContainsKey(obj1.Name))
              Server.ChestDatabase["Veltain Chest " + client.chestfee].Treasure[obj1.Name]++;
            else
              Server.ChestDatabase["Veltain Chest " + client.chestfee].Treasure.Add(obj1.Name, 1);
          }
          else
            Server.ChestDatabase.Add("Veltain Chest " + client.chestfee, new ChestItemXML("Veltain Chest " + client.chestfee, 1U)
            {
              Treasure = {
                {
                  obj1.Name,
                  1
                }
              }
            });
          client.veltainchestopen = false;
          Program.MainForm.BeginInvoke((Action) (() => Program.MainForm.ItemXMLEditor.UpdateChestsForm()));
        }
        if (client.heavychestopen && obj1.Name != "Treasure Chest")
        {
          if (Server.ChestDatabase.ContainsKey("Heavy Veltain Chest " + client.chestfee))
          {
            ++Server.ChestDatabase["Heavy Veltain Chest " + client.chestfee].OpenedCount;
            if (Server.ChestDatabase["Heavy Veltain Chest " + client.chestfee].Treasure.ContainsKey(obj1.Name))
              Server.ChestDatabase["Heavy Veltain Chest " + client.chestfee].Treasure[obj1.Name]++;
            else
              Server.ChestDatabase["Heavy Veltain Chest " + client.chestfee].Treasure.Add(obj1.Name, 1);
          }
          else
            Server.ChestDatabase.Add("Heavy Veltain Chest " + client.chestfee, new ChestItemXML("Heavy Veltain Chest " + client.chestfee, 1U)
            {
              Treasure = {
                {
                  obj1.Name,
                  1
                }
              }
            });
          client.heavychestopen = false;
          Program.MainForm.BeginInvoke((Action) (() => Program.MainForm.ItemXMLEditor.UpdateChestsForm()));
        }
        if (client.smallbagopen && obj1.Name != "Treasure Chest")
        {
          if (Server.ChestDatabase.ContainsKey("Canal Bag"))
          {
            ++Server.ChestDatabase["Canal Bag"].OpenedCount;
            if (Server.ChestDatabase["Canal Bag"].Treasure.ContainsKey(obj1.Name))
              Server.ChestDatabase["Canal Bag"].Treasure[obj1.Name]++;
            else
              Server.ChestDatabase["Canal Bag"].Treasure.Add(obj1.Name, 1);
          }
          client.smallbagopen = false;
          Program.MainForm.BeginInvoke((Action) (() => Program.MainForm.ItemXMLEditor.UpdateChestsForm()));
        }
        if (client.bigbagopen && obj1.Name != "Treasure Chest")
        {
          if (Server.ChestDatabase.ContainsKey("Big Canal Bag"))
          {
            ++Server.ChestDatabase["Big Canal Bag"].OpenedCount;
            if (Server.ChestDatabase["Big Canal Bag"].Treasure.ContainsKey(obj1.Name))
              Server.ChestDatabase["Big Canal Bag"].Treasure[obj1.Name]++;
            else
              Server.ChestDatabase["Big Canal Bag"].Treasure.Add(obj1.Name, 1);
          }
          client.bigbagopen = false;
          Program.MainForm.BeginInvoke((Action) (() => Program.MainForm.ItemXMLEditor.UpdateChestsForm()));
        }
        if (client.heavybagopen && obj1.Name != "Treasure Chest")
        {
          if (Server.ChestDatabase.ContainsKey("Heavy Canal Bag"))
          {
            ++Server.ChestDatabase["Heavy Canal Bag"].OpenedCount;
            if (Server.ChestDatabase["Heavy Canal Bag"].Treasure.ContainsKey(obj1.Name))
              Server.ChestDatabase["Heavy Canal Bag"].Treasure[obj1.Name]++;
            else
              Server.ChestDatabase["Heavy Canal Bag"].Treasure.Add(obj1.Name, 1);
          }
          client.heavybagopen = false;
          Program.MainForm.BeginInvoke((Action) (() => Program.MainForm.ItemXMLEditor.UpdateChestsForm()));
        }
      }
      client.swappingitem = false;
      client.waitingonlore = false;
      if (obj1.MaximumDurability > 0U && obj1.Name != "Warranty Bag" && (obj1.CurrentDurability < 800U && obj1.MaximumDurability > 1000U || (double) obj1.CurrentDurability / (double) obj1.MaximumDurability * 100.0 < 80.0) && !client.HasLowDuraDN())
        client.needsrepaired = true;
      if (client.polishsuccess == 1)
        client.dropitemslot = obj1.InventorySlot;
      if (obj1.InventorySlot == 1 && client.firstitemslot == "")
        client.firstitemslot = obj1.Name;
      if (obj1.Name == "Giant Pearl" && client.giantpearl)
        client.giantpearl2 = true;
      else if ((int) obj1.Icon - 32768 == 54 && obj1.Amount == 25U && client.Tab.impskillbutton.Text == "Stop")
        client.has25hydele = true;
      else if ((int) obj1.Icon - 32768 == 62 && obj1.Amount == 25U && client.Tab.impskillbutton.Text == "Stop")
        client.has25betony = true;
      else if ((int) obj1.Icon - 32768 == 55 && obj1.Amount == 25U && client.Tab.impskillbutton.Text == "Stop")
        client.has25personaca = true;
      else if (obj1.Name == "Komadium")
      {
        if (obj1.Amount == 10U)
          client.SendMessage(client.Name + " is low on Komadiums! [10 left]", "red", true);
        else if (obj1.Amount == 5U)
          client.SendMessage(client.Name + " is low on Komadiums! [5 left]", "red", true);
        else if (obj1.Amount == 1U)
          client.SendMessage(client.Name + " is low on Komadiums! [1 left]", "red", true);
      }
      else if (obj1.Name.Contains("Prayer Necklace"))
      {
        object obj2 = new object();
        string[] strArray = obj1.Name.Split(' ');
        if (!client.Tab.prayernecklist.Items.Contains((object) obj1.Name))
          client.Tab.prayernecklist.Items.Add((object) obj1.Name);
        if (client.PrayerSpell.Contains(strArray[0]))
        {
          object name = (object) obj1.Name;
          client.PrayerNeck = obj1.Name;
          client.Tab.prayernecklist.SelectedItem = name;
        }
      }
      if (obj1.Name == "Wine" && client.outofwine)
      {
        client.outofwine = false;
        client.Tab.impskillbutton.Text = "Stop";
      }
      if (client.Tab.pigwalk.Checked && client.HasItem("Ability and Experience Gift 1") && client.ItemAmount("Ability and Experience Gift 1") == 5U)
      {
        client.SendMessage("Stopped walking, you're at max stack of gift 1s", "red");
        client.pause = true;
        client.Tab.btnPlay.Enabled = true;
        client.Tab.btnStop.Enabled = false;
      }
      if (client.Tab.pigwalk.Checked && client.HasItem("Ability and Experience Gift 2") && client.ItemAmount("Ability and Experience Gift 2") == 5U)
      {
        client.SendMessage("Stopped walking, you're at max stack of gift 2s", "red");
        client.pause = true;
        client.Tab.btnPlay.Enabled = true;
        client.Tab.btnStop.Enabled = false;
      }
      if (client.MapInfo.Name == "Mount Giragan 1" || client.MapInfo.Name == "Mount Giragan 2" || client.MapInfo.Name == "Mount Giragan 3")
      {
        if (client.HasItem("Cedar Log"))
          client.cedarlogs = client.ItemAmount("Cedar Log");
        if (client.HasItem("Fir Log"))
          client.firlogs = client.ItemAmount("Fir Log");
        client.yulelogcount = "{=bC " + client.cedarlogs.ToString() + ", F " + client.firlogs.ToString();
        if (client.ItemAmount("Fir Log") == 12U && client.ItemAmount("Cedar Log") == 12U)
        {
          client.Tab.autowalker_locales.Text = "Suomi";
          client.Tab.walklocaleslist.SelectedItem = (object) "Weapon Shop";
          client.Tab.autowalker_button.Text = "Stop";
          client.autowalkon = true;
          client.Tab.mediumwalk.Checked = true;
          client.pause = false;
          client.Tab.btnPlay.Enabled = false;
          client.Tab.btnStop.Enabled = true;
          client.LastnpcpopupID = 0U;
          client.yulequest = true;
          client.Tab.walktomonster.Checked = false;
          client.Tab.assail.Checked = false;
          client.Tab.insectassail.Checked = false;
          client.Tab.wayregionson.Checked = false;
        }
        client.SendMessage(client.yulelogcount, (byte) 18);
      }
      if (obj1.Name == "Copar Neck" && client.digboneseast < 1)
        client.digboneseast = 1;
      else if (obj1.Name == "Copar Bones" && client.digboneseast < 2)
        client.digboneseast = 2;
      else if (obj1.Name == "Copar Wing" && client.digboneseast < 3)
        client.digboneseast = 3;
      else if (obj1.Name == "Copar Leg" && client.digbonesnorth < 1)
        client.digbonesnorth = 1;
      else if (obj1.Name == "Copar Claw" && client.digbonesnorth < 2)
        client.digbonesnorth = 2;
      else if (obj1.Name == "Copar Horn" && client.digbonesnorth < 3)
        client.digbonesnorth = 3;
      else if (obj1.Name == "Copar Tail" && client.digbonesmiddle < 1)
        client.digbonesmiddle = 1;
      else if (obj1.Name == "Copar Skull" && client.digbonesmiddle < 2)
        client.digbonesmiddle = 2;
      else if (obj1.Name == "Copar Ribs" && client.digbonesmiddle < 3)
        client.digbonesmiddle = 3;
      if (client.Tab.vautowalker_locales.Equals("Nobis"))
      {
        if (obj1.Name == "Emerald Eye" && client.MapInfo.Name.Contains("Ruins Altar"))
        {
          client.Tab.walklocaleslist.Text = "4th Summon";
          client.Tab.autowalker_button.Text = "Stop";
          client.autowalkon = true;
        }
        else if (obj1.Name == "Sapphire Eye" && client.MapInfo.Name.Contains("Ruins Altar"))
        {
          client.Tab.walklocaleslist.Text = "3rd Summon";
          client.Tab.autowalker_button.Text = "Stop";
          client.autowalkon = true;
        }
        else if (obj1.Name == "Ruby Eye" && client.MapInfo.Name.Contains("Ruins Altar"))
        {
          client.Tab.walklocaleslist.Text = "2nd Summon";
          client.Tab.autowalker_button.Text = "Stop";
          client.autowalkon = true;
        }
        else if (obj1.Name == "Copar Ribs")
        {
          client.Tab.walklocaleslist.Text = "1st Summon";
          client.Tab.autowalker_button.Text = "Stop";
          client.autowalkon = true;
        }
        else if (obj1.Name == "Copar Horn")
        {
          client.Tab.walklocaleslist.Text = "dig bones (middle)";
          client.Tab.autowalker_button.Text = "Stop";
          client.autowalkon = true;
        }
        else if (obj1.Name == "Copar Wing")
        {
          client.Tab.walklocaleslist.Text = "dig bones (north)";
          client.Tab.autowalker_button.Text = "Stop";
          client.autowalkon = true;
        }
      }
      if (client.MapInfo.Number == 8997)
      {
        if ((int) obj1.Icon - 32768 == 8135)
          client.bug35 = obj1.Amount;
        else if ((int) obj1.Icon - 32768 == 8136)
          client.bug36 = obj1.Amount;
        else if ((int) obj1.Icon - 32768 == 8137)
          client.bug37 = obj1.Amount;
        else if ((int) obj1.Icon - 32768 == 8138)
          client.bug38 = obj1.Amount;
        else if ((int) obj1.Icon - 32768 == 8139)
          client.bug39 = obj1.Amount;
        client.bugcount = client.bug35.ToString() + " - " + client.bug36.ToString() + " - " + client.bug37.ToString() + " - " + client.bug38.ToString() + " - " + client.bug39.ToString();
        client.SendMessage(client.bugcount, (byte) 18);
      }
      else if (client.MapInfo.Number == 8996)
      {
        if ((int) obj1.Icon - 32768 == 8140)
          client.bug40 = obj1.Amount;
        else if ((int) obj1.Icon - 32768 == 8141)
          client.bug41 = obj1.Amount;
        else if ((int) obj1.Icon - 32768 == 8142)
          client.bug42 = obj1.Amount;
        else if ((int) obj1.Icon - 32768 == 8143)
          client.bug43 = obj1.Amount;
        else if ((int) obj1.Icon - 32768 == 8144)
          client.bug44 = obj1.Amount;
        client.bugcount = client.bug40.ToString() + " - " + client.bug41.ToString() + " - " + client.bug42.ToString() + " - " + client.bug43.ToString() + " - " + client.bug44.ToString();
        client.SendMessage(client.bugcount, (byte) 18);
      }
      else if (client.MapInfo.Number == 8999)
      {
        if ((int) obj1.Icon - 32768 == 8125)
          client.bug25 = obj1.Amount;
        else if ((int) obj1.Icon - 32768 == 8126)
          client.bug26 = obj1.Amount;
        else if ((int) obj1.Icon - 32768 == 8127)
          client.bug27 = obj1.Amount;
        else if ((int) obj1.Icon - 32768 == 8128)
          client.bug28 = obj1.Amount;
        else if ((int) obj1.Icon - 32768 == 8129)
          client.bug29 = obj1.Amount;
        client.bugcount = client.bug25.ToString() + " - " + client.bug26.ToString() + " - " + client.bug27.ToString() + " - " + client.bug28.ToString() + " - " + client.bug29.ToString();
        client.SendMessage(client.bugcount, (byte) 18);
      }
      else if (client.MapInfo.Number == 8998)
      {
        if ((int) obj1.Icon - 32768 == 8130)
          client.bug30 = obj1.Amount;
        else if ((int) obj1.Icon - 32768 == 8131)
          client.bug31 = obj1.Amount;
        else if ((int) obj1.Icon - 32768 == 8132)
          client.bug32 = obj1.Amount;
        else if ((int) obj1.Icon - 32768 == 8133)
          client.bug33 = obj1.Amount;
        else if ((int) obj1.Icon - 32768 == 8134)
          client.bug34 = obj1.Amount;
        client.bugcount = client.bug30.ToString() + " - " + client.bug31.ToString() + " - " + client.bug32.ToString() + " - " + client.bug33.ToString() + " - " + client.bug34.ToString();
        client.SendMessage(client.bugcount, (byte) 18);
      }
      return true;
    }

    public bool ServerMessage_0x10_RemoveItem(Client client, ServerPacket msg)
    {
      int num = (int) msg.ReadByte();
      if (num > 0 && client.Tab.recorditemdata.Checked)
      {
        foreach (Character character in client.Characters.Values.ToArray<Character>())
        {
          if (character != null && character.InventorySlot == num)
            character.InventorySlot = int.MinValue;
        }
      }
      if (client.Inventory[num - 1] != null && client.Inventory[num - 1].Name == "Komadium" && !client.HasItem("Komadium"))
        client.SendMessage(client.Name + " is out of Komadiums!", "red", true);
      else if (client.Inventory[num - 1] != null && client.Inventory[num - 1].Name == "Leaf Key" && !client.HasItem("Leaf Key"))
        client.SendMessage("You are out of Leaf Keys", "red");
      else if (client.Inventory[num - 1] != null && client.Inventory[num - 1].Name == "Love Key" && !client.HasItem("Love Key"))
        client.SendMessage("You are out of Love Keys", "red");
      else if (client.Inventory[num - 1] != null && client.Inventory[num - 1].Name == "Red Key" && !client.HasItem("Red Key"))
        client.SendMessage("You are out of Red Keys", "red");
      else if (client.Inventory[num - 1] != null && client.Inventory[num - 1].Name == "Marble Key" && !client.HasItem("Marble Key"))
        client.SendMessage("You are out of Marble Keys", "red");
      if (num > 0)
        client.Inventory[num - 1] = (Item) null;
      return true;
    }

    public bool ServerMessage_0x11_CharacterTurn(Client client, ServerPacket msg)
    {
      uint key = msg.ReadUInt32();
      Direction direction = (Direction) msg.ReadByte();
      if ((int) key == (int) client.PlayerID)
      {
        client.ClientLocation.Direction = direction;
        client.ServerLocation.Direction = direction;
      }
      if (client.Characters.ContainsKey(key) && client.Characters[key] != null)
      {
        if (!client.Characters[key].Moved)
          client.Characters[key].Moved = true;
        client.Characters[key].LastAction = DateTime.UtcNow;
        client.Characters[key].Location.Direction = direction;
      }
      return true;
    }

    public bool ServerMessage_0x13_HpBar(Client client, ServerPacket msg)
    {
      uint key = msg.ReadUInt32();
      int num1 = (int) msg.ReadByte();
      byte num2 = msg.ReadByte();
      int num3 = (int) msg.ReadByte();
      if (client.Characters.ContainsKey(key) && Server.StaticCharacters.ContainsKey(key) && client.Characters[key] != null && Server.StaticCharacters[key] != null)
        Server.StaticCharacters[key].HpAmount = (double) num2;
      if (client.Characters.ContainsKey(key) && client.Characters[key] != null)
      {
        client.Characters[key].Lured = true;
        client.Characters[key].LastAction = DateTime.UtcNow;
        if (client.MainTarget != null && (int) client.MainTarget.ID == (int) key)
          client.follow_walk = 2;
      }
      return true;
    }

    public bool ServerMessage_0x15_MapInfo(Client client, ServerPacket msg)
    {
      string str = string.Empty;
      if (client.MapInfo.Number != int.MinValue)
        client.lastmapnum = client.MapInfo.Number;
      if (client.MapInfo.Name != null)
        str = client.MapInfo.Name;
      client.Towns.Clear();
      client.MapInfo.Number = (int) msg.ReadUInt16();
      client.MapInfo.Width = (int) msg.ReadByte();
      client.MapInfo.Height = (int) msg.ReadByte();
      client.MapInfo.Bitmask = msg.ReadByte();
      int num1 = (int) msg.ReadByte();
      int num2 = (int) msg.ReadByte();
      client.MapInfo.Checksum = msg.ReadUInt16();
      client.MapInfo.Name = msg.ReadString((int) msg.ReadByte());
      if (client.MapInfo.Number == 435 && Server.StaticCharacters.ContainsKey(client.PlayerID))
      {
        client.IsSkulled = false;
        Server.SkullList.Remove(client.Name);
        Server.StaticCharacters[client.PlayerID].IsSkulled = false;
      }
      if (client.MapInfo.Number == 511)
        client.RequestGroupList();
      if (str.Equals("Training Dojo 1") || str.Equals("Training Dojo 2") || str.Equals("Training Dojo 3") || str.Equals("Training Dojo 4") || str.Equals("Training Dojo 5") || str.Equals("Training Dojo 6") || str.Equals("Training Dojo 7") || str.Equals("Training Dojo 8") || str.Equals("Training Dojo 9") && (!str.Equals("Training Dojo 1") && !str.Equals("Training Dojo 2") && !str.Equals("Training Dojo 3") && !str.Equals("Training Dojo 4") && !str.Equals("Training Dojo 5") && !str.Equals("Training Dojo 6") && !str.Equals("Training Dojo 7") && !str.Equals("Training Dojo 8") && !str.Equals("Training Dojo 9") || !client.Tab.dojo.Checked))
      {
        client.Tab.autowalker_locales.SelectedItem = (object) "Mehadi";
        client.Tab.walklocaleslist.SelectedItem = (object) "Entrance";
        client.Tab.autowalker_button.Text = "Stop";
        client.autowalkon = true;
        //if (!client.BotThread.IsAlive)
        //  client.BotThread.Start();
        client.pause = false;
        client.Tab.btnPlay.Enabled = false;
        client.Tab.btnStop.Enabled = true;
      }
      lock (client.Characters)
      {
        foreach (Character character in client.Characters.Values.ToArray<Character>())
        {
          if (character != null && (int) character.ID != (int) client.PlayerID)
            character.IsOnScreen = false;
        }
      }
      client.MapInfo.Initialize();
      int num3 = !(client.MapInfo.Name != "Prairie Circuit") ? 0 : (client.CantSpellMaps.Contains(client.MapInfo.Number) ? 1 : 0);
      client.spellmap = num3 == 0;
      client.skillmap = !client.CantSkillMaps.Contains(client.MapInfo.Number);
      if (!client.safemode && (client.MapInfo.Bitmask == (byte) 3 || client.MapInfo.Bitmask == (byte) 64))
        msg.BodyData[4] = (byte) 0;
      if (client.Tab.disablesnow.Checked && client.MapInfo.Bitmask == (byte) 1)
        msg.BodyData[4] = (byte) 0;
      return true;
    }

    public bool ServerMessage_0x17_AddSpell(Client client, ServerPacket msg)
    {
      Spell spell = new Spell();
      spell.SpellSlot = (int) msg.ReadByte();
      spell.Icon = (int) msg.ReadUInt16();
      spell.Type = (int) msg.ReadByte();
      spell.Name = msg.ReadString((int) msg.ReadByte());
      spell.Prompt = msg.ReadString((int) msg.ReadByte());
      spell.CastLines = (int) msg.ReadByte();
      Match match = Regex.Match(spell.Name, "(.*?)( \\(Lev:)(\\d+)(\\/)(\\d+)(\\))");
      if (match.Success)
      {
        spell.Name = match.Groups[1].Value;
        spell.CurrentLevel = int.Parse(match.Groups[3].Value);
        spell.MaximumLevel = int.Parse(match.Groups[5].Value);
      }
      if (System.IO.File.Exists(Options.DarkAgesDirectoryName + "\\" + client.Name + "\\SpellBook.cfg"))
      {
        StreamReader streamReader = new StreamReader((Stream) System.IO.File.Open(Options.DarkAgesDirectoryName + "\\" + client.Name + "\\SpellBook.cfg", FileMode.Open, FileAccess.Read, FileShare.Read));
        while (!streamReader.EndOfStream)
        {
          if (streamReader.ReadLine().Equals(spell.Name, StringComparison.CurrentCultureIgnoreCase))
          {
            for (int index = 0; index < spell.Captions.Length; ++index)
              spell.Captions[index] = streamReader.ReadLine().Split(':')[1];
          }
        }
        streamReader.Close();
      }
      client.SpellBook[spell.SpellSlot - 1] = spell;
      if (spell.Name.Contains("Prayer"))
        client.PrayerSpell = spell.Name;
      if (spell.Name != "nis" && spell.Name != "Learning Spell")
      {
        if (client.Tab.MacroOptions.macrospellslistview.Items.ContainsKey(spell.Name) && client.Tab.MacroOptions.macrospellslistview.Items[spell.Name] != null && spell.CurrentLevel < int.Parse(client.Tab.MacroOptions.macrospellslistview.Items[spell.Name].SubItems[2].Text))
        {
          client.Tab.MacroOptions.macrospellslistview.Items[spell.Name].SubItems[1].Text = spell.CurrentLevel.ToString();
          client.SaveMacroList();
        }
        else if (client.Tab.MacroOptions.macrospellslistview.Items.ContainsKey(spell.Name) && client.Tab.MacroOptions.macrospellslistview.Items[spell.Name] != null && spell.CurrentLevel >= int.Parse(client.Tab.MacroOptions.macrospellslistview.Items[spell.Name].SubItems[2].Text))
        {
          client.Tab.MacroOptions.macrospellslistview.Items[spell.Name].Remove();
          client.SaveMacroList();
        }
      }
      if (client.Tab.SkillSwap.spelltemlist.Items.ContainsKey(spell.SpellSlot.ToString()))
      {
        if (client.Tab.SkillSwap.spelltemlist.Items[spell.SpellSlot.ToString()].SubItems.Count > 1)
          client.Tab.SkillSwap.spelltemlist.Items[spell.SpellSlot.ToString()].SubItems[1].Text = spell.Name;
        else
          client.Tab.SkillSwap.spelltemlist.Items[spell.SpellSlot.ToString()].SubItems.Add(spell.Name);
      }
      else if (spell.SpellSlot > 36 && client.Tab.SkillSwap.spellmedlist.Items.ContainsKey((spell.SpellSlot - 36).ToString()))
      {
        if (client.Tab.SkillSwap.spellmedlist.Items[(spell.SpellSlot - 36).ToString()].SubItems.Count > 1)
          client.Tab.SkillSwap.spellmedlist.Items[(spell.SpellSlot - 36).ToString()].SubItems[1].Text = spell.Name;
        else
          client.Tab.SkillSwap.spellmedlist.Items[(spell.SpellSlot - 36).ToString()].SubItems.Add(spell.Name);
      }
      return true;
    }

    public bool ServerMessage_0x18_RemoveSpell(Client client, ServerPacket msg)
    {
      int num = (int) msg.ReadByte();
      if (num > 0)
        client.SpellBook[num - 1] = (Spell) null;
      if (client.Tab.SkillSwap.spelltemlist.Items.ContainsKey(num.ToString()))
      {
        if (client.Tab.SkillSwap.spelltemlist.Items[num.ToString()].SubItems.Count > 1)
          client.Tab.SkillSwap.spelltemlist.Items[num.ToString()].SubItems[1].Text = "";
      }
      else if ((num <= 36 ? 0 : (client.Tab.SkillSwap.spellmedlist.Items.ContainsKey((num - 36).ToString()) ? 1 : 0)) != 0 && client.Tab.SkillSwap.spellmedlist.Items[(num - 36).ToString()].SubItems.Count > 1)
        client.Tab.SkillSwap.spellmedlist.Items[(num - 36).ToString()].SubItems[1].Text = "";
      return true;
    }

    public bool ServerMessage_0x19_SoundEffect(Client client, ServerPacket msg)
    {
      if (msg.ReadByte() == byte.MaxValue)
      {
        if (client.anttunnels == 1)
        {
          client.anttunnel = DateTime.Now;
          client.SaveTimedStuff(23);
        }
        else if (client.guardiananttunnels == 1)
        {
          client.guardiananttunnel = DateTime.Now;
          client.SaveTimedStuff(24);
        }
      }
      return true;
    }

    public bool ServerMessage_0x1A_BodyAnimation(Client client, ServerPacket msg)
    {
      uint key = msg.ReadUInt32();
      if (client.Characters.ContainsKey(key) && client.Characters[key] != null)
      {
        if (!client.Characters[key].Moved)
          client.Characters[key].Moved = true;
        client.Characters[key].LastAction = DateTime.UtcNow;
      }
      return (client.safemode || !client.Tab.vdisableallbody || (int) key != (int) client.PlayerID) && !client.Tab.disableitemsnstuff.Checked;
    }

    public bool ServerMessage_0x1F_NewMap(Client client, ServerPacket msg)
    {
      client.tocpopup = false;
      client.IsSkulled = false;
      client.checkedtiles.Clear();
      foreach (MappedMaps mappedMaps in client.AutoWalkMaps.Values)
        mappedMaps.Deadend = false;
      client.CurAWDest = (Location) null;
      client.HasAWPath = false;
      if (client.Tab.recorditemdata.Checked && ((IEnumerable<Form>) Program.MainForm.ItemXMLEditor.MdiChildren).Count<Form>() > 0)
      {
        foreach (Form mdiChild in Program.MainForm.ItemXMLEditor.MdiChildren)
        {
          Form f = mdiChild;
          if (f != null && f is ItemMapXMLEditorChild && (f as ItemMapXMLEditorChild).charname != null && (f as ItemMapXMLEditorChild).charname.Text == client.Name)
            Program.MainForm.BeginInvoke((Action) (() =>
            {
              if (client.Tab.all5classes.Checked)
                (f as ItemMapXMLEditorChild).Save(client.Name, true);
              else
                (f as ItemMapXMLEditorChild).Save(client.Name);
            }));
        }
      }
      if (client.ytquest == 15 && client.MapInfo.Number == 8349 && client.HasItem("Yowien Fishing Pole"))
      {
        client.Assail();
        client.UseItem("Yowien Fishing Pole");
        client.SendMessage("Search for house key when you get to YT12");
      }
      client.newmapdelay = DateTime.UtcNow;
      client.ignorewaitatdoors = true;
      MapNum mapNum = new MapNum();
      mapNum.Number = client.MapInfo.Number;
      if (!client.TempRegions.ContainsKey(client.MapInfo.Number))
        client.TempRegions.Add(client.MapInfo.Number, mapNum);
      client.Tab.Wayregion.map.Text = client.MapInfo.Number.ToString() + " - " + client.MapInfo.Name;
      client.randomdest = (Location) null;
      client.Previous.Clear();
      if (!client.LastPermMessage.Contains("You are blind") && !client.LastPermMessage.Contains("You have Gifts!") && !client.MapInfo.Name.Contains("Water Dungeon") && !client.MapInfo.Name.Contains("Veltain Mine") && !client.MapInfo.Name.Contains("Canal") && !client.MapInfo.Name.Equals("Yowien Territory12") && !client.MapInfo.Name.Equals("Yowien Territory13") && !client.MapInfo.Name.Equals("Yowien Territory14") && !client.MapInfo.Name.Equals("Yowien Territory24") && client.MapInfo.Number != 8308 && !client.MapInfo.Name.Equals("Balanced Arena") && client.MapInfo.Number != 8375 && client.MapInfo.Number != 8376)
        client.SendMessage("", (byte) 18);
      if (client.MapInfo.Name.Equals("Water Dungeon 15") || client.MapInfo.Name.Equals("Veltain Mine 1") || client.MapInfo.Name.Equals("Veltain Mine 5") || client.MapInfo.Name.Equals("Veltain Mine 6") || client.MapInfo.Name.Equals("Veltain Mine 7") || client.MapInfo.Number == 7403 || client.MapInfo.Number == 9377 || client.MapInfo.Number == 9385 || client.MapInfo.Number == 9386 || client.MapInfo.Number == 11363)
        client.SendMessage("", (byte) 18);
      if (client.MapInfo.Number == 2141 && client.Tab.pigwalk.Checked && !client.Tab.wayregionson.Checked)
      {
        client.Tab.Wayregion.LoadMapPack(Program.StartupPath + "\\Settings\\MapPacks\\pigmaze.xml");
        client.Tab.actonlyinmobs.Checked = true;
        client.Tab.wayregionson.Checked = true;
      }
      if (client.MapInfo.Name == "Mount Giragan 1" || client.MapInfo.Name == "Mount Giragan 2" || client.MapInfo.Name == "Mount Giragan 3")
      {
        if (client.HasItem("Cedar Log"))
          client.cedarlogs = client.ItemAmount("Cedar Log");
        if (client.HasItem("Fir Log"))
          client.firlogs = client.ItemAmount("Fir Log");
        client.yulelogcount = "{=bC " + client.cedarlogs.ToString() + ", F " + client.firlogs.ToString();
        client.SendMessage(client.yulelogcount, (byte) 18);
      }
      if (client.HasItem("Yule Log") && client.HasItem("fior srad"))
      {
        if ((client.MapInfo.Number == 3079 || client.MapInfo.Number == 3006 || client.MapInfo.Number == 500) && !client.yulemileth)
        {
          client.Tab.autowalker_locales.Text = "Mileth";
          client.Tab.walklocaleslist.SelectedItem = (object) "Inn";
          client.Tab.autowalker_button.Text = "Stop";
          client.autowalkon = true;
          client.Tab.fastwalk.Checked = true;
          client.pause = false;
          client.Tab.btnPlay.Enabled = false;
          client.Tab.btnStop.Enabled = true;
        }
        if (client.MapInfo.Number == 502 && !client.yuleabel)
        {
          client.Tab.autowalker_locales.Text = "Abel";
          client.Tab.walklocaleslist.SelectedItem = (object) "Inn";
          client.Tab.autowalker_button.Text = "Stop";
          client.autowalkon = true;
          client.Tab.fastwalk.Checked = true;
          client.pause = false;
          client.Tab.btnPlay.Enabled = false;
          client.Tab.btnStop.Enabled = true;
        }
        if ((client.MapInfo.Number == 3020 || client.MapInfo.Number == 501) && !client.yulepiet)
        {
          client.Tab.autowalker_locales.Text = "Piet";
          client.Tab.walklocaleslist.SelectedItem = (object) "Inn";
          client.Tab.autowalker_button.Text = "Stop";
          client.autowalkon = true;
          client.Tab.fastwalk.Checked = true;
          client.pause = false;
          client.Tab.btnPlay.Enabled = false;
          client.Tab.btnStop.Enabled = true;
        }
        if ((client.MapInfo.Number == 3081 || client.MapInfo.Number == 505) && !client.yuleruc)
        {
          client.Tab.autowalker_locales.Text = "Rucesion";
          client.Tab.walklocaleslist.SelectedItem = (object) "Inn";
          client.Tab.autowalker_button.Text = "Stop";
          client.autowalkon = true;
          client.Tab.fastwalk.Checked = true;
          client.pause = false;
          client.Tab.btnPlay.Enabled = false;
          client.Tab.btnStop.Enabled = true;
        }
        if (client.MapInfo.Number == 662 && !client.yuletagor)
        {
          client.Tab.autowalker_locales.Text = "Tagor";
          client.Tab.walklocaleslist.SelectedItem = (object) "Inn";
          client.Tab.autowalker_button.Text = "Stop";
          client.autowalkon = true;
          client.Tab.fastwalk.Checked = true;
          client.pause = false;
          client.Tab.btnPlay.Enabled = false;
          client.Tab.btnStop.Enabled = true;
        }
        if ((client.MapInfo.Number == 503 || client.MapInfo.Number == 3016) && !client.yulesuomi)
        {
          client.Tab.autowalker_locales.Text = "Suomi";
          client.Tab.walklocaleslist.SelectedItem = (object) "Inn";
          client.Tab.autowalker_button.Text = "Stop";
          client.autowalkon = true;
          client.Tab.fastwalk.Checked = true;
          client.pause = false;
          client.Tab.btnPlay.Enabled = false;
          client.Tab.btnStop.Enabled = true;
        }
      }
      if (client.MapInfo.Number == 7401 && (client.previousmap == 7412 || client.previousmap == 7413))
        client.SaveTimedStuff(1);
      if (client.MapInfo.Number == 7401 && client.previousmap == 7405)
        client.SaveTimedStuff(2);
      if (client.MapInfo.Number == 7431 && client.previousmap == 7432)
        client.SaveTimedStuff(3);
      if (client.MapInfo.Number == 7433 && client.previousmap == 7403)
        client.SaveTimedStuff(4);
      if (client.MapInfo.Number == 7401 && client.previousmap == 7436)
        client.SaveTimedStuff(5);
      if (client.MapInfo.Number == 2901 && client.previousmap == 2905)
        client.SaveTimedStuff(6);
      if (client.MapInfo.Number == 2901 && client.previousmap == 2906)
        client.SaveTimedStuff(7);
      if (client.MapInfo.Number == 2901 && client.previousmap == 2907)
        client.SaveTimedStuff(8);
      if (client.MapInfo.Number == 10038 && client.previousmap == 10240)
      {
        client.SaveTimedStuff(9);
        client.pause = true;
        client.Tab.btnPlay.Enabled = true;
        client.Tab.btnStop.Enabled = false;
        client.Tab.LoadTemplate("default");
      }
      if (client.MapInfo.Number == 9378 && (client.previousmap == 9377 || client.previousmap == 9385 || client.previousmap == 9386))
      {
        client.SaveTimedStuff(43);
        client.pause = true;
        client.Tab.btnPlay.Enabled = true;
        client.Tab.btnStop.Enabled = false;
        client.Tab.LoadTemplate("default");
      }
      if (client.previousmap == 11363 && (client.MapInfo.Number == 11300 || client.MapInfo.Number == 10004 || client.MapInfo.Number == 10056 || client.MapInfo.Number == 136 || client.MapInfo.Number == 498 || client.MapInfo.Number == 3079 || client.MapInfo.Number == 950 || client.MapInfo.Number == 1960))
      {
        client.SaveTimedStuff(44);
        client.pause = true;
        client.Tab.btnPlay.Enabled = true;
        client.Tab.btnStop.Enabled = false;
        client.Tab.LoadTemplate("default");
      }
      if (client.MapInfo.Number == 136 && client.previousmap == 8446)
      {
        client.SaveTimedStuff(45);
        client.pause = true;
        client.Tab.btnPlay.Enabled = true;
        client.Tab.btnStop.Enabled = false;
        client.Tab.LoadTemplate("default");
      }
      if (client.MapInfo.Number == 3210 && client.previousmap == 8120)
      {
        client.SaveTimedStuff(46);
        client.pause = true;
        client.Tab.btnPlay.Enabled = true;
        client.Tab.btnStop.Enabled = false;
        client.Tab.LoadTemplate("default");
      }
      if (client.MapInfo.Number == 701 && client.previousmap == 6932)
      {
        client.pause = true;
        client.Tab.btnPlay.Enabled = true;
        client.Tab.btnStop.Enabled = false;
      }
      if (client.MapInfo.Name == "Road to House Macabre" && client.previousmapname.Contains("Macabre Grave Yard"))
      {
        client.pause = true;
        client.Tab.btnPlay.Enabled = true;
        client.Tab.btnStop.Enabled = false;
      }
      client.previousmap = client.MapInfo.Number;
      if (client.MapInfo.Number == 8997)
      {
        foreach (Item obj in client.Inventory)
        {
          if (obj != null)
          {
            if ((int) obj.Icon - 32768 == 8135)
              client.bug35 = obj.Amount;
            else if ((int) obj.Icon - 32768 == 8136)
              client.bug36 = obj.Amount;
            else if ((int) obj.Icon - 32768 == 8137)
              client.bug37 = obj.Amount;
            else if ((int) obj.Icon - 32768 == 8138)
              client.bug38 = obj.Amount;
            else if ((int) obj.Icon - 32768 == 8139)
              client.bug39 = obj.Amount;
          }
        }
        client.bugcount = client.bug35.ToString() + " - " + client.bug36.ToString() + " - " + client.bug37.ToString() + " - " + client.bug38.ToString() + " - " + client.bug39.ToString();
        client.SendMessage(client.bugcount, (byte) 18);
      }
      if (client.MapInfo.Number == 8996)
      {
        foreach (Item obj in client.Inventory)
        {
          if (obj != null)
          {
            if ((int) obj.Icon - 32768 == 8140)
              client.bug40 = obj.Amount;
            else if ((int) obj.Icon - 32768 == 8141)
              client.bug41 = obj.Amount;
            else if ((int) obj.Icon - 32768 == 8142)
              client.bug42 = obj.Amount;
            else if ((int) obj.Icon - 32768 == 8143)
              client.bug43 = obj.Amount;
            else if ((int) obj.Icon - 32768 == 8144)
              client.bug44 = obj.Amount;
          }
        }
        client.bugcount = client.bug40.ToString() + " - " + client.bug41.ToString() + " - " + client.bug42.ToString() + " - " + client.bug43.ToString() + " - " + client.bug44.ToString();
        client.SendMessage(client.bugcount, (byte) 18);
      }
      if (client.MapInfo.Number == 8999)
      {
        foreach (Item obj in client.Inventory)
        {
          if (obj != null)
          {
            if ((int) obj.Icon - 32768 == 8125)
              client.bug25 = obj.Amount;
            else if ((int) obj.Icon - 32768 == 8126)
              client.bug26 = obj.Amount;
            else if ((int) obj.Icon - 32768 == 8127)
              client.bug27 = obj.Amount;
            else if ((int) obj.Icon - 32768 == 8128)
              client.bug28 = obj.Amount;
            else if ((int) obj.Icon - 32768 == 8129)
              client.bug29 = obj.Amount;
          }
        }
        client.bugcount = client.bug25.ToString() + " - " + client.bug26.ToString() + " - " + client.bug27.ToString() + " - " + client.bug28.ToString() + " - " + client.bug29.ToString();
        client.SendMessage(client.bugcount, (byte) 18);
      }
      if (client.MapInfo.Number == 8998)
      {
        foreach (Item obj in client.Inventory)
        {
          if (obj != null)
          {
            if ((int) obj.Icon - 32768 == 8130)
              client.bug30 = obj.Amount;
            else if ((int) obj.Icon - 32768 == 8131)
              client.bug31 = obj.Amount;
            else if ((int) obj.Icon - 32768 == 8132)
              client.bug32 = obj.Amount;
            else if ((int) obj.Icon - 32768 == 8133)
              client.bug33 = obj.Amount;
            else if ((int) obj.Icon - 32768 == 8134)
              client.bug34 = obj.Amount;
          }
        }
        client.bugcount = client.bug30.ToString() + " - " + client.bug31.ToString() + " - " + client.bug32.ToString() + " - " + client.bug33.ToString() + " - " + client.bug34.ToString();
        client.SendMessage(client.bugcount, (byte) 18);
      }

      // Code by Avi
      client.monsterKills.Reset(client.MapInfo.Number);
      client.monsterKills.Display(client.MapInfo.Name);
      //

      return true;
    }

    public bool ServerMessage_0x29_SpellAnimation(Client client, ServerPacket msg)
    {
      ushort num1 = 0;
      uint key1 = 0;
      uint key2 = msg.ReadUInt32();
      ushort key3;
      ushort num2;
      if (key2 > 0U)
      {
        key1 = msg.ReadUInt32();
        key3 = msg.ReadUInt16();
        num1 = msg.ReadUInt16();
        num2 = msg.ReadUInt16();
      }
      else
      {
        key3 = msg.ReadUInt16();
        num2 = msg.ReadUInt16();
        int num3 = (int) msg.ReadUInt16();
        int num4 = (int) msg.ReadUInt16();
        int num5 = (int) msg.ReadByte();
      }
      if (client.Characters.ContainsKey(key1) && client.Characters[key1] is Player && client.Characters.ContainsKey(key2) && client.Characters[key2] is Npc && Server.StaticCharacters.ContainsKey(key2))
      {
        if ((int) key1 == (int) client.PlayerID && (key3 == (ushort) 254 || key3 == (ushort) 137))
        {
          if (!Server.StaticCharacters[key2].hasdion && !Server.StaticCharacters[key2].hasmonsterdion && Server.StaticCharacters[key2].hascurse)
            ++client.Characters[key2].HitCount;
          if ((Decimal) client.Characters[key2].HitCount >= client.Tab.brieshits.Value)
          {
            client.Characters[key2].HitCount = 0UL;
            Server.StaticCharacters[key2].CantAttack = true;
          }
        }
        if (key3 == (ushort) 25)
          Server.StaticCharacters[key2].CantAttack = false;
      }
      if ((key3 == (ushort) 21 || key3 == (ushort) 22) && (int) key2 == (int) client.PlayerID && client.MapInfo.Number == 421 && !client.Tab.vimpskillbutton && Program.MainForm.laborname.Text != string.Empty && Program.MainForm.getmentored.Checked && Program.MainForm.laborname.Text.ToLower() != client.Name.ToLower())
      {
        client.SaveTimedStuff(34);
        client.LogOff();
      }
      if (key3 == (ushort) 3 && (int) key2 == (int) client.PlayerID && client.Tab.vautowalker_locales.Equals("Aman Jungle"))
      {
        if (client.ytquest == 1)
          client.YTQuestStep("YT 3");
        if (client.ytquest == 2)
          client.YTQuestStep("CC 7");
        if (client.ytquest == 3)
          client.YTQuestStep("YT 3");
        if (client.ytquest == 4)
          client.YTQuestStep("CC 7");
        if (client.ytquest == 5)
          client.YTQuestStep("YT 6");
        if (client.ytquest == 7)
          client.YTQuestStep("CC 7");
        if (client.ytquest == 8)
          client.YTQuestStep("YT 3");
        if (client.ytquest == 9)
          client.YTQuestStep("CC 7");
        if (client.ytquest == 10)
        {
          if (client.HasItem("Crystal Orb"))
            client.YTQuestStep("YT 11");
          else
            client.SendMessage("Get a Crystal Orb and fresh hide!", "red");
        }
        if (client.ytquest == 15)
        {
          if (client.MapInfo.Number == 8349 && client.HasItem("Yowien Fishing Pole") && client.ServerLocation.Y > 3)
          {
            client.Assail();
            client.UseItem("Yowien Fishing Pole");
          }
          else
            client.YTQuestStep("YT 12");
        }
        if (client.ytquest == 70)
        {
          if (client.ItemAmount("Dendron Flower") >= 20U)
            client.YTQuestStep("YT 6");
          else
            client.SendMessage("Get more Dendron Flowers and fresh hide!", "red");
        }
      }
      if (client.Characters.ContainsKey(key1) && client.Characters[key1] != null)
      {
        client.Characters[key1].LastAction = DateTime.UtcNow;
        if (!client.Characters[key1].Moved)
          client.Characters[key1].Moved = true;
      }
      if (client.Characters.ContainsKey(key2) && client.Characters[key2] != null)
        client.Characters[key2].LastAction = DateTime.UtcNow;
      lock (Server.StaticCharacters)
      {
        if (client.Characters.ContainsKey(key2) && Server.StaticCharacters.ContainsKey(key2) && client.Characters[key2] != null && Server.StaticCharacters[key2] != null)
        {
          if (client.LastSpell == "asgall faileas" && (int) key2 == (int) client.PlayerID && (key3 == (ushort) 66 || num1 == (ushort) 66))
            client.asgalltime = DateTime.UtcNow;
          if (client.Tab.MonstersByPlayer != null && (int) key1 == (int) client.playeridformonster)
            client.trackedmonsterID = key2;
          if (!Server.StaticCharacters[key2].SpellAnimationHistory.ContainsKey((int) key3))
            Server.StaticCharacters[key2].SpellAnimationHistory.Add((int) key3, DateTime.UtcNow);
          else
            Server.StaticCharacters[key2].SpellAnimationHistory[(int) key3] = DateTime.UtcNow;
          if ((int) key1 == (int) client.PlayerID && client.LastSpell != string.Empty)
          {
            if (!Server.StaticCharacters[key2].ProbableSpellType.ContainsKey((int) key3))
              Server.StaticCharacters[key2].ProbableSpellType.Add((int) key3, client.LastSpell);
            else
              Server.StaticCharacters[key2].ProbableSpellType[(int) key3] = client.LastSpell;
            if (num1 == (ushort) 192)
              client.cttime = DateTime.UtcNow;
          }
          if (client.Characters.ContainsKey(key1) && Server.StaticCharacters.ContainsKey(key1) && client.Characters[key1] != null && Server.StaticCharacters[key1] != null)
          {
            if (key3 == (ushort) 84 && client.altsneedflowered.Count > 0 && Server.StaticCharacters[key2].Name == client.altsneedflowered[0])
              client.altsneedflowered.Remove(Server.StaticCharacters[key2].Name);
            if (key3 == (ushort) 84 && Server.StaticCharacters[key2].wantsflowered)
              Server.StaticCharacters[key2].wantsflowered = false;
            if ((int) key1 == (int) client.PlayerID && key3 != (ushort) 33 && key3 != (ushort) 32 && key3 != (ushort) 40 && key3 != (ushort) 117 && key3 != (ushort) 257 && key3 != (ushort) 259 && key3 != (ushort) 104 && key3 != (ushort) 243 && key3 != (ushort) 258 && key3 != (ushort) 82 && key3 != (ushort) 75 && key3 != (ushort) 273)
            {
              client.Characters[key2].Lured = true;
              if (client.MainTarget != null && (int) client.MainTarget.ID == (int) key2)
                client.follow_walk = 2;
            }
            if (key3 != (ushort) 33 && key3 != (ushort) 32 && key3 != (ushort) 40 && key3 != (ushort) 117 && key3 != (ushort) 257 && key3 != (ushort) 259 && key3 != (ushort) 104 && key3 != (ushort) 243 && key3 != (ushort) 258 && key3 != (ushort) 82 && key3 != (ushort) 75 && key3 != (ushort) 273 && key3 != (ushort) 235)
              client.Characters[key2].LastHitByID = key1;
            if ((int) key1 == (int) client.PlayerID && (key3 == (ushort) 25 || key3 == (ushort) 274))
              client.Characters[key2].LastHitByID = 0U;
            if (Server.StaticCharacters[key1] is Npc && Server.StaticCharacters[key2] is Player && !Server.StaticCharacters[key2].AnimationFrom.ContainsKey((int) key3))
              Server.StaticCharacters[key2].AnimationFrom.Add((int) key3, (Server.StaticCharacters[key1] as Npc).Image);
            if (Server.StaticCharacters[key1] is Player && key3 == (ushort) 48)
            {
              if (!Server.StaticCharacters[key1].SpellAnimationHistory.ContainsKey((int) key3))
                Server.StaticCharacters[key1].SpellAnimationHistory.Add((int) key3, DateTime.UtcNow);
              else
                Server.StaticCharacters[key1].SpellAnimationHistory[(int) key3] = DateTime.UtcNow;
            }
            if (Server.StaticCharacters[key1] is Player && (int) key1 != (int) client.PlayerID)
            {
              if (key3 == (ushort) 19 && num1 != (ushort) 19 && (int) key2 == (int) client.PlayerID && client.MapInfo.Name.StartsWith("Training Dojo "))
              {
                client.throwss = true;
                client.throwername = Server.StaticCharacters[key1].Name;
              }
              if (!Server.StaticCharacters[key2].ProbableSpellType.ContainsKey((int) key3))
                Server.StaticCharacters[key2].ProbableSpellType.Add((int) key3, Server.StaticCharacters[key1].LastBlueText);
              else
                Server.StaticCharacters[key2].ProbableSpellType[(int) key3] = Server.StaticCharacters[key1].LastBlueText;
            }
            if (key3 == (ushort) 161 || key3 == (ushort) 162)
              Server.StaticCharacters[key1].IsCupping = true;
            if ((int) key1 != (int) key2 && key3 == (ushort) 5)
            {
              Server.StaticCharacters[key2].SpellAnimationHistory.Remove(24);
              Server.StaticCharacters[key2].IsSkulled = false;
            }
            if ((int) key1 == (int) key2 && key3 == (ushort) 245 && client.Characters[key1] is Npc && Server.StaticCharacters[key1].hasardcradh && !client.MapInfo.Name.Contains("Oren Ruin") && !client.MapInfo.Name.Contains("Shinewood") && !client.MapInfo.Name.Contains("Ice Cave"))
              Server.StaticCharacters[key2].SpellAnimationHistory.Remove(257);
            else if (Server.StaticCharacters[key2] is Npc && (int) key1 == (int) key2 && key3 == (ushort) 232 && !client.MapInfo.Name.Contains("Oren Ruin"))
              Server.StaticCharacters[key2].SpellAnimationHistory.Clear();
            else if (Server.StaticCharacters[key1] is Npc && Server.StaticCharacters[key2] is Player && key3 == (ushort) 232)
              Server.StaticCharacters[key2].SpellAnimationHistory.Clear();
            if (Server.StaticCharacters[key1] is Npc && key3 == (ushort) 1)
              Server.StaticCharacters[key1].isParentGrime = true;
          }
          if (!Server.StaticCharacters[key2].hasdion && !Server.StaticCharacters[key2].hasmonsterdion && key3 != (ushort) 117 && key3 != (ushort) 41 && key3 != (ushort) 32 && key3 != (ushort) 42 && key3 != (ushort) 40 && key3 != (ushort) 259 && key3 != (ushort) 258 && key3 != (ushort) 243 && key3 != (ushort) 257 && key3 != (ushort) 104 && key3 != (ushort) 82 && key3 != (ushort) 75 && key3 != (ushort) 231 && key3 != (ushort) 273 && key3 != (ushort) 263 && key3 != (ushort) 278 && key3 != (ushort) 245 && key3 != (ushort) 44 && key3 != (ushort) 25 && key3 != (ushort) 247 && key3 != (ushort) 295 && key3 != (ushort) 33)
          {
            Server.StaticCharacters[key2].SpellAnimationHistory.Remove(32);
            Server.StaticCharacters[key2].SpellAnimationHistory.Remove(117);
          }
          if (client.Tab.AscendOptions.rescuername.Text != string.Empty)
          {
            Player characterByName = client.FindCharacterByName<Player>(client.Tab.AscendOptions.rescuername.Text);
            if (characterByName != null && characterByName.IsOnScreen && (int) key1 == (int) characterByName.ID && key3 == (ushort) 274)
              client.rescuedtime = DateTime.UtcNow;
          }
          if (!client.safemode && client.Characters[key2] is Player && (int) key2 != (int) client.PlayerID)
          {
            if (client.Tab.vmonitordion && (key3 == (ushort) 244 || key3 == (ushort) 66 || key3 == (ushort) 89 || key3 == (ushort) 93 || key3 == (ushort) 86))
              client.UpdatePlayerImage(client.Characters[key2] as Player);
            else if (client.Tab.vmonitorcurses && (key3 == (ushort) 245 || key3 == (ushort) 257))
              client.UpdatePlayerImage(client.Characters[key2] as Player);
            else if (client.Tab.monitords.Checked && (key3 == (ushort) 245 || key3 == (ushort) 104 || key3 == (ushort) 82 || key3 == (ushort) 75))
              client.UpdatePlayerImage(client.Characters[key2] as Player);
            else if (client.Tab.vmonitorspells && (key3 == (ushort) 231 || key3 == (ushort) 273))
              client.UpdatePlayerImage(client.Characters[key2] as Player);
          }
        }
        if (client.Characters.ContainsKey(key2) && client.Characters[key2] != null && client.Characters[key2] is Npc && (int) key1 == (int) client.PlayerID)
          client.LastTarget = key2;
        if (!client.safemode && client.Tab.vdisableallspell && key3 != (ushort) 99 && key3 != (ushort) 24 && key3 != (ushort) 5 && key3 != (ushort) 191 && key3 != (ushort) 112 && key3 != (ushort) 212 && key3 != (ushort) 213 && key3 != (ushort) 362 && key3 != (ushort) 214 && key3 != (ushort) 96)
          return false;
        if (msg.Length > (ushort) 13)
        {
          if (!client.safemode)
          {
            if (MainForm.voldanim)
            {
              bool flag = false;
              ushort num6 = key3;
              int num7;
              switch (key3)
              {
                case 231:
                  num6 = (ushort) 21;
                  flag = true;
                  goto label_149;
                case 243:
                  num6 = (ushort) 18;
                  flag = true;
                  goto label_149;
                case 245:
                  num6 = !client.Characters.ContainsKey(key2) || !Server.StaticCharacters.ContainsKey(key2) ? (ushort) 8 : (!(client.LastSpell == "ao ard cradh") ? (!(client.LastSpell == "ao mor cradh") ? (!(client.LastSpell == "ao cradh") ? (!(client.LastSpell == "ao beag cradh") ? (!Server.StaticCharacters[key2].hasardcradh ? (Server.StaticCharacters[key2].hascradh || Server.StaticCharacters[key2].hasbardo ? (ushort) 38 : (!Server.StaticCharacters[key2].hasbeagcradh ? (ushort) 8 : (ushort) 39)) : (ushort) 37) : (ushort) 39) : (ushort) 38) : (ushort) 8) : (ushort) 37);
                  flag = true;
                  goto label_149;
                case 254:
                  num6 = (ushort) 52;
                  flag = true;
                  goto label_149;
                case 257:
                  num6 = (ushort) 43;
                  flag = true;
                  goto label_149;
                case 258:
                  num6 = (ushort) 44;
                  flag = true;
                  goto label_149;
                case 259:
                  num6 = (ushort) 45;
                  flag = true;
                  goto label_149;
                case 273:
                  num6 = (ushort) 23;
                  flag = true;
                  goto label_149;
                case 279:
                  num7 = 1;
                  break;
                default:
                  num7 = key3 == (ushort) 232 ? 1 : 0;
                  break;
              }
              if (num7 != 0)
              {
                num6 = (ushort) 2;
                flag = true;
              }
              else
              {
                switch (key3)
                {
                  case 234:
                    num6 = client.LastSpell == "sal" || client.LastSpell == "beag sal" || client.LastSpell == "beag sal lamh" || client.LastSpell == "sal lamh" ? (ushort) 9 : (ushort) 10;
                    flag = true;
                    break;
                  case 235:
                    num6 = (ushort) 11;
                    flag = true;
                    break;
                  case 237:
                    num6 = (ushort) 12;
                    flag = true;
                    break;
                  case 238:
                    num6 = (ushort) 13;
                    flag = true;
                    break;
                  case 239:
                    num6 = (ushort) 14;
                    flag = true;
                    break;
                  case 240:
                    num6 = (ushort) 15;
                    flag = true;
                    break;
                  case 241:
                    num6 = (ushort) 16;
                    flag = true;
                    break;
                  case 242:
                    num6 = (ushort) 17;
                    flag = true;
                    break;
                  case 244:
                    num6 = (ushort) 6;
                    flag = true;
                    break;
                  case 250:
                    num6 = (ushort) 29;
                    flag = true;
                    break;
                  case 251:
                    num6 = (ushort) 30;
                    flag = true;
                    break;
                  case 252:
                    num6 = (ushort) 31;
                    flag = true;
                    break;
                  case 263:
                    num6 = (ushort) 81;
                    flag = true;
                    break;
                  case 267:
                    num6 = (ushort) 4;
                    flag = true;
                    break;
                  case 271:
                    num6 = (ushort) 121;
                    flag = true;
                    break;
                  case 280:
                    num6 = (ushort) 19;
                    flag = true;
                    break;
                }
              }
            label_149:
              if ((int)key1 == (int)client.PlayerID && Server.StaticCharacters.ContainsKey(key2) && (key3 == (ushort)245 || key3 == (ushort)243 || key3 == (ushort)258 || key3 == (ushort)259 || key3 == (ushort)104 || key3 == (ushort)82))
              {
                int num8;
                if (Server.StaticCharacters[key2].SpellAnimationHistory.ContainsKey(257))
                {
                  switch (key3)
                  {
                    case 245:
                      num8 = client.LastSpell == "ao ard cradh" ? 1 : 0;
                      goto label_155;
                    case 257:
                      break;
                    default:
                      num8 = key3 != (ushort)245 ? 1 : 0;
                      goto label_155;
                  }
                }
                num8 = 0;
              label_155:
                if (num8 != 0)
                  Server.StaticCharacters[key2].SpellAnimationHistory.Remove(257);
                if ((!Server.StaticCharacters[key2].SpellAnimationHistory.ContainsKey(243) || key3 == (ushort)243 ? 0 : (key3 != (ushort)245 ? (key3 != (ushort)245 ? 1 : 0) : (client.LastSpell == "ao mor cradh" ? 1 : 0))) != 0)
                  Server.StaticCharacters[key2].SpellAnimationHistory.Remove(243);
                int num9;
                if (Server.StaticCharacters[key2].SpellAnimationHistory.ContainsKey(258))
                {
                  switch (key3)
                  {
                    case 245:
                      num9 = client.LastSpell == "ao cradh" ? 1 : 0;
                      goto label_164;
                    case 258:
                      break;
                    default:
                      num9 = key3 != (ushort)245 ? 1 : 0;
                      goto label_164;
                  }
                }
                num9 = 0;
              label_164:
                if (num9 != 0)
                  Server.StaticCharacters[key2].SpellAnimationHistory.Remove(258);
                int num10;
                if (Server.StaticCharacters[key2].SpellAnimationHistory.ContainsKey(259))
                {
                  switch (key3)
                  {
                    case 245:
                      num10 = client.LastSpell == "ao beag cradh" ? 1 : 0;
                      goto label_171;
                    case 259:
                      break;
                    default:
                      num10 = key3 != (ushort)245 ? 1 : 0;
                      goto label_171;
                  }
                }
                num10 = 0;
              label_171:
                if (num10 != 0)
                  Server.StaticCharacters[key2].SpellAnimationHistory.Remove(259);
                if (Server.StaticCharacters[key2].SpellAnimationHistory.ContainsKey(104) && key3 != (ushort)245 && key3 != (ushort)104)
                  Server.StaticCharacters[key2].SpellAnimationHistory.Remove(104);
                if (Server.StaticCharacters[key2].SpellAnimationHistory.ContainsKey(82) && key3 != (ushort)245 && key3 != (ushort)82)
                  Server.StaticCharacters[key2].SpellAnimationHistory.Remove(82);
                if (Server.StaticCharacters[key2].SpellAnimationHistory.ContainsKey(75) && key3 != (ushort)245 && key3 != (ushort)75)
                  Server.StaticCharacters[key2].SpellAnimationHistory.Remove(75);
                if (Server.StaticCharacters[key2].SpellAnimationHistory.ContainsKey(76) && key3 != (ushort)245 && key3 != (ushort)76)
                  Server.StaticCharacters[key2].SpellAnimationHistory.Remove(76);
              }
              if (flag)
              {
                msg = new ServerPacket((byte) 41);
                msg.WriteUInt32(key2);
                msg.WriteUInt32(key1);
                msg.WriteUInt16(num6);
                msg.WriteUInt16(num1);
                msg.WriteUInt16(num2);
                msg.Write(new byte[3]);
                client.Enqueue(msg);
                return false;
              }
            }
          }
        }
      }
      return true;
    }

    public bool ServerMessage_0x2C_AddSkill(Client client, ServerPacket msg)
    {
      Skill skill1 = new Skill();
      skill1.SkillSlot = (int) msg.ReadByte();
      skill1.Icon = (int) msg.ReadUInt16();
      skill1.Name = msg.ReadString((int) msg.ReadByte());
      Match match = Regex.Match(skill1.Name, "(.*?)( \\(Lev:)(\\d+)(\\/)(\\d+)(\\))");
      if (match.Success)
      {
        skill1.Name = match.Groups[1].Value;
        skill1.CurrentLevel = int.Parse(match.Groups[3].Value);
        skill1.MaximumLevel = int.Parse(match.Groups[5].Value);
      }
      if (System.IO.File.Exists(Options.DarkAgesDirectoryName + "\\" + client.Name + "\\SkillBook.cfg"))
      {
        StreamReader streamReader = new StreamReader((Stream) System.IO.File.Open(Options.DarkAgesDirectoryName + "\\" + client.Name + "\\SkillBook.cfg", FileMode.Open, FileAccess.Read, FileShare.Read));
        while (!streamReader.EndOfStream)
        {
          if (streamReader.ReadLine().Equals(skill1.Name, StringComparison.CurrentCultureIgnoreCase))
            skill1.Caption = streamReader.ReadLine().Split(':')[1];
        }
        streamReader.Close();
      }
      client.SkillBook[skill1.SkillSlot - 1] = skill1;
      if (client.FakeSkills.Count > 0)
      {
        foreach (Skill skill2 in client.FakeSkills.Values)
        {
          if (skill2 != null && skill1.SkillSlot == skill2.SkillSlot && !skill2.moved)
          {
            client.CreateSkill((byte) skill2.NewSlot, skill2.Icon, skill2.Name);
            client.SendMessage("created in add skill packet 1 slot " + skill2.NewSlot.ToString());
            break;
          }
        }
      }
      if ((!skill1.Name.Contains("Instrumental") || skill1.Name.Contains("Instrumental Attack")) && !skill1.Name.Contains("Elemental Bless") && skill1.Name != "Animal Feast" && skill1.Name != "Triple Kick" && skill1.Name != "Midnight Slash" && skill1.Name != "Thrash" && skill1.Name != "Long Strike" && skill1.Name != "Clobber" && skill1.Name != "Wallop" && skill1.Name != "Assault" && skill1.Name != "Double Punch" && skill1.Name != "Assail" && skill1.Name != "Lucky Hand" && !skill1.Name.Contains("Mend") && skill1.Name != "Tailoring" && skill1.Name != "Crasher" && skill1.Name != "Hairstyle" && skill1.Name != "Throw Smoke Bomb" && skill1.Name != "Unlock" && !skill1.Name.Contains("Inner Beast") && !skill1.Name.Contains("Archery") && !skill1.Name.Contains("Thrust Attack") && skill1.Name != "Two-handed Attack" && skill1.Name != "swimming" && skill1.Name != "Lumberjack" && !skill1.Name.Contains("Lore") && !skill1.Name.Contains("Item") && skill1.Name != "Appraise" && skill1.Name != "Wise Touch" && skill1.Name != "Look" && skill1.Name != "Wield Staff" && skill1.Name != "Execute")
      {
        if (!client.Tab.MacroOptions.macroskillslistview.Items.ContainsKey(skill1.Name) && skill1.CurrentLevel < skill1.MaximumLevel)
        {
          ListViewItem listViewItem = client.Tab.MacroOptions.macroskillslistview.Items.Add(skill1.Name, skill1.Name, -1);
          listViewItem.SubItems.Add(skill1.CurrentLevel.ToString());
          listViewItem.Checked = true;
        }
        else if (client.Tab.MacroOptions.macroskillslistview.Items.ContainsKey(skill1.Name) && client.Tab.MacroOptions.macroskillslistview.Items[skill1.Name] != null && skill1.CurrentLevel == skill1.MaximumLevel)
          client.Tab.MacroOptions.macroskillslistview.Items[skill1.Name].Remove();
        else if (client.Tab.MacroOptions.macroskillslistview.Items.ContainsKey(skill1.Name) && client.Tab.MacroOptions.macroskillslistview.Items[skill1.Name] != null)
          client.Tab.MacroOptions.macroskillslistview.Items[skill1.Name].SubItems[1].Text = skill1.CurrentLevel.ToString();
      }
      client.Tab.MacroOptions.macroskillslistview.Sort();
      if (client.Tab.SkillSwap.skilltemlist.Items.ContainsKey(skill1.SkillSlot.ToString()))
      {
        if (client.Tab.SkillSwap.skilltemlist.Items[skill1.SkillSlot.ToString()].SubItems.Count > 1)
          client.Tab.SkillSwap.skilltemlist.Items[skill1.SkillSlot.ToString()].SubItems[1].Text = skill1.Name;
        else
          client.Tab.SkillSwap.skilltemlist.Items[skill1.SkillSlot.ToString()].SubItems.Add(skill1.Name);
      }
      else if ((skill1.SkillSlot <= 36 ? 0 : (client.Tab.SkillSwap.skillmedlist.Items.ContainsKey((skill1.SkillSlot - 36).ToString()) ? 1 : 0)) != 0)
      {
        if (client.Tab.SkillSwap.skillmedlist.Items[(skill1.SkillSlot - 36).ToString()].SubItems.Count > 1)
          client.Tab.SkillSwap.skillmedlist.Items[(skill1.SkillSlot - 36).ToString()].SubItems[1].Text = skill1.Name;
        else
          client.Tab.SkillSwap.skillmedlist.Items[(skill1.SkillSlot - 36).ToString()].SubItems.Add(skill1.Name);
      }
      return true;
    }

    public bool ServerMessage_0x2D_RemoveSkill(Client client, ServerPacket msg)
    {
      byte num = msg.ReadByte();
      if (num > (byte) 0)
        client.SkillBook[(int) num - 1] = (Skill) null;
      if (num > (byte) 0 && client.FakeSkills.Count > 0)
      {
        foreach (Skill skill in client.FakeSkills.Values)
        {
          if (skill != null)
          {
            if ((int) num == skill.SkillSlot && !skill.moved)
            {
              skill.moved = true;
              break;
            }
            if (skill.moved)
            {
              skill.moved = false;
              client.CreateSkill((byte) skill.NewSlot, skill.Icon, skill.Name);
              client.SendMessage("created in remove skill slot " + skill.NewSlot.ToString());
              return false;
            }
            if ((int) num == skill.NewSlot)
            {
              skill.moved = true;
              break;
            }
          }
        }
      }
      if (client.Tab.SkillSwap.skilltemlist.Items.ContainsKey(num.ToString()))
      {
        if (client.Tab.SkillSwap.skilltemlist.Items[num.ToString()].SubItems.Count > 1)
          client.Tab.SkillSwap.skilltemlist.Items[num.ToString()].SubItems[1].Text = "";
      }
      else if ((num <= (byte) 36 ? 0 : (client.Tab.SkillSwap.skillmedlist.Items.ContainsKey(((int) num - 36).ToString()) ? 1 : 0)) != 0 && client.Tab.SkillSwap.skillmedlist.Items[((int) num - 36).ToString()].SubItems.Count > 1)
        client.Tab.SkillSwap.skillmedlist.Items[((int) num - 36).ToString()].SubItems[1].Text = "";
      return true;
    }

    public bool ServerMessage_0x2E_DisplayWorldMap(Client client, ServerPacket msg)
    {
      client.MapInfo.Number = 0;
      client.Towns.Clear();
      msg.ReadString((int) msg.ReadByte());
      byte num1 = msg.ReadByte();
      int num2 = (int) msg.ReadByte();
      for (int index = 0; index < (int) num1; ++index)
      {
        Town town = new Town();
        msg.Read(4);
        town.Name = msg.ReadString((int) msg.ReadByte());
        town.Number = msg.ReadUInt32();
        town.DestX = msg.ReadUInt16();
        town.DestY = msg.ReadUInt16();
        client.Towns.Add(town.Name, town);
      }
      return true;
    }

    public bool ServerMessage_0x2F_DialogueResponse(Client client, ServerPacket msg)
    {
      List<string> stringList = new List<string>();
      string empty = string.Empty;
      client.popup = true;
      client.cancast = false;
      client.canskill = false;
      client.donotwalk = true;
      byte num1 = msg.ReadByte();
      int num2 = (int) msg.ReadByte();
      uint num3 = msg.ReadUInt32();
      int num4 = (int) msg.ReadByte();
      int num5 = (int) msg.ReadInt16();
      msg.Read(2);
      int num6 = (int) msg.ReadInt16();
      msg.Read(2);
      string str1 = msg.ReadString((int) msg.ReadByte());
      string str2 = msg.ReadString((int) msg.ReadUInt16());
      string str3 = str2;
      if (num1 == (byte) 0)
      {
        byte num7 = msg.ReadByte();
        string str4 = string.Empty;
        for (int index = 0; index < (int) num7; ++index)
        {
          str4 = str4 + ", " + msg.ReadString((int) msg.ReadByte());
          msg.Read(2);
        }
        str3 = str2 + str4;
      }
      if (num1 == (byte) 4)
      {
        string str5 = string.Empty;
        msg.Read(3);
        byte num8 = msg.ReadByte();
        for (int index = 0; index < (int) num8; ++index)
        {
          int num9 = (int) msg.ReadUInt16();
          int num10 = (int) msg.ReadByte();
          uint num11 = msg.ReadUInt32();
          string str6 = msg.ReadString((int) msg.ReadByte());
          str5 = str5 + ", " + str6;
          stringList.Add(str6 + " : " + num11.ToString());
          msg.ReadString((int) msg.ReadByte());
        }
        str3 = str2 + str5;
      }
      if (client.banklist)
      {
        StreamWriter streamWriter = new StreamWriter(Program.StartupPath + "\\Settings\\" + client.Name.ToLower() + "\\banklist.txt", false);
        foreach (string str7 in stringList)
        {
          if (str7 != string.Empty)
            streamWriter.WriteLine(str7);
        }
        streamWriter.Close();
        Process.Start(Program.StartupPath + "\\Settings\\" + client.Name.ToLower() + "\\banklist.txt");
      }
      client.Currentnpcname = str1;
      client.Currentnpctext = str3;
      client.CurrentnpcpopupID = num3;
      return true;
    }

    public bool ServerMessage_0x30_PopupResponse(Client client, ServerPacket msg)
    {
      client.agchest = false;
      client.agchestopen = false;
      client.wdchest = false;
      client.wdchestopen = false;
      client.andorchest = false;
      client.andorchestopen = false;
      client.queenchest = false;
      client.queenchestopen = false;
      client.veltainchest = false;
      client.heavychest = false;
      client.smallbag = false;
      client.smallbagopen = false;
      client.bigbag = false;
      client.bigbagopen = false;
      client.heavybag = false;
      client.heavybagopen = false;
      client.atemeg = false;
      client.ateabbox = false;
      client.ateabgift = false;
      byte num1 = 0;
      string str1 = string.Empty;
      byte num2 = msg.ReadByte();
      uint num3 = 0;
      if (num2 != (byte) 10)
      {
        client.popup = true;
        client.cancast = false;
        client.canskill = false;
        client.donotwalk = true;
        switch (msg.ReadByte())
        {
          case 1:
            num3 = msg.ReadUInt32();
            int num4 = (int) msg.ReadByte();
            int num5 = (int) msg.ReadInt16();
            msg.Read(2);
            int num6 = (int) msg.ReadInt16();
            msg.Read(2);
            msg.Read(6);
            string str2 = msg.ReadString((int) msg.ReadByte());
            string str3 = msg.ReadString((int) msg.ReadUInt16());
            str1 = str3;
            if (num2 == (byte) 2)
            {
              byte num7 = msg.ReadByte();
              string str4 = string.Empty;
              for (int index = 0; index < (int) num7; ++index)
                str4 = str4 + ", " + msg.ReadString((int) msg.ReadByte());
              str1 = str3 + str4;
            }
            client.Currentnpcname = str2;
            if (client.Tab.p4.Text != string.Empty)
            {
              byte[] bytes = BitConverter.GetBytes(uint.Parse(client.Tab.p4.Text));
              msg.BodyData[2] = bytes[3];
              msg.BodyData[3] = bytes[2];
              msg.BodyData[4] = bytes[1];
              msg.BodyData[5] = bytes[0];
              break;
            }
            break;
          case 2:
            num3 = msg.ReadUInt32();
            int num8 = (int) msg.ReadByte();
            int num9 = (int) msg.ReadInt16();
            msg.Read(2);
            int num10 = (int) msg.ReadInt16();
            msg.Read(2);
            msg.Read(6);
            string str5 = msg.ReadString((int) msg.ReadByte());
            client.Currentnpcname = str5;
            string str6 = msg.ReadString((int) msg.ReadUInt16());
            str1 = str6;
            if (str5 == "Map of Ant Tunnels" && str6.StartsWith("You will be transported to"))
            {
              client.anttunnels = 1;
              break;
            }
            if (str5 == "Map of Ant Guardian Tunnels" && str6.StartsWith("You will be transported to"))
            {
              client.guardiananttunnels = 1;
              break;
            }
            if (str5 == "Bard's Notes")
            {
              client.bardsnotesID = num3;
              break;
            }
            break;
          case 4:
            num3 = msg.ReadUInt32();
            msg.Read(8);
            int num11 = (int) msg.ReadByte();
            num1 = msg.ReadByte();
            int num12 = (int) msg.ReadUInt16();
            msg.Read(3);
            byte length = msg.ReadByte();
            if (length > (byte) 0)
              msg.ReadString((int) length);
            string str7 = msg.ReadString((int) msg.ReadUInt16());
            str1 = str7;
            if (num2 == (byte) 2)
            {
              byte num13 = msg.ReadByte();
              string str8 = string.Empty;
              for (int index = 0; index < (int) num13; ++index)
                str8 = str8 + ", " + msg.ReadString((int) msg.ReadByte());
              str1 = str7 + str8;
              break;
            }
            break;
        }
      }
      if (num2 == (byte) 10)
      {
        client.closepopupvars();
        str1 = string.Empty;
      }
      client.Currentnpcscript = num1;
      client.Currentpopuptype = (int) num2;
      client.CurrentnpcpopupID = num3;
      client.Currentnpctext = str1;
      if (client.Currentnpctext.StartsWith("You have lost touch") && client.autowalkon)
      {
        client.SendMessage("No faith, auto-walk stopped.");
        client.Tab.autowalker_button.Text = "Start";
        client.autowalkon = false;
      }
      if (client.Currentnpctext == "You don't have any swords that may be smithed any more than they are.")
        client.bladesmithnoswords = true;
      if (client.Currentnpctext == "You don't have any garment that may be tailored.")
        client.tailornoarmors = true;
      if (client.Currentnpctext.StartsWith("You've blistered yourself badly"))
        client.tailorool = true;
      if (client.MapInfo.Number == 7050 && client.Currentnpctext.StartsWith("Thank you for scaring "))
        client.SaveTimedStuff(22);
      else if (client.MapInfo.Number == 8990 && client.Currentnpctext.StartsWith("This must be the wall markings that Nairn was talking about."))
        client.SaveTimedStuff(11);
      else if (client.MapInfo.Number == 8995 && client.Currentnpctext.StartsWith("Guess who we encountered after performing the ritual?"))
        client.SaveTimedStuff(12);
      else if (client.MapInfo.Number == 8298 && client.Currentnpctext.StartsWith("Thank you! You saved me"))
        client.SaveTimedStuff(15);
      else if (client.MapInfo.Number == 6998 && client.Currentnpctext.StartsWith("Thank you for your efforts."))
        client.SaveTimedStuff(10);
      else if (client.MapInfo.Number == 8297 && client.Currentnpctext.StartsWith("Excellent! Here is"))
        client.SaveTimedStuff(16);
      else if (client.MapInfo.Number == 10266 && client.Currentnpctext.StartsWith("That's something for us to worry about"))
        client.SaveTimedStuff(17);
      else if (client.MapInfo.Number == 6805 && client.Currentnpctext.StartsWith("You drink from the fountain."))
        client.SaveTimedStuff(18);
      else if (client.MapInfo.Number == 950 && (client.Currentnpctext.StartsWith("Excellent! Thank you so much!") || client.Currentnpctext.StartsWith("Well done!") || client.Currentnpctext.StartsWith("You remind me of a dear friend")))
        client.SaveTimedStuff(19);
      else if (client.MapInfo.Number == 115 && client.Currentnpctext.StartsWith("I see you were able "))
        client.SaveTimedStuff(21);
      else if (client.MapInfo.Number == 992 && client.Currentnpctext.StartsWith("Sorry, no faeries are willing to bond with you."))
        client.SaveTimedStuff(39);
      else if (client.MapInfo.Number == 3052 && client.Currentnpctext.StartsWith("Excellent! Here is your prize"))
        client.SaveTimedStuff(41);
      else if (client.MapInfo.Number == 132 && (client.Currentnpctext.StartsWith("Thank ya so much. Here's a few coins for your effort.") || client.Currentnpctext.StartsWith("Ah thank ya, you've done well.") || client.Currentnpctext.StartsWith("Ah, it's a good thing you've done") || client.Currentnpctext.StartsWith("You remind me of my own little child.")))
        client.SaveTimedStuff(42);
      else if (client.Currentnpcname.Equals("Arcella's Gift1") && client.Currentnpctext.StartsWith("You are about to open the gift"))
        client.agchest = true;
      else if (client.Currentnpcname.Equals("Water Dungeon Chest") && client.Currentnpctext.Contains(", What type of prize would you"))
        client.wdchest = true;
      else if (client.Currentnpcname.Equals("Andor Chest") && client.Currentnpctext.Contains(", What type of prize"))
        client.andorchest = true;
      else if (client.Currentnpcname.Equals("Andor Queen's Chest") && client.Currentnpctext.Contains(", What type of prize"))
        client.queenchest = true;
      else if (client.Currentnpcname.Equals("Heavy Canal Treasure Bag") && client.Currentnpctext.Contains("You are about to pull an item"))
        client.heavybag = true;
      else if (client.Currentnpcname.Equals("Big Canal Treasure Bag") && client.Currentnpctext.Contains("You are about to pull an item"))
        client.bigbag = true;
      else if (client.Currentnpcname.Equals("Canal Treasure Bag") && client.Currentnpctext.Contains("You are about to pull an item"))
        client.smallbag = true;
      else if (client.Currentnpcname.Equals("Veltain Treasure Chest") && client.Currentnpctext.Contains("How much do you want to invest"))
        client.veltainchest = true;
      else if (client.Currentnpcname.Equals("Heavy Veltain Treasure Chest") && client.Currentnpctext.Contains("How much do you want to invest"))
        client.heavychest = true;
      else if (client.Currentnpcname.Equals("Mother Erbie Gift") && client.Currentnpctext.Contains("You will gain great"))
        client.atemeg = true;
      else if (client.Currentnpcname.StartsWith("Ability and Experience Gift") && client.Currentnpctext.Contains("You will gain great"))
        client.ateabgift = true;
      else if (client.Currentnpcname.StartsWith("Ability and Experience Box") && client.Currentnpctext.Contains("You will gain great"))
        client.ateabbox = true;
      else if (client.Currentnpctext.Contains("Wonderful. Just wait a moment"))
        client.SaveTimedStuff(31);
      else if (client.Currentnpctext.Equals("She finally bows her head, accepting it."))
        client.SaveTimedStuff(33);
      else if (client.Currentnpctext.StartsWith("Ahh well, it's best we don't tell Santa"))
        client.SaveTimedStuff(37);
      else if (client.Currentnpctext.StartsWith("Thanks a bunch, I wish I was there"))
        client.SaveTimedStuff(38);
      if (client.Currentnpctext.StartsWith("This must be the first part of the story."))
        client.lawwall = 1;
      else if (client.Currentnpctext.StartsWith("This seems to match the next part of the story."))
        client.lawwall = 2;
      else if (client.Currentnpctext.StartsWith("This must be the third part of the story."))
        client.lawwall = 3;
      else if (client.Currentnpctext.StartsWith("Yes! The fourth part!"))
        client.lawwall = 4;
      else if (client.Currentnpctext.StartsWith("Great! Another part of the story!"))
        client.lawwall = 5;
      else if (client.Currentnpctext.StartsWith("Woohoo! This story is still confusing to me."))
        client.lawwall = 6;
      else if (client.Currentnpctext.StartsWith("Finally!!! I'm glad this is the last"))
        client.lawwall = 7;
      else if (client.MapInfo.Number == 10240 && (client.Currentnpctext.StartsWith("You killed my beloved") || client.Currentnpctext.StartsWith("How dare you enter")))
      {
        client.closepopupvars();
        string empty = string.Empty;
      }
      else if (client.MapInfo.Number == 9375 || client.MapInfo.Number == 9376 && (client.Currentnpctext.StartsWith("Not all your group is here.") || client.Currentnpctext.StartsWith("One of your group member recently defeated Son of Drakari ((3 hour wait)).") || client.Currentnpctext.StartsWith("Not all of your group has the skill and experience to enter this cave...")))
      {
        client.closepopupvars();
        string empty = string.Empty;
      }
      else if (client.Currentnpctext.Contains("one of the gods of the ") || client.Currentnpctext.Contains(" is praying to "))
      {
        if (client.Currentnpctext.Contains("Deoch Trinity") || client.Currentnpctext.Contains(" is praying to Deoch"))
          client.prayscript = (byte) 30;
        else if (client.Currentnpctext.Contains("Glioca Trinity") || client.Currentnpctext.Contains(" is praying to Glioca"))
          client.prayscript = (byte) 31;
        else if (client.Currentnpctext.Contains("Cail Trinity") || client.Currentnpctext.Contains(" is praying to Cail"))
          client.prayscript = (byte) 32;
        else if (client.Currentnpctext.Contains("Luathas Trinity") || client.Currentnpctext.Contains(" is praying to Luathas"))
          client.prayscript = (byte) 33;
        else if (client.Currentnpctext.Contains("Gramail Trinity") || client.Currentnpctext.Contains(" is praying to Gramail"))
          client.prayscript = (byte) 34;
        else if (client.Currentnpctext.Contains("Fiosachd Trinity") || client.Currentnpctext.Contains(" is praying to Fiosachd"))
          client.prayscript = (byte) 35;
        else if (client.Currentnpctext.Contains("Ceannlaidir Trinity") || client.Currentnpctext.Contains(" is praying to Ceannlaidir"))
          client.prayscript = (byte) 36;
        else if (client.Currentnpctext.Contains("Sgrios Trinity") || client.Currentnpctext.Contains(" is praying to Sgrios"))
          client.prayscript = (byte) 37;
      }
      else if (client.Currentnpctext.Contains("Ancusa"))
        client.herbscript = (byte) 38;
      else if (client.Currentnpctext.Contains("Betony"))
        client.herbscript = (byte) 39;
      else if (client.Currentnpctext.Contains("Fifleaf"))
        client.herbscript = (byte) 40;
      else if (client.Currentnpctext.Contains("Hemloch"))
        client.herbscript = (byte) 41;
      else if (client.Currentnpctext.Contains("Hydele"))
        client.herbscript = (byte) 42;
      else if (client.Currentnpctext.Contains("Personaca"))
        client.herbscript = (byte) 43;
      return true;
    }

    public bool ServerMessage_0x31_MailMenu(Client client, ServerPacket msg)
    {
      byte num1 = msg.ReadByte();
      if (num1 == (byte) 4)
      {
        int num2 = (int) msg.ReadUInt32();
        int num3 = (int) msg.ReadUInt32();
        byte num4 = msg.ReadByte();
        for (int index = 0; index < (int) num4; ++index)
        {
          int num5 = (int) msg.ReadByte();
          short num6 = msg.ReadInt16();
          string str1 = msg.ReadString8();
          int num7 = (int) msg.ReadByte();
          int num8 = (int) msg.ReadByte();
          string str2 = msg.ReadString8();
          Mail mail = new Mail();
          mail.Sender = str1;
          mail.Title = str2;
          mail.Number = num6;
          int number = (int) mail.Number;
          client.MailList.Add(mail);
        }
      }
      if (num1 == (byte) 5)
      {
        int num9 = (int) msg.ReadByte();
        int num10 = (int) msg.ReadByte();
        int num11 = (int) msg.ReadInt16();
        msg.ReadString8();
        int num12 = (int) msg.ReadByte();
        int num13 = (int) msg.ReadByte();
        msg.ReadString8();
        msg.ReadString16();
      }
      return true;
    }

    public bool ServerMessage_0x33_DisplayPlayer(Client client, ServerPacket msg)
    {
      Player player = new Player();
      player.Location.X = (int) msg.ReadUInt16();
      player.Location.Y = (int) msg.ReadUInt16();
      player.Location.Direction = (Direction) msg.ReadByte();
      player.ID = msg.ReadUInt32();
      player.Head = msg.ReadUInt16();
      if (player.Head == ushort.MaxValue)
      {
        player.Form = msg.ReadUInt16();
        player.Body = (byte) 0;
        player.Arms = (ushort) msg.ReadByte();
        player.Boots = msg.ReadByte();
        player.Armor = msg.ReadUInt16();
        player.Shield = msg.ReadByte();
        player.Weapon = msg.ReadUInt16();
        msg.Read(1);
        player.HeadColor = (byte) 0;
        player.BootColor = (byte) 0;
        player.Acc1Color = (byte) 0;
        player.Acc1 = (ushort) 0;
        player.Acc2Color = (byte) 0;
        player.Acc2 = (ushort) 0;
        player.Unknown = (byte) 0;
        player.Acc3 = (ushort) 0;
        player.Unknown2 = (byte) 0;
        player.RestCloak = (byte) 0;
        player.Overcoat = (ushort) 0;
        player.OvercoatColor = (byte) 0;
        player.SkinColor = (byte) 0;
        player.HideBool = (byte) 0;
        player.FaceShape = (byte) 0;
      }
      else
      {
        player.Form = (ushort) 0;
        player.Body = msg.ReadByte();
        player.Arms = msg.ReadUInt16();
        player.Boots = msg.ReadByte();
        player.Armor = msg.ReadUInt16();
        player.Shield = msg.ReadByte();
        player.Weapon = msg.ReadUInt16();
        player.HeadColor = msg.ReadByte();
        player.BootColor = msg.ReadByte();
        player.Acc1Color = msg.ReadByte();
        player.Acc1 = msg.ReadUInt16();
        player.Acc2Color = msg.ReadByte();
        player.Acc2 = msg.ReadUInt16();
        player.Unknown = msg.ReadByte();
        player.Acc3 = msg.ReadUInt16();
        player.Unknown2 = msg.ReadByte();
        player.RestCloak = msg.ReadByte();
        player.Overcoat = msg.ReadUInt16();
        player.OvercoatColor = msg.ReadByte();
        player.SkinColor = msg.ReadByte();
        player.HideBool = msg.ReadByte();
        player.FaceShape = msg.ReadByte();
      }
      player.NameTagStyle = msg.ReadByte();
      player.Name = msg.ReadString((int) msg.ReadByte());
      player.GroupName = msg.ReadString((int) msg.ReadByte());
      player.Map = client.MapInfo.Number;
      if (client.Tab.MonstersByPlayer != null && player.Name.ToLower() == client.Tab.MonstersByPlayer.Text.Remove(client.Tab.MonstersByPlayer.Text.IndexOf("'s")).ToLower())
        client.playeridformonster = player.ID;
      if (!Server.StaticCharacters.ContainsKey(player.ID))
        Server.StaticCharacters.Add(player.ID, (Character) player);
      if (!client.Characters.ContainsKey(player.ID))
      {
        if (player.Name != string.Empty && client.FindCharacterByName<Player>(player.Name) != null)
          client.RemoveCharacterByName(player.Name);
        client.Characters.Add(player.ID, (Character) player);
        client.Characters[player.ID].CreateTime = DateTime.UtcNow;
        if (player.Name == string.Empty)
        {
          foreach (Client client1 in Server.Alts.Values.ToArray<Client>())
          {
            if (client1 != null && (int) client1.PlayerID == (int) player.ID)
              client.Characters[player.ID].Name = client1.Name;
          }
          if (client.Characters[player.ID].Name == string.Empty && (int) client.ClickEntityID != (int) player.ID)
          {
            client.hidelegend = true;
            client.ClickEntityID = player.ID;
            client.ClickEntity(player.ID);
          }
        }
        if (client.Characters[player.ID].Name != string.Empty)
          (client.Characters[player.ID] as Player).DisplayName = client.Characters[player.ID].Name;
      }
      else
      {
        if (player.Name != string.Empty)
          client.Characters[player.ID].Name = player.Name;
        Player character = client.Characters[player.ID] as Player;
        character.Map = player.Map;
        character.Location.X = player.Location.X;
        character.Location.Y = player.Location.Y;
        character.Location.Direction = player.Location.Direction;
        character.Head = player.Head;
        character.Form = player.Form;
        character.Body = player.Body;
        character.Arms = player.Arms;
        character.Boots = player.Boots;
        character.Armor = player.Armor;
        character.Shield = player.Shield;
        character.Weapon = player.Weapon;
        character.HeadColor = player.HeadColor;
        character.BootColor = player.BootColor;
        character.Acc1Color = player.Acc1Color;
        character.Acc1 = player.Acc1;
        character.Acc2Color = player.Acc2Color;
        character.Acc2 = player.Acc2;
        character.Unknown = player.Unknown;
        character.Acc3 = player.Acc3;
        character.Unknown2 = player.Unknown2;
        character.RestCloak = player.RestCloak;
        character.Overcoat = player.Overcoat;
        character.OvercoatColor = player.OvercoatColor;
        character.SkinColor = player.SkinColor;
        character.HideBool = player.HideBool;
        character.FaceShape = player.FaceShape;
        character.NameTagStyle = player.NameTagStyle;
        character.GroupName = player.GroupName;
        character.IsOnScreen = true;
      }
      if (!client.AlarmMapAllowList && Server.alertfornonfriends && !client.SafeToWalkFast && (client.Characters.ContainsKey(player.ID) && client.Characters[player.ID].Name != string.Empty && !Server.Alts.ContainsKey(client.Characters[player.ID].Name.ToLower()) && !Server.friendlist.Contains(client.Characters[player.ID].Name.ToLower()) || client.Characters[player.ID].Name == string.Empty) && (client.MapInfo.Number != 623 || client.ServerLocation.Y > 65 || client.ServerLocation.X > 20 || client.ServerLocation.Y < 27))
      {
        if (client.Characters[player.ID].Name != string.Empty)
          client.SendMessage(client.Characters[player.ID].Name + " was seen!", "red");
        else
          client.Characters[player.ID].wasseenwarning = true;
        if (Program.MainForm.vplaynoise && !Server.SentryAlarm)
        {
          User32.FlashWindow(client.mainProc.MainWindowHandle, false);
          Server.SentryAlarm = true;
          Server.alarm = new SoundPlayer(Assembly.GetExecutingAssembly().GetManifestResourceStream("Flintstones.ALARMquiet.wav"));
          Server.alarmTimer = DateTime.UtcNow;
          Server.alarm.PlayLooping();
        }
        Server.alertfornonfriends = false;
        Server.AlertNonFriendTimer.Start();
      }
      if ((int) player.ID == (int) client.PlayerID)
      {
        client.UpdateFakeNpcs();
        client.SendMessage(client.LastPermMessage, (byte) 18);
        client.ClientLocation.X = player.Location.X;
        client.ClientLocation.Y = player.Location.Y;
        client.ClientLocation.Direction = player.Location.Direction;
        client.ServerLocation.Direction = player.Location.Direction;
        client.ClientHead = player.Head;
        client.ClientForm = player.Form;
        client.ClientBody = player.Body;
        client.ClientArms = player.Arms;
        client.ClientBoots = player.Boots;
        client.ClientArmor = player.Armor;
        client.ClientShield = player.Shield;
        client.ClientWeapon = player.Weapon;
        client.ClientHeadColor = player.HeadColor;
        client.ClientBootColor = player.BootColor;
        client.ClientAcc1Color = player.Acc1Color;
        client.ClientAcc1 = player.Acc1;
        client.ClientAcc2Color = player.Acc2Color;
        client.ClientAcc2 = player.Acc2;
        client.ClientUnknown = player.Unknown;
        client.ClientAcc3 = player.Acc3;
        client.ClientUnknown2 = player.Unknown2;
        client.ClientRestCloak = player.RestCloak;
        client.ClientOvercoat = player.Overcoat;
        client.ClientOvercoatColor = player.OvercoatColor;
        client.ClientSkinColor = player.SkinColor;
        client.ClientHideBool = player.HideBool;
        client.ClientFaceShape = player.FaceShape;
        client.ClientNameTagStyle = player.NameTagStyle;
        client.ClientName = player.Name;
        client.ClientGroup = player.GroupName;
        if (!client.safemode)
        {
          if (client.Tab.vusemonster && client.SafeToWalkFast)
          {
            client.imonster = true;
            msg = new ServerPacket((byte) 51);
            msg.WriteUInt16((ushort) player.Location.X);
            msg.WriteUInt16((ushort) player.Location.Y);
            msg.WriteByte((byte) player.Location.Direction);
            msg.WriteUInt32(player.ID);
            msg.WriteUInt16(ushort.MaxValue);
            msg.WriteUInt16((ushort) (client.Tab.usemonsterid.Value + 16384M));
            msg.WriteUInt32(0U);
            msg.WriteUInt32(0U);
            msg.WriteByte(player.NameTagStyle);
            msg.WriteString8(client.Name);
            msg.WriteString8(player.GroupName);
          }
          else
          {
            client.imonster = false;
            msg = new ServerPacket((byte) 51);
            msg.WriteUInt16((ushort) player.Location.X);
            msg.WriteUInt16((ushort) player.Location.Y);
            msg.WriteByte((byte) player.Location.Direction);
            msg.WriteUInt32(player.ID);
            if (client.ClientHead == ushort.MaxValue)
            {
              msg.WriteUInt16(client.ClientHead);
              msg.WriteUInt16(client.ClientForm);
              msg.WriteUInt32(0U);
              msg.WriteUInt32(0U);
            }
            else
            {
              if (client.Tab.duhat.Checked && client.Tab.duhatnum.Value != 0M)
                msg.WriteUInt16((ushort) client.Tab.duhatnum.Value);
              else
                msg.WriteUInt16(client.ClientHead);
              msg.WriteByte(client.ClientBody);
              msg.WriteUInt16(client.ClientArms);
              if (client.Tab.duboots.Checked && client.Tab.dubootsnum.Value != 0M)
                msg.WriteByte((byte) client.Tab.dubootsnum.Value);
              else
                msg.WriteByte(client.ClientBoots);
              if (client.Tab.duarmor.Checked && client.Tab.duarmornum.Value != 0M)
                msg.WriteUInt16((ushort) client.Tab.duarmornum.Value);
              else
                msg.WriteUInt16(client.ClientArmor);
              if (client.Tab.dushield.Checked && client.Tab.dushieldnum.Value != 0M)
                msg.WriteByte((byte) client.Tab.dushieldnum.Value);
              else
                msg.WriteByte(client.ClientShield);
              if (client.Tab.duweapon.Checked && client.Tab.duweaponnum.Value != 0M)
                msg.WriteUInt16((ushort) client.Tab.duweaponnum.Value);
              else
                msg.WriteUInt16(client.ClientWeapon);
              if (client.Tab.duhat.Checked && client.Tab.duhatcolor.Value != 0M)
                msg.WriteByte((byte) client.Tab.duhatcolor.Value);
              else
                msg.WriteByte(client.ClientHeadColor);
              if (client.Tab.duboots.Checked && client.Tab.dubootcolor.Value != 0M)
                msg.WriteByte((byte) client.Tab.dubootcolor.Value);
              else
                msg.WriteByte(client.ClientBootColor);
              if (client.Tab.duacc1.Checked && client.Tab.duacc1color.Value != 0M)
                msg.WriteByte((byte) client.Tab.duacc1color.Value);
              else
                msg.WriteByte(client.ClientAcc1Color);
              if (client.Tab.duacc1.Checked && client.Tab.duacc1num.Value != 0M)
                msg.WriteUInt16((ushort) client.Tab.duacc1num.Value);
              else
                msg.WriteUInt16(client.ClientAcc1);
              if (client.Tab.duacc2.Checked && client.Tab.duacc2color.Value != 0M)
                msg.WriteByte((byte) client.Tab.duacc2color.Value);
              else
                msg.WriteByte(client.ClientAcc2Color);
              if (client.Tab.duacc2.Checked && client.Tab.duacc2num.Value != 0M)
                msg.WriteUInt16((ushort) client.Tab.duacc2num.Value);
              else
                msg.WriteUInt16(client.ClientAcc2);
              if (client.Tab.duacc3.Checked && client.Tab.duacc3color.Value != 0M)
                msg.WriteByte((byte) client.Tab.duacc3color.Value);
              else
                msg.WriteByte(client.ClientUnknown);
              if (client.Tab.duacc3.Checked && client.Tab.duacc3num.Value != 0M)
                msg.WriteUInt16((ushort) client.Tab.duacc3num.Value);
              else
                msg.WriteUInt16(client.ClientAcc3);
              if (client.Tab.duarmor.Checked && client.Tab.duunknown2.Value != 0M)
                msg.WriteByte((byte) client.Tab.duunknown2.Value);
              else
                msg.WriteByte(client.ClientUnknown2);
              msg.WriteByte(client.ClientRestCloak);
              if (client.Tab.duovercoat.Checked && client.Tab.duovercoatnum.Value != 0M)
                msg.WriteUInt16((ushort) client.Tab.duovercoatnum.Value);
              else
                msg.WriteUInt16(client.ClientOvercoat);
              if (client.Tab.duovercoat.Checked && client.Tab.duovercoatcolor.Value != 0M)
                msg.WriteByte((byte) client.Tab.duovercoatcolor.Value);
              else
                msg.WriteByte(client.ClientOvercoatColor);
              if (client.Tab.duskin.Checked && client.Tab.duskinnum.Value != 0M)
                msg.WriteByte((byte) client.Tab.duskinnum.Value);
              else
                msg.WriteByte(client.ClientSkinColor);
              msg.WriteByte(client.ClientHideBool);
              if (client.Tab.duface.Checked && client.Tab.dufacenum.Value != 0M)
                msg.WriteByte((byte) client.Tab.dufacenum.Value);
              else
                msg.WriteByte(client.ClientFaceShape);
            }
            msg.WriteByte(client.ClientNameTagStyle);
            msg.WriteString8(client.ClientName);
            msg.WriteString8(client.ClientGroup);
          }
        }
      }
      if (client.imonster && !client.SafeToWalkFast && !client.safemode)
      {
        client.imonster = false;
        ServerPacket msg1 = new ServerPacket((byte) 51);
        msg1.WriteUInt16((ushort) client.ServerLocation.X);
        msg1.WriteUInt16((ushort) client.ServerLocation.Y);
        msg1.WriteByte((byte) client.ClientLocation.Direction);
        msg1.WriteUInt32(client.PlayerID);
        if (client.ClientHead == ushort.MaxValue)
        {
          msg1.WriteUInt16(client.ClientHead);
          msg1.WriteUInt16(client.ClientForm);
          msg1.WriteUInt32(0U);
          msg1.WriteUInt32(0U);
        }
        else
        {
          msg1.WriteUInt16(client.ClientHead);
          msg1.WriteByte(client.ClientBody);
          msg1.WriteUInt16(client.ClientArms);
          msg1.WriteByte(client.ClientBoots);
          msg1.WriteUInt16(client.ClientArmor);
          msg1.WriteByte(client.ClientShield);
          msg1.WriteUInt16(client.ClientWeapon);
          msg1.WriteByte(client.ClientHeadColor);
          msg1.WriteByte(client.ClientBootColor);
          msg1.WriteByte(client.ClientAcc1Color);
          msg1.WriteUInt16(client.ClientAcc1);
          msg1.WriteByte(client.ClientAcc2Color);
          msg1.WriteUInt16(client.ClientAcc2);
          msg1.WriteByte(client.ClientUnknown);
          msg1.WriteUInt16(client.ClientAcc3);
          msg1.WriteByte(client.ClientUnknown2);
          msg1.WriteByte(client.ClientRestCloak);
          msg1.WriteUInt16(client.ClientOvercoat);
          msg1.WriteByte(client.ClientOvercoatColor);
          msg1.WriteByte(client.ClientSkinColor);
          msg1.WriteByte(client.ClientHideBool);
          msg1.WriteByte(client.ClientFaceShape);
        }
        msg1.WriteByte(client.ClientNameTagStyle);
        msg1.WriteString8(client.ClientName);
        msg1.WriteString8(client.ClientGroup);
        msg1.Write(new byte[3]);
        client.Enqueue(msg1);
      }
      if (!client.Characters.ContainsKey(player.ID))
        return true;
      byte num = (client.Characters[player.ID] as Player).NameTagStyle;
      string str = (client.Characters[player.ID] as Player).Name;
      if (str.Contains("["))
        str = str.Remove(str.IndexOf("[") - 1);
      if (str.Contains(")"))
        str = str.Remove(0, str.IndexOf(" ") + 1);
      if (!client.safemode)
      {
        DateTime utcNow;
        TimeSpan timeSpan;
        if (client.Tab.vmonitordion && Server.StaticCharacters[player.ID].hasdion)
        {
          num = (byte) 3;
          utcNow = DateTime.UtcNow;
          timeSpan = utcNow.Subtract(Server.StaticCharacters[player.ID].SpellAnimationHistory[244]);
          str = "(" + (20 - (int) timeSpan.TotalSeconds).ToString() + ") " + str;
        }
        if (client.Tab.vmonitordion && Server.StaticCharacters[player.ID].hasironskin)
        {
          num = (byte) 3;
          utcNow = DateTime.UtcNow;
          timeSpan = utcNow.Subtract(Server.StaticCharacters[player.ID].SpellAnimationHistory[89]);
          str = "(" + (19 - (int) timeSpan.TotalSeconds).ToString() + ") " + str;
        }
        if (client.Tab.vmonitordion && Server.StaticCharacters[player.ID].hasdioncomlha)
        {
          num = (byte) 3;
          utcNow = DateTime.UtcNow;
          timeSpan = utcNow.Subtract(Server.StaticCharacters[player.ID].SpellAnimationHistory[93]);
          str = "(" + (20 - (int) timeSpan.TotalSeconds).ToString() + ") " + str;
        }
        if (client.Tab.vmonitordion && Server.StaticCharacters[player.ID].haswingsofprot)
        {
          num = (byte) 3;
          utcNow = DateTime.UtcNow;
          timeSpan = utcNow.Subtract(Server.StaticCharacters[player.ID].SpellAnimationHistory[86]);
          str = "(" + (14 - (int) timeSpan.TotalSeconds).ToString() + ") " + str;
        }
        if (client.Tab.vmonitordion && Server.StaticCharacters[player.ID].hasasgall)
        {
          num = (byte) 3;
          utcNow = DateTime.UtcNow;
          timeSpan = utcNow.Subtract(Server.StaticCharacters[player.ID].SpellAnimationHistory[66]);
          str = "(" + (13 - (int) timeSpan.TotalSeconds).ToString() + ") " + str;
        }
        if (client.Tab.vmonitorspells && Server.StaticCharacters[player.ID].hasaite && !Server.StaticCharacters[player.ID].hasfas)
        {
          num = (byte) 3;
          str += " [aite]";
        }
        else if (client.Tab.vmonitorspells && Server.StaticCharacters[player.ID].hasaite && Server.StaticCharacters[player.ID].hasfas)
        {
          num = (byte) 3;
          str += " [aite/fas]";
        }
        else if (client.Tab.vmonitorspells && !Server.StaticCharacters[player.ID].hasaite && Server.StaticCharacters[player.ID].hasfas)
        {
          num = (byte) 3;
          str += " [fas]";
        }
      }
      if (!client.safemode && client.Tab.seeinvis.Checked)
        (client.Characters[player.ID] as Player).DisplayName = str;
      ServerPacket msg2 = new ServerPacket((byte) 51);
      msg2.WriteUInt16((ushort) client.Characters[player.ID].Location.X);
      msg2.WriteUInt16((ushort) client.Characters[player.ID].Location.Y);
      msg2.WriteByte((byte) client.Characters[player.ID].Location.Direction);
      msg2.WriteUInt32(client.Characters[player.ID].ID);
      msg2.WriteUInt16((client.Characters[player.ID] as Player).Head);
      if ((client.Characters[player.ID] as Player).Head == ushort.MaxValue)
      {
        msg2.WriteUInt16((client.Characters[player.ID] as Player).Form);
        msg2.WriteUInt32(0U);
        msg2.WriteUInt32(0U);
        if (!client.safemode)
          msg2.WriteByte((byte) 3);
        else
          msg2.WriteByte(num);
      }
      else
      {
        msg2.WriteByte((client.Characters[player.ID] as Player).Body);
        msg2.WriteUInt16((client.Characters[player.ID] as Player).Arms);
        msg2.WriteByte((client.Characters[player.ID] as Player).Boots);
        msg2.WriteUInt16((client.Characters[player.ID] as Player).Armor);
        msg2.WriteByte((client.Characters[player.ID] as Player).Shield);
        msg2.WriteUInt16((client.Characters[player.ID] as Player).Weapon);
        msg2.WriteByte((client.Characters[player.ID] as Player).HeadColor);
        msg2.WriteByte((client.Characters[player.ID] as Player).BootColor);
        msg2.WriteByte((client.Characters[player.ID] as Player).Acc1Color);
        msg2.WriteUInt16((client.Characters[player.ID] as Player).Acc1);
        msg2.WriteByte((client.Characters[player.ID] as Player).Acc2Color);
        msg2.WriteUInt16((client.Characters[player.ID] as Player).Acc2);
        msg2.WriteByte((client.Characters[player.ID] as Player).Unknown);
        msg2.WriteUInt16((client.Characters[player.ID] as Player).Acc3);
        msg2.WriteByte((client.Characters[player.ID] as Player).Unknown2);
        msg2.WriteByte((client.Characters[player.ID] as Player).RestCloak);
        msg2.WriteUInt16((client.Characters[player.ID] as Player).Overcoat);
        msg2.WriteByte((client.Characters[player.ID] as Player).OvercoatColor);
        msg2.WriteByte((client.Characters[player.ID] as Player).SkinColor);
        msg2.WriteByte((client.Characters[player.ID] as Player).HideBool);
        msg2.WriteByte((client.Characters[player.ID] as Player).FaceShape);
        if (!client.safemode && msg2.BodyData[11] == (byte) 0 && client.Tab.seeinvis.Checked)
        {
          msg2.BodyData[11] = Server.invis;
          msg2.WriteByte((byte) 3);
        }
        else
          msg2.WriteByte(num);
      }
      msg2.WriteString8(str);
      msg2.WriteString8((client.Characters[player.ID] as Player).GroupName);
      client.Characters[player.ID].NameIsRed = false;
      Server.StaticCharacters[player.ID].NameIsRed = false;
      if (!client.safemode)
      {
        if (client.Tab.monitords.Checked && player.Head != ushort.MaxValue && Server.StaticCharacters.ContainsKey(player.ID) && (Server.StaticCharacters[player.ID].hasdemonseal || Server.StaticCharacters[player.ID].hasdemise || Server.StaticCharacters[player.ID].hasdarkerseal || Server.StaticCharacters[player.ID].hasdarkseal))
        {
          client.Characters[player.ID].NameIsRed = true;
          Server.StaticCharacters[player.ID].NameIsRed = true;
          msg2.BodyData[39] = (byte) 1;
        }
        else if (client.Tab.monitords.Checked && player.Head == ushort.MaxValue && Server.StaticCharacters.ContainsKey(player.ID) && (Server.StaticCharacters[player.ID].hasdemonseal || Server.StaticCharacters[player.ID].hasdemise || Server.StaticCharacters[player.ID].hasdarkerseal || Server.StaticCharacters[player.ID].hasdarkseal))
        {
          client.Characters[player.ID].NameIsRed = true;
          Server.StaticCharacters[player.ID].NameIsRed = true;
          msg2.BodyData[21] = (byte) 1;
        }
        else if (client.Tab.vmonitorcurses && player.Head != ushort.MaxValue && Server.StaticCharacters.ContainsKey(player.ID) && Server.StaticCharacters[player.ID].hasardcradh)
        {
          client.Characters[player.ID].NameIsRed = true;
          Server.StaticCharacters[player.ID].NameIsRed = true;
          msg2.BodyData[39] = (byte) 1;
        }
        else if (client.Tab.vmonitorcurses && player.Head == ushort.MaxValue && Server.StaticCharacters.ContainsKey(player.ID) && Server.StaticCharacters[player.ID].hasardcradh)
        {
          client.Characters[player.ID].NameIsRed = true;
          Server.StaticCharacters[player.ID].NameIsRed = true;
          msg2.BodyData[21] = (byte) 1;
        }
      }
      msg2.Write(new byte[3]);
      client.Enqueue(msg2);
      if ((int) player.ID == (int) client.PlayerID)
      {
        msg.Write(new byte[3]);
        client.Enqueue(msg);
      }
      return false;
    }

    public bool ServerMessage_0x34_Legend(Client client, ServerPacket msg)
    {
      string str1 = string.Empty;
      uint key = msg.ReadUInt32();
      msg.Read(55);
      string str2 = msg.ReadString((int) msg.ReadByte());
      int num1 = (int) msg.ReadByte();
      msg.ReadString((int) msg.ReadByte());
      int num2 = (int) msg.ReadByte();
      msg.ReadString((int) msg.ReadByte());
      msg.ReadString((int) msg.ReadByte());
      msg.ReadString((int) msg.ReadByte());
      byte num3 = msg.ReadByte();
      for (int index = 0; index < (int) num3; ++index)
      {
        int num4 = (int) msg.ReadByte();
        int num5 = (int) msg.ReadByte();
        msg.ReadString((int) msg.ReadByte());
        string str3 = msg.ReadString((int) msg.ReadByte());
        if (str3.StartsWith("Mentored by "))
        {
          string str4 = str3.Remove(str3.IndexOf(" - "));
          str1 = str4.Remove(str4.IndexOf("M"), 12);
        }
      }
      if (str1 != string.Empty)
      {
        if (str1.ToLower() == client.Name.ToLower())
        {
          client.rementor = true;
        }
        else
        {
          client.hasamentor = true;
          client.Hasmentortimer = DateTime.UtcNow;
        }
      }
      else
        client.mentoraccept = true;
      if (client.Characters.ContainsKey(key) && client.Characters[key] != null && client.Characters[key] is Player && (client.Characters[key] as Player).Name == string.Empty)
      {
        client.Characters[key].Name = str2;
        if (Server.StaticCharacters.ContainsKey(key))
          Server.StaticCharacters[key].Name = str2;
        if (client.Characters[key].wasseenwarning && !Server.Alts.ContainsKey(client.Characters[key].Name.ToLower()))
        {
          client.SendMessage(client.Characters[key].Name + " was seen!", "red");
          client.Characters[key].wasseenwarning = false;
        }
        if (!client.safemode && client.Tab.seeinvis.Checked)
        {
          msg = new ServerPacket((byte) 51);
          msg.WriteUInt16((ushort) client.Characters[key].Location.X);
          msg.WriteUInt16((ushort) client.Characters[key].Location.Y);
          msg.WriteByte((byte) client.Characters[key].Location.Direction);
          msg.WriteUInt32(client.Characters[key].ID);
          msg.WriteUInt16((client.Characters[key] as Player).Head);
          if ((client.Characters[key] as Player).Head == ushort.MaxValue)
          {
            msg.WriteUInt16((client.Characters[key] as Player).Form);
            msg.WriteUInt32(0U);
            msg.WriteUInt32(0U);
          }
          else
          {
            msg.WriteByte((client.Characters[key] as Player).Body);
            msg.WriteUInt16((client.Characters[key] as Player).Arms);
            msg.WriteByte((client.Characters[key] as Player).Boots);
            msg.WriteUInt16((client.Characters[key] as Player).Armor);
            msg.WriteByte((client.Characters[key] as Player).Shield);
            msg.WriteUInt16((client.Characters[key] as Player).Weapon);
            msg.WriteByte((client.Characters[key] as Player).HeadColor);
            msg.WriteByte((client.Characters[key] as Player).BootColor);
            msg.WriteByte((client.Characters[key] as Player).Acc1Color);
            msg.WriteUInt16((client.Characters[key] as Player).Acc1);
            msg.WriteByte((client.Characters[key] as Player).Acc2Color);
            msg.WriteUInt16((client.Characters[key] as Player).Acc2);
            msg.WriteByte((client.Characters[key] as Player).Unknown);
            msg.WriteUInt16((client.Characters[key] as Player).Acc3);
            msg.WriteByte((client.Characters[key] as Player).Unknown2);
            msg.WriteByte((client.Characters[key] as Player).RestCloak);
            msg.WriteUInt16((client.Characters[key] as Player).Overcoat);
            msg.WriteByte((client.Characters[key] as Player).OvercoatColor);
            msg.WriteByte((client.Characters[key] as Player).SkinColor);
            msg.WriteByte((client.Characters[key] as Player).HideBool);
            msg.WriteByte((client.Characters[key] as Player).FaceShape);
            if (msg.BodyData[11] == (byte) 0)
              msg.BodyData[11] = Server.invis;
          }
          client.Characters[key].NameIsRed = false;
          Server.StaticCharacters[key].NameIsRed = false;
          if (client.Tab.vmonitorcurses && Server.StaticCharacters.ContainsKey(key) && Server.StaticCharacters[key] != null && (Server.StaticCharacters[key].hasardcradh || Server.StaticCharacters[key].hasdemonseal || Server.StaticCharacters[key].hasdemise || Server.StaticCharacters[key].hasdarkerseal || Server.StaticCharacters[key].hasdarkseal))
          {
            client.Characters[key].NameIsRed = true;
            Server.StaticCharacters[key].NameIsRed = true;
            msg.WriteByte((byte) 1);
          }
          else
            msg.WriteByte((byte) 3);
          msg.WriteString8((client.Characters[key] as Player).Name);
          msg.WriteString8((client.Characters[key] as Player).GroupName);
          msg.Write(new byte[3]);
          client.Enqueue(msg);
        }
        client.hidelegend = false;
        return false;
      }
      if (!client.hidelegend)
        return !client.disablelegend;
      client.hidelegend = false;
      return false;
    }

    public bool ServerMessage_0x36_CountryList(Client client, ServerPacket msg)
    {
      client.CountryList.Clear();
      int num1 = (int) msg.ReadUInt16();
      ushort num2 = msg.ReadUInt16();
      for (int index = 0; index < (int) num2; ++index)
      {
        int num3 = (int) msg.ReadByte();
        int num4 = (int) msg.ReadByte();
        int num5 = (int) msg.ReadByte();
        msg.ReadString((int) msg.ReadByte());
        int num6 = (int) msg.ReadByte();
        string str = msg.ReadString((int) msg.ReadByte());
        if (!client.CountryList.Contains(str.ToLower()))
          client.CountryList.Add(str.ToLower());
      }
      if (!client.manualopencountrylist)
        return true;
      client.manualopencountrylist = false;
      return false;
    }

    public bool ServerMessage_0x37_AddAppendage(Client client, ServerPacket msg)
    {
      byte num1 = msg.ReadByte();
      int num2 = (int) msg.ReadUInt16();
      int num3 = (int) msg.ReadByte();
      string str = msg.ReadString((int) msg.ReadByte());
      int num4 = (int) msg.ReadByte();
      uint num5 = msg.ReadUInt32();
      uint num6 = msg.ReadUInt32();
      if (num5 > 0U && (num6 < 800U && num5 > 1000U || (double) num6 / (double) num5 * 100.0 < 80.0) && !client.HasLowDuraDN())
        client.needsrepaired = true;
      switch (num1)
      {
        case 1:
          client.staffnow = str;
          break;
        case 2:
          client.manualremovedarmor = false;
          client.removedarmordelay = DateTime.MinValue;
          client.removedarmor = string.Empty;
          client.armornow = str;
          break;
        case 3:
          client.ShieldOn = true;
          break;
        case 6:
          client.Necklace = str;
          break;
        case 15:
          client.Overcoat = str;
          break;
        case 16:
          client.Overhat = str;
          break;
      }
      return true;
    }

    public bool ServerMessage_0x38_RemoveAppendage(Client client, ServerPacket msg)
    {
      switch (msg.ReadByte())
      {
        case 1:
          client.staffnow = string.Empty;
          break;
        case 2:
          if (client.armornow != string.Empty)
          {
            client.removedarmordelay = DateTime.UtcNow;
            client.removedarmor = client.armornow;
          }
          client.armornow = string.Empty;
          break;
        case 3:
          client.ShieldOn = false;
          break;
        case 6:
          client.Necklace = string.Empty;
          break;
        case 15:
          client.Overcoat = string.Empty;
          break;
        case 16:
          client.Overhat = string.Empty;
          break;
      }
      return true;
    }

    public bool ServerMessage_0x39_Profile(Client client, ServerPacket msg)
    {
      client.ihaveamentor = false;
      bool flag1 = false;
      bool flag2 = false;
      bool flag3 = false;
      bool flag4 = false;
      bool flag5 = false;
      if (client.Tab.improveskill.Text.Equals("Tailoring (cowl)") && client.Tab.impskillbutton.Text == "Stop")
        flag2 = true;
      else if (client.Tab.improveskill.Text.Equals("Tailoring") && client.Tab.impskillbutton.Text == "Stop")
        flag2 = true;
      else if (client.Tab.improveskill.Text.Contains("Herbalist") && client.Tab.impskillbutton.Text == "Stop")
        flag3 = true;
      else if (client.Tab.improveskill.Text.Equals("Wizardry Researcher") && client.Tab.impskillbutton.Text == "Stop")
        flag4 = true;
      else if (client.Tab.improveskill.Text.Equals("Elementalist") && client.Tab.impskillbutton.Text == "Stop")
        flag5 = true;
      client.GroupMembers.Clear();
      client.Nation = msg.ReadByte();
      msg.ReadString((int) msg.ReadByte());
      msg.ReadString((int) msg.ReadByte());
      string str1 = msg.ReadString((int) msg.ReadByte());
      int num1 = (int) msg.ReadByte();
      msg.ReadString((int) msg.ReadByte());
      int num2 = (int) msg.ReadByte();
      client.Medenian = msg.ReadByte();
      int num3 = (int) msg.ReadByte();
      msg.ReadString((int) msg.ReadByte());
      msg.ReadString((int) msg.ReadByte());
      byte num4 = msg.ReadByte();
      for (int index = 0; index < (int) num4; ++index)
      {
        int num5 = (int) msg.ReadByte();
        int num6 = (int) msg.ReadByte();
        string key = msg.ReadString((int) msg.ReadByte());
        string text = msg.ReadString((int) msg.ReadByte());
        if (!client.LegendMarks.ContainsKey(key))
          client.LegendMarks.Add(key, text);
        if (flag2 && text.Contains("Tailor ("))
          client.SendMessage(text, (byte) 18);
        else if (flag3 && text.Contains("Herbalist ("))
          client.SendMessage(text, (byte) 18);
        else if (flag4 && text.Contains("Wizardry Researcher ("))
          client.SendMessage(text, (byte) 18);
        else if (flag5 && text.Contains("Elementalist ("))
          client.SendMessage(text, (byte) 18);
        else if (text.Contains("Dugon by"))
        {
          string str2 = text.Substring(0, text.IndexOf(' '));
          if (str2 == "White")
            client.currentdugon = "Green";
          else if (str2 == "Green")
            client.currentdugon = "Blue";
          else if (str2 == "Blue")
            client.currentdugon = "Yellow";
          else if (str2 == "Yellow")
            client.currentdugon = "Purple";
          else if (str2 == "Purple")
            client.currentdugon = "Brown";
          else if (str2 == "Brown")
            client.currentdugon = "Red";
          else if (str2 == "Red")
            client.currentdugon = "Black";
          flag1 = true;
        }
        else if (text.Contains("Mentored by "))
          client.ihaveamentor = true;
      }
      if (!flag1)
        client.currentdugon = "White";
      if (str1.Contains("Total"))
      {
        string[] strArray = str1.Substring(14, str1.IndexOf("Total") - 14).Split(new string[1]
        {
          "\n"
        }, 14, StringSplitOptions.None);
        for (int index = 0; index < strArray.Length - 1; ++index)
        {
          strArray[index] = strArray[index].Trim();
          if (strArray[index].StartsWith("* "))
            strArray[index] = strArray[index].Remove(0, 2);
          if (strArray[index] != client.Name)
            client.GroupMembers.Add(strArray[index]);
        }
      }
      return true;
    }

    public bool ServerMessage_0x3A_SpellBar(Client client, ServerPacket msg)
    {
      ushort num1 = msg.ReadUInt16();
      byte num2 = msg.ReadByte();
      if (num2 > (byte) 0)
      {
        if (num1 == (ushort) 89 && num2 != (byte) 6)
        {
          client.IsSkulled = true;
          Server.StaticCharacters[client.PlayerID].IsSkulled = true;
        }
        if (!client.SpellBar.Contains(num1))
          client.SpellBar.Add(num1);
        if (num2 == (byte) 1 && num1 == (ushort) 10 && client.Tab.vselfhide && !client.pause)
        {
          client.hidetime = DateTime.UtcNow;
          client.MacroCast("Hide", new uint?());
          client.MacroCast("White Bat Stance", new uint?());
          if (client.CanSkill("Claw Fist"))
            client.UseSkill("Claw Fist");
          else if (client.CanSkill("ao beag suain"))
            client.UseSkill("ao beag suain");
          else
            client.UseSkill("Assail");
          client.MacroCast("Hide", new uint?());
          client.MacroCast("White Bat Stance", new uint?());
        }
      }
      else
      {
        if (num1 == (ushort) 8 && Program.MainForm.expalert.Checked && !client.Tab.expapbonus.Checked)
        {
          if (!Server.SentryAlarm)
          {
            Server.SentryAlarm = true;
            Server.alarm = new SoundPlayer(Assembly.GetExecutingAssembly().GetManifestResourceStream("Flintstones.Ding.wav"));
            Server.alarmTimer = DateTime.UtcNow;
            Server.alarm.Play();
          }
          client.SendMessage(client.Name + "'s bonus ran out", "red", true);
        }
        if (num1 == (ushort) 148 && Program.MainForm.expalert.Checked && !client.Tab.xpshroom.Checked)
        {
          if (!Server.SentryAlarm)
          {
            Server.SentryAlarm = true;
            Server.alarm = new SoundPlayer(Assembly.GetExecutingAssembly().GetManifestResourceStream("Flintstones.Ding.wav"));
            Server.alarmTimer = DateTime.UtcNow;
            Server.alarm.Play();
          }
          client.SendMessage(client.Name + "'s bonus ran out", "red", true);
        }
        if (num1 == (ushort) 147 && Program.MainForm.expalert.Checked && !client.Tab.useskillbonus.Checked && !client.Tab.abrune.Checked)
        {
          if (!Server.SentryAlarm)
          {
            Server.SentryAlarm = true;
            Server.alarm = new SoundPlayer(Assembly.GetExecutingAssembly().GetManifestResourceStream("Flintstones.Ding.wav"));
            Server.alarmTimer = DateTime.UtcNow;
            Server.alarm.Play();
          }
          client.SendMessage(client.Name + "'s bonus ran out", "red", true);
        }
        client.SpellBar.Remove(num1);
      }
      return true;
    }

    public bool ServerMessage_0x3F_Cooldown(Client client, ServerPacket msg)
    {
      int num1 = (int) msg.ReadByte();
      byte num2 = msg.ReadByte();
      uint num3 = msg.ReadUInt32();
      client.ImCasting = false;
      if (num1 == 0)
      {
        if (num2 > (byte) 0)
        {
          Spell spell = client.SpellBook[(int) num2 - 1];
          if (spell != null)
          {
            client.GlobalSpellCD = DateTime.UtcNow;
            spell.NextUse = num3 <= 0U ? DateTime.UtcNow.AddMilliseconds(335.0) : DateTime.UtcNow.AddMilliseconds((double) (num3 * 1000U));
          }
        }
      }
      else if (num2 > (byte) 0)
      {
        Skill skill = client.SkillBook[(int) num2 - 1];
        if (skill != null)
          skill.NextUse = num3 <= 0U ? DateTime.UtcNow.AddMilliseconds(335.0) : DateTime.UtcNow.AddMilliseconds((double) (num3 * 1000U));
      }
      return true;
    }

    public bool ServerMessage_0x42_ExchangeWindow(Client client, ServerPacket msg)
    {
      int num1 = (int) msg.ReadByte();
      if (num1 == 0)
      {
        client.cancast = false;
        client.canskill = false;
        client.donotwalk = true;
        int num2 = (int) msg.ReadUInt32();
        msg.ReadString((int) msg.ReadByte());
      }
      if (num1 == 2)
      {
        int num3 = (int) msg.ReadByte();
        int num4 = (int) msg.ReadByte();
        int num5 = (int) msg.ReadUInt16();
        int num6 = (int) msg.ReadByte();
        msg.ReadString((int) msg.ReadByte());
      }
      if (num1 == 3)
      {
        int num7 = (int) msg.ReadByte();
        int num8 = (int) msg.ReadUInt32();
      }
      if (num1 == 4)
      {
        client.cancast = true;
        client.canskill = true;
        client.donotwalk = false;
        int num9 = (int) msg.ReadByte();
        msg.ReadString((int) msg.ReadByte());
      }
      if (num1 == 5)
      {
        client.cancast = true;
        client.canskill = true;
        client.donotwalk = false;
        int num10 = (int) msg.ReadByte();
        msg.ReadString((int) msg.ReadByte());
      }
      return true;
    }

    public bool ServerMessage_0x4C_LogOffSignal(Client client, ServerPacket msg)
    {
      client.manuallog = true;
      client.LoggedOn = false;
      return true;
    }

    public bool ServerMessage_0x60_OK(Client client, ServerPacket msg)
    {
      if (Server.Relog.Count > 0)
      {
        foreach (Relog relog in Server.Relog.Values)
        {
          if (relog != null && relog.WaitForOk)
          {
            client.stream = new ProcessMemoryStream(relog.Process.Id, ProcessAccess.VmOperation | ProcessAccess.VmRead | ProcessAccess.VmWrite);
            client.stream.Position = client.nameadd;
            byte[] numArray = new byte[relog.Name.Length];
            client.stream.Read(numArray, 0, relog.Name.Length);
            string key = Encoding.ASCII.GetString(numArray);
            if (Server.Relog[key].WaitForOk)
            {
              Server.Relog[key].WaitForOk = false;
              break;
            }
          }
        }
      }
      return true;
    }

    public bool ServerMessage_0x67_WorldMapResponse(Client client, ServerPacket msg)
    {
      client.mapresponse = true;
      return true;
    }
  }
}
