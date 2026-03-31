using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Flintstones
{
  internal class SpeakCommands
  {
    private Dictionary<string, Action<Client, string[]>> Commands { get; }

    string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

    Client _client;

    /// <summary>
    /// List of spells used to attack boss (Count/Countess)
    /// </summary>
    private readonly List<string> AttackBossSpells = new List<string>
    {
      "ard pian na dion",
      "mor pian na dion",
      "Shock Arrow",
      "Frost Arrow",
      "Frost + 3 Shocks",
      "Cursed Tune",
    };
    
    /// <summary>
    /// Provides a list of item abbreviations to convert them to their full descriptions.
    /// </summary>
    private readonly Dictionary<string, string> ItemAbbreviations = new Dictionary<string, string>
    {
      { "suc",  "succubus's hair" },
      { "war", "warranty bag" },
      { "xmas d", "xmas double exp-ap(x5)" },
      { "double", "double bonus exp-ap(x5)" },
      { "christmas", "christmas double exp-ap(x5)" },
      { "vday", "vday bonus exp-ap(x5)" },
      { "gsf", "golden starfish" },
      { "bog", "band of gales" },
      { "bos", "band of storms" },
      { "divinities", "divinities staff" },
      { "eagles", "eagles grasp" },
      { "hell", "hellreavers blade" },
      { "inferno", "inferno blade" },
      { "satchel", "satchel of goods" },
      { "skylight", "skylight staff" },
      { "thunder", "thunderfury" },
    };

    public SpeakCommands(Client client)
    {
      _client = client;
      Commands = new Dictionary<string, Action<Client, string[]>>(StringComparer.OrdinalIgnoreCase);

      // Register aliases for a command
      // Generic play commands
      Register(Stop, "/s");
      Register(Play, "/p");
      Register(StopWalking, "/stop");
      Register(SlowWalking, "/slow");
      Register(MediumWalking, "/medium", "/med");
      Register(FastWalking, "/fast");
      Register(Walk, "/walk");
      Register(Follow, "/follow");
      Register(SaveTemplate, "/save");
      Register(LoadTemplate, "/load");
      Register(Calculator,"/calc");
      Register(SafeWalking, "/safe");
      Register(ReadMail, "/readmail");
      Register(CreateSong, "/song");
      Register(QuestTimer, "/quest");
      Register(BossTimer, "/boss");
      Register(ToggleBodyAnimation, "/b");
      Register(ToggleDion, "/t");
      Register(ToggleWalls, "/n");
      Register(BuffIcons, "/icon");
      Register(MapID, "/map");
      Register(ItemID, "/item");
      Register(Legend, "/legend", "/test");

      // Bank and inventory commands
      Register(DropItems, "/drop");
      Register(WithdrawItems, "/withdraw");
      Register(LegalWithdraw, "/w");
      Register(LegalWithdrawAll, "/wa");
      Register(DepositItems, "/deposit");
      Register(LegalDeposit, "/d");
      Register(LegalDepositAll, "/da");
      Register(SendItems, "/send");
      Register(ReceiveItems, "/rec");
      Register(RepairAll, "/repair");
      Register(BuyItems, "/buy");
      Register(ShowMoney, "/c?");
      Register(Beggar, "/beg");
      Register(BuyFiorSrad, "/fiorsread");
      Register(BankList, "/banklist");
      Register(BuyAllPots, "/balls");

      Register(SwitchMonsterForm, "/m");
      Register(RemoveMonsters, "/ram");
      Register(Hide, "/hide");
      Register(GiveLabor, "/labor");
      Register(Group, "/g");
      Register(ForceGroup, "/fg");
      Register(CastDiabean, "/diabean", "/fiobean");
      Register(CastGramReflect, "/ref");
      Register(CastGramAoSith, "/ao");
      Register(Higgle, "/higgle");

      // Alternative walking commands (other than /walk).
      Register(ToCWarrior, "/warrior");
      Register(ToCMonk, "/monk");
      Register(ToCRogue, "/rogue");
      Register(ToCPriest, "/priest");
      Register(ToCWizard, "/wizard");
      Register(Ascend, "/ascend");
      Register(AscendHP, "/ascendhp", "/hp");
      Register(AscendMP, "/ascendmp", "/mp");
      Register(Dojo, "/dojo");
      Register(TeachDugon, "/teach");
      Register(DoneDugon, "/done");

      // Quest commands
      Register(DarkMaze, "/darkmaze");
      Register(DragonScaleSword, "/tent", "/tentacle", "/dss");
      Register(GiantPearl, "/pearl");
      Register(HalfTalisman, "/tali", "/halftalisman");
      Register(Law, "/law");
      Register(MothersLove, "/molo", "/motherslove");
      Register(StoneSlabs, "/slab", "/stoneslab");
      Register(WaterDungeon, "/wd");
      Register(TheLetter, "/thel", "/theletter");

      // Seasonal event commands
      Register(SunProtection, "/hat", "/sunprotection");
      Register(BeachAttire, "/attire");
      Register(MakeAWish, "/wish");
      Register(Attack, "/attack");
      Register(YuleLogs, "/yule");
      Register(Frosty, "/frosty");
      Register(FilthyErbies, "/meg", "/filthyerbies");
      Register(CountUpErbies, "/count+");
      Register(CountDownErbies, "/count-");
      Register(ShowCountedErbies, "/count");


    }

    public void Run(CancellationToken token)
    {
      Console.WriteLine($"Speaking thread of {_client.Name} Started");

      while (!token.IsCancellationRequested)
      {
        if (_client.SpeakMessage != string.Empty)
        {
          bool commandExecuted = false;
          if (TryExecuteCommand(_client.SpeakMessage, _client))
          {
            commandExecuted = true;
          }

          _client.SpeakMessage = string.Empty;
          if (!commandExecuted)
          {
            _client.SendMessage($"Command not implemented: {_client.SpeakMessage}", "pink");
          }
        }

        Thread.Sleep(200);
      }

      Console.WriteLine($"Speak command Thread of {_client.Name} stopped");
    }

    /// <summary>
    /// Registers aliases for a command script.
    /// </summary>
    /// <remarks>If multiple aliases are provided, each will be mapped to the same handler. Existing handlers
    /// for the specified aliases will be overwritten.</remarks>
    /// <param name="action">The delegate to execute when a command matching one of the provided aliases is invoked. Cannot be null.</param>
    /// <param name="aliases">An array of command aliases that will be associated with the handler. Each alias must be a non-empty string.</param>
    private void Register(Action<Client, string[]> action, params string[] aliases)
    {
      foreach (var alias in aliases)
      {
        Commands[alias] = action;
      }
    }

    /// <summary>
    /// Attempts to execute the command.
    /// </summary>
    /// <remarks>If the command does not exist in the command registry, no action is performed and the method
    /// returns false. This method does not throw an exception if the command is not found.</remarks>
    /// <param name="input">The speak command to execute. Can have parameters, cannot be null or empty.</param>
    /// <param name="client">The client class reference. Cannot be null.</param>
    /// <returns>true if the command was found and executed; otherwise, false.</returns>
    public bool TryExecuteCommand(string input, Client client)
    {
      if (string.IsNullOrWhiteSpace(input))
        return false;

      // Split input by spaces
      var parts = input.Split(' ', (char)StringSplitOptions.RemoveEmptyEntries);

      // Try longest command match first 
      for (int i = parts.Length; i > 0; i--)
      {
        var command = string.Join(" ", parts.Take(i));

        if (Commands.TryGetValue(command, out var action))
        {
          var args = parts.Skip(i).ToArray();
          action(client, args);
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Initializes walking mode, enabling autowalk and updating the user interface.
    /// </summary>
    /// <remarks>This method enables autowalk, updates UI controls, and ensures the bot thread is
    /// running. Call this method to start walking mode.</remarks>
    /// <param name="client">The client instance for which walking mode is to be initialized. Cannot be null.</param>
    private void InitWalking(Client client)
    {
      client.Tab.autowalker_button.Text = "Stop";
      client.autowalkon = true;
      //if (!client.BotThread.IsAlive)
      //  client.BotThread.Start();
      client.pause = false;
      client.Tab.btnPlay.Enabled = false;
      client.Tab.btnStop.Enabled = true;
      client.Tab.fastwalk.Checked = true;
    }

    private uint findCloseMundane(Client client)
    {
      uint npcID = 0;
      Npc[] npcList = client.NearbyNpcs(Npc.NpcType.Mundane);
      if (npcList != null && npcList.Length > 0)
      {
        npcID = npcList[0].ID;
      }

      return npcID;
    }

    /// <summary>
    /// Stops the client.
    /// </summary>
    /// <param name="client">The client instance to stop. Cannot be null.</param>
    /// <param name="args">Argument given to /s, can only by "all" to play all clients</param>
    public void Stop(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        //if (!client.BotThread.IsAlive)
        //  client.BotThread.Start();
        client.pause = false;
        client.Tab.btnPlay.Enabled = false;
        client.Tab.btnStop.Enabled = true;
        client.lastaction = DateTime.UtcNow;
        client.laststep = DateTime.UtcNow;
        client.SendMessage($"{client.Name} is stopped.", "grey");
      }
      else
      {
        // If "all" parameter is provided, call Stop without parameters for each client
        if (args[0].Equals("all", StringComparison.CurrentCulture))
        {
          Client[] runningClients = Server.Alts.Values.ToArray<Client>();
          for (int clientNr = 0; clientNr < runningClients.Length; ++clientNr)
          {
            Stop(runningClients[clientNr], null);
          }
          client.SendMessage("All clients are stopped.", "grey");
        }
      }
    }

    /// <summary>
    /// Starts the client.
    /// </summary>
    /// <param name="client">The client instance whose bot will be started. Cannot be null.</param>
    /// <param name="args">Argument given to /p, can only by "all" to play all clients</param>
    public void Play(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        //if (!client.BotThread.IsAlive)
        //  client.BotThread.Start();
        client.pause = false;
        client.Tab.btnPlay.Enabled = false;
        client.Tab.btnStop.Enabled = true;
        client.lastaction = DateTime.UtcNow;
        client.laststep = DateTime.UtcNow;
        client.SendMessage($"{client.Name} is Playing.", "grey");
      } 
      else
      {
        // If "all" parameter is provided, call Play without parameters for each client
        if (args[0].Equals("all", StringComparison.CurrentCulture))
        {
          Client[] runningClients = Server.Alts.Values.ToArray<Client>();
          for (int clientNr = 0; clientNr < runningClients.Length; ++clientNr)
          {
            Play(runningClients[clientNr], null);
          }
          client.SendMessage("All clients are playing.", "grey");
        }
      }
    }

    /// <summary>
    /// Stops the client's automatic walking .
    /// </summary>
    /// <param name="client">The client instance for which to stop automatic walking. Cannot be null.</param>
    /// <param name="args">Either "all" or null</param>
    public void StopWalking(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.Tab.followplayer.Checked = false;
        client.Tab.autowalker_button.Text = "Start";
        client.autowalkon = false;
        client.Tab.wayregionson.Checked = false;
        client.Tab.actonlyinmobs.Checked = false;
        client.SendMessage("You stopped walking.", "grey");
      }
      else
      {
        // If "all" parameter is provided, call StopWalking without parameters for each client
        if (args[0].Equals("all", StringComparison.CurrentCulture))
        {
          Client[] runningClients = Server.Alts.Values.ToArray<Client>();
          for (int clientNr = 0; clientNr < runningClients.Length; ++clientNr)
          {
            StopWalking(runningClients[clientNr], null);
          }
          client.SendMessage("All clients stopped walking.", "grey");
        }

      }
    }

    /// <summary>
    /// Stops the client's automatic walking .
    /// </summary>
    /// <param name="client">The client instance for which to stop automatic walking. Cannot be null.</param>
    /// <param name="args">Either "all" or null</param>
    public void SlowWalking(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.Tab.mediumwalk.Checked = false;
        client.Tab.fastwalk.Checked = false;
        client.SendMessage("You walk at slow pace.", "grey");
      }
      else
      {
        // If "all" parameter is provided, call SlowWalking without parameters for each client
        if (args[0].Equals("all", StringComparison.CurrentCulture))
        {
          Client[] runningClients = Server.Alts.Values.ToArray<Client>();
          for (int clientNr = 0; clientNr < runningClients.Length; ++clientNr)
          {
            SlowWalking(runningClients[clientNr], null);
          }
          client.SendMessage("All clients walk at slow pace.", "grey");
        }
      }
    }

    /// <summary>
    /// Sets the client's to medium speed walking.
    /// </summary>
    /// <param name="client">The client instance for which to stop automatic walking. Cannot be null.</param>
    /// <param name="args">Either "all" or null</param>
    public void MediumWalking(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.Tab.mediumwalk.Checked = true;
        client.Tab.fastwalk.Checked = false;
        client.SendMessage("You walk at medium pace.", "grey");
      }
      else
      {
        // If "all" parameter is provided, call MediumWalking without parameters for each client
        if (args[0].Equals("all", StringComparison.CurrentCulture))
        {
          Client[] runningClients = Server.Alts.Values.ToArray<Client>();
          for (int clientNr = 0; clientNr < runningClients.Length; ++clientNr)
          {
            MediumWalking(runningClients[clientNr], null);
          }
          client.SendMessage("All clients walk at medium pace.", "grey");
        }
      }
    }

    /// <summary>
    /// Sets the client's to medium speed walking.
    /// </summary>
    /// <param name="client">The client instance for which to stop automatic walking. Cannot be null.</param>
    /// <param name="args">Either "all" or null</param>
    public void FastWalking(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.Tab.mediumwalk.Checked = false;
        client.Tab.fastwalk.Checked = true;
        client.SendMessage("You walk at fast pace.", "grey");
      }
      else
      {
        // If "all" parameter is provided, call FastWalking without parameters for each client
        if (args.Length > 0 && args[0].Equals("all", StringComparison.CurrentCulture))
        {
          Client[] runningClients = Server.Alts.Values.ToArray<Client>();
          for (int clientNr = 0; clientNr < runningClients.Length; ++clientNr)
          {
            FastWalking(runningClients[clientNr], null);
          }
          client.SendMessage("All clients walk at fast pace.", "grey");
        }
      }
    }

    /// <summary>
    /// Walk to a given location or, if specified moving all clients to that location.
    /// </summary>
    /// <remarks>If the first argument is 'all', the method moves all clients to the specified location.</remarks>
    /// <param name="client">The client instance that will execute the walk command and receive any resulting messages.</param>
    /// <param name="args">An array of strings representing the command arguments. The first argument may be 'all' to indicate that all
    /// clients should be moved, followed by the name of the destination location.</param>
    public void Walk(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.SendMessage("Please provide a location.", "pink");
        return;
      }

      bool walkAll = false; // Only walk this client
      if (args[0].Equals("all", StringComparison.CurrentCulture))
      {
        args = args.Skip(1).ToArray(); // remove "all" from arguments
        walkAll = true; // Walk all clients
      } 

      string SpeakMessage = string.Join(" ", args).ToLower();
      if (Server.WalkLocations.ContainsKey(SpeakMessage))
      {
        var location = Server.WalkLocations[SpeakMessage];
        client._walkCommands.StartAutoWalk(location.Area, location.Location, walkAll);
      } 
      else 
      {
        client.SendMessage("That location does not exist.", "pink");
      }
    }

    /// <summary>
    /// Initiates following a specified player.
    /// </summary>
    /// <remarks>If the first argument is 'all', the method will attempt to follow all clients. If the second
    /// argument is provided and is a valid integer, it sets the following distance.</remarks>
    /// <param name="client">The client that will follow the specified player or players.</param>
    /// <param name="args">Can be "all", player name, or player name and distance to follow</param>
    public void Follow(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.SendMessage("Please provide a player name to follow. /follow player (distance)", "pink");
        return;
      }

      if (args[0].Equals("all", StringComparison.CurrentCulture))
      {
        args = args.Skip(1).ToArray(); // remove "all" from arguments

        // Call this function without "all" for each client
        if (args.Length > 0 && args[0].Equals("all", StringComparison.CurrentCulture))
        {
          Client[] runningClients = Server.Alts.Values.ToArray<Client>();
          for (int clientNr = 0; clientNr < runningClients.Length; ++clientNr)
          {
            Follow(runningClients[clientNr], args);
          }
          client.SendMessage($"All clients follow {args[0]}.", "grey");
        }
      }
      else
      {
        if (args[0].Equals(client.Name, StringComparison.CurrentCultureIgnoreCase))
        {
          client.SendMessage("You cannot follow yourself.", "pink");
          return;
        }
        string playerName = args[0];
        client.Tab.followplayer.Checked = true;
        client.Tab.followtarget.Text = playerName;

        int distance;
        if (args.Length == 2 && int.TryParse(args[1], out distance))
          client.Tab.followdist.Value = distance;

        client.SendMessage($"Now following {playerName}.", "grey");
      }
    }

    /// <summary>
    /// Saves a template, or for all connected clients if the 'all' parameter is provided.
    /// </summary>
    /// <remarks>If the 'args' parameter is null or empty, the method prompts the client to provide a template
    /// name. When 'all' is specified as the first argument, the template is saved for each connected client.</remarks>
    /// <param name="client">The client for whom the template is being saved.</param>
    /// <param name="args">Can be "all" and the template name to save</param>
    public void SaveTemplate(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.SendMessage("Please provide a template. /load (all) templateName", "pink");
        return;
      }

      if (args[0].Equals("all", StringComparison.CurrentCulture))
      {
        args = args.Skip(1).ToArray(); // remove "all" from arguments

        // Call this function without "all" for each client
        if (args.Length > 0 && args[0].Equals("all", StringComparison.CurrentCulture))
        {
          foreach (var runningClient in Server.Clients)
          {
            SaveTemplate(runningClient, args);
          }
          client.SendMessage($"Template {args[0]} saved for all clients.", "grey");
        }
      }
      else
      {
        string templateName = string.Join(" ", args).ToLower();
        client.Tab.SaveTemplate(templateName);
        client.SendMessage($"Template {templateName} saved.", "grey");
      }
    }

    /// <summary>
    /// Loads a template, or for all clients if the 'all' parameter is provided.
    /// </summary>
    /// <remarks>If no template name is provided, a message is sent to the client requesting a valid template.
    /// When 'all' is specified, the method recursively loads the template for each connected client.</remarks>
    /// <param name="client">The client for whom the template is being loaded.</param>
    /// <param name="args">Can be "all" and the template name to load</param>
    public void LoadTemplate(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.SendMessage("Please provide a template. /load (all) templateName", "pink");
        return;
      }

      if (args[0].Equals("all", StringComparison.CurrentCultureIgnoreCase))
      {
        args = args.Skip(1).ToArray(); // remove "all" from arguments

        // If "all" parameter is provided, call this function without "all" for each client
        foreach (var runningClient in Server.Clients)
        {
          LoadTemplate(runningClient, args);
        }

        client.SendMessage($"Template loaded for all clients.", "grey");
      }
      else
      {
        string templateName = string.Join(" ", args).ToLower();
        client.Tab.LoadTemplate(templateName);
      }
    }

    /// <summary>
    /// Give item in inventory to a beggar. 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args">Item</param>
    public void Beggar(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.SendMessage("Please provide item to give to beggar. /beg item", "pink");
        return;
      }

      string item = string.Join(" ", args).ToLower();
      if (client.HasItem(item))
      {
        Npc npcByName = client.FindNpcByName<Npc>("Beggar");
        if (npcByName != null && npcByName.IsOnScreen)
        {
          client.DialogueRespond(npcByName.ID, (byte)10, (byte)155);
          client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)0, (byte)0, (byte)2);
          client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)0, (byte)0, (byte)2);
          client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)2, item);
          client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)0, (byte)0, (byte)2, (byte)1, (byte)1);
          client.PopupRespond(new uint?(npcByName.ID), (byte)0, (byte)0, (byte)0, (byte)1);
        }
        else
        {
          client.SendMessage("No beggar near.", "pink");
        }
      }
    }


    /// <summary>
    /// Buy Fior Srad at the black market.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public void BuyFiorSrad(Client client, string[] args)
    {
      client.buyfiorsrads = true;
    }

    /// <summary>
    /// Get a list of everything in the bank and store it in banklist.txt
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public void BankList(Client client, string[] args)
    {
      Npc[] source = client.NearbyNpcs(Npc.NpcType.Mundane);
      if (source.Count<Npc>() > 0)
      {
        client.banklist = true;
        client.DialogueRespond(new uint?(source[0].ID), "Withdraw");
      }
    }

    /// <summary>
    /// Buying potions
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public void BuyAllPots(Client client, string[] args)
    {
      if (!client.buyballpots)
      {
        client.SendMessage("Buying potions.");
        client.buyballpots = true;
      }
      else
      {
        client.SendMessage("Stopped buying potions.");
        client.buyballpots = false;
      }
    }

    /// <summary>
    /// Switches the monster form  If no arguments are supplied, toggles the monster form state.
    /// </summary>
    /// <remarks>If the arguments are invalid or not provided, a message is sent to the client prompting for a
    /// valid monster ID in the range of 1 to 255.</remarks>
    /// <param name="client">The client whose monster form is to be switched. Cannot be null.</param>
    /// <param name="args">Either null or the monster ID</param>
    public void SwitchMonsterForm(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.Tab.usemonster.Checked = !client.Tab.usemonster.Checked;
        return;
      }

      if (int.TryParse(args[0], out int id)) 
      {
        client.Tab.usemonsterid.Value = id;
        client.Tab.usemonster.Checked = true;
      }

      client.SendMessage("Provide a monster ID (1 - 255) to switch to that form.", "pink");
    }

    /// <summary>
    /// Calculator command to start, stop, or reset the XP/AB/Gold calculator. If no arguments are provided, the current stats are send to the client.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public void Calculator(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.Tab.Calculator_Message();
      } else if (args[0].Equals("on", StringComparison.CurrentCultureIgnoreCase))
      {
        if (client.Tab.calc_toggle.Text == "Start")
        {
          client.Tab.StartCalc();
          client.Tab.calc_toggle.Text = "Pause";
        }
      }
      else if (args[0].Equals("off", StringComparison.CurrentCultureIgnoreCase))
      {
        if (client.Tab.calc_toggle.Text == "Pause")
          client.Tab.StopCalc();
      }
      else if (args[0].Equals("reset", StringComparison.CurrentCultureIgnoreCase))
      {
        if (client.Tab.calc_toggle.Text == "Start")
          client.Tab.ResetCalc();
      }
       else
      {
        client.SendMessage("Use '/calc', '/calc on', '/calc off', '/calc reset'.", "pink");
      }
    }

    public void SafeWalking(Client client, string[] args)
    {
      if (client.safemode)
      {
        foreach (Client runningClient in Server.Clients)
        {
          if (!(runningClient is null))
          {
            runningClient.safemode = false;
            runningClient.Tab.safemode.Checked = false;
            runningClient.EnableSeeInvis();
            if (runningClient.Tab.nowalls.Checked)
              runningClient.DisableWalls();
            runningClient.Refresh();
          }
        }
        User32.Show();
      }
      else
      {
        foreach (Client runningClient in Server.Clients)
        {
          if (!(runningClient is null))
          {
            if (runningClient != null)
            {
              runningClient.SendMessage("", (byte)18);
              runningClient.safemode = true;
              runningClient.DisableSeeInvis();
              runningClient.EnableWalls();
              runningClient.Refresh();
            }
          }
        }
        User32.Hide();
      }
    }

    /// <summary>
    ///  Open all mail so they appear read.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public void ReadMail(Client client, string[] args)
    {
      client.openallmails();
    }

    /// <summary>
    /// Look for prayer possibility and create a song if possible.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args">null</param>
    public void CreateSong(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.SendMessage("Please provide a song to use. /song songname", "pink");
        return;
      }

      if (client.cancast)
      {
        string godName = string.Empty;

        foreach (Spell spell in client.SpellBook)
        {
          if (!(spell is null) && spell.Name.Contains("prayer"))
          {
            godName = spell.Name.Substring(0, spell.Name.IndexOf(' '));
            break;
          }
        }
        if (client.HasSpell(godName + " Prayer") && !client.MapInfo.Tiles[client.ServerLocation.X, client.ServerLocation.Y].HasPrayerSpell)
        {
          client.MacroCast(godName + " Prayer", new uint?());
          Thread.Sleep(1200);
        }

        if (client.HasItem(godName + " Prayer Necklace") && client.MapInfo.Tiles[client.ServerLocation.X, client.ServerLocation.Y].SafeToDropNecklace)
        {
          client.Drop(client.ServerLocation.X, client.ServerLocation.Y, client.ItemSlot(godName + " Prayer Necklace"), 1);
          Thread.Sleep(1200);
          client.Pickup(client.ServerLocation.X, client.ServerLocation.Y);
          Thread.Sleep(1200);
          uint currentnpcpopupId = client.CurrentnpcpopupID;
          client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)30, (byte)0, (byte)92, (byte)1, (byte)2, (byte)4);

          switch (godName)
          {
            case "Gramail":
              client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)34, (byte)0, (byte)248, (byte)1, (byte)2, (byte)4);
              client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)34, (byte)2, (byte)109, (byte)1, (byte)5, (byte)4);
              break;
            case "Fiosachd":
              client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)35, (byte)0, (byte)248, (byte)1, (byte)2, (byte)4);
              client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)35, (byte)2, (byte)94, (byte)1, (byte)7, (byte)4);
              break;
            case "Deoch":
              client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)30, (byte)0, (byte)252, (byte)1, (byte)2, (byte)4);
              client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)30, (byte)2, (byte)78, (byte)1, (byte)7, (byte)4);
              break;
            case "Luathas":
              client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)33, (byte)0, (byte)248, (byte)1, (byte)2, (byte)4);
              client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)33, (byte)2, (byte)42, (byte)1, (byte)6, (byte)4);
              break;
            case "Sgrios":
              client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)37, (byte)0, (byte)248, (byte)1, (byte)2, (byte)4);
              client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)37, (byte)2, (byte)105, (byte)1, (byte)6, (byte)4);
              break;
            case "Glioca":
              client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)31, (byte)0, (byte)248, (byte)1, (byte)2, (byte)4);
              client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)31, (byte)2, (byte)80, (byte)1, (byte)8, (byte)4);
              break;
            case "Cail":
              client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)32, (byte)0, (byte)248, (byte)1, (byte)2, (byte)4);
              client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)32, (byte)2, (byte)45, (byte)1, (byte)6, (byte)4);
              break;
            case "Ceannlaidir":
              client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)36, (byte)0, (byte)248, (byte)1, (byte)2, (byte)4);
              client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)36, (byte)1, (byte)248, (byte)1, (byte)6, (byte)4);
              break;
            default:
              client.SendMessage("God's name unknown", "pink");
              break;
          }
        }
      }
    }

    /// <summary>
    /// Show in game quest timers
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public void QuestTimer(Client client, string[] args)
    {
      client.LoadRepeatQuests();
    }

    /// <summary>
    /// Show in game boss timers
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public void BossTimer(Client client, string[] args)
    {
      client.LoadBossLog();
    }

    /// <summary>
    /// Toggle all body animations to reduceCPU usage. 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public void ToggleBodyAnimation(Client client, string[] args)
    {
      client.Tab.disableallbody.Checked = !client.Tab.disableallbody.Checked;
    }

    /// <summary>
    /// Toggle Dion on/off. Usefullin Medenia.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public void ToggleDion(Client client, string[] args)
    {
      client.Tab.monitords.Checked = !client.Tab.monitords.Checked;
      client.Tab.monitorcurses.Checked = !client.Tab.monitorcurses.Checked;
      client.Tab.monitordion.Checked = !client.Tab.monitordion.Checked;
    }

    /// <summary>
    /// Toggle wall visibility on/off.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public void ToggleWalls(Client client, string[] args)
    {
      client.Tab.nowalls.Checked = !client.Tab.nowalls.Checked;
    }


    /// <summary>
    /// Shows list of current (de)buff ID's on player.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public void BuffIcons(Client client, string[] args)
    {
      foreach (int spell in client.SpellBar)
        client.SendMessage(spell.ToString());
    }

    /// <summary>
    /// Show info on current map (Name and ID) and location.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public void MapID(Client client, string[] args)
    {
      client.SendMessage($"{client.MapInfo.Name} - {client.MapInfo.Number}, XY: {client.MapInfo.Width}, {client.MapInfo.Height}", "pink");
    }

    /// <summary>
    /// Give Item information in 1st slot of inventory, including name, image number and palette number.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public void ItemID(Client client, string[] args)
    {
      int iconNr = client.Inventory[0].Icon - 32768;
      client.SendMessage($"{client.Inventory[0].Name}, Img#: {iconNr}, Pal#: {client.Inventory[0].IconPal}");
    }

    /// <summary>
    /// Writes you legend marks to a file on your desktop.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public void Legend(Client client, string[] args)
    {
      string filePath = Path.Combine(DesktopPath, $"{client.Name}-marks.txt");
      StreamWriter streamWriter = new StreamWriter(filePath, true);
      foreach (KeyValuePair<string, string> legendMark in client.LegendMarks)
      {
        if (legendMark.Key != null)
          streamWriter.WriteLine(legendMark.Key + " _ " + legendMark.Value);
      }
      streamWriter.Close();
    }

    /// <summary>
    /// Drops one or more specified items from inventory. Supports dropping items for all clients.
    /// </summary>
    /// <param name="client">The client whose inventory will be affected by the drop operation.</param>
    /// <param name="args">"all" for all clients, "chaos" a predefined set of items from chaos, or specific item name (can be abbreviated).
    /// </param>
    public void DropItems(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.SendMessage("Please provide an item to drop. You can also use abbreviations like 'suc' for 'succubus's hair'.", "pink");
        return;
      } 
      else if (args[0].Equals("all", StringComparison.CurrentCultureIgnoreCase))
      {
        // If "all" parameter is provided, call this function without "all" for each client
        args = args.Skip(1).ToArray(); // remove "all" from arguments
        if (args.Length > 0 && args[0].Equals("all", StringComparison.CurrentCulture))
        {
          foreach (var runningClient in Server.Clients)
          {
            DropItems(runningClient, args);
          }
        }
      } 
      else if (args[0].Equals("chaos", StringComparison.CurrentCultureIgnoreCase))
      { // Get rid of your junk from Choas
        // TODO: Test dropping all choas junk
        client.DropItems("White Jade Ring");
        client.DropItems("Drake Tongue");
        client.DropItems("Fire Beetlalic Head");
        client.DropItems("Warrior Beetlalic Head");
        client.DropItems("Cracked Aosdic Helm");
        client.DropItems("Losgann Tail");
        client.DropItems("Ruidhtear Toe");
        client.DropItems("Draco's Jaw");
        client.DropItems("Kabungkl Tail");
        client.DropItems("Chaos Orb");
        client.DropItems("Scale Bracer");
        client.DropItems("Black Stone Ring");
        client.DropItems("Hydraco Horn");
        client.DropItems("Golem Stones");
        client.DropItems("Porboss Claw");
      } 
      else 
      {  
        string itemName = string.Join(" ", args).ToLower();
        if (ItemAbbreviations.TryGetValue(itemName, out string fullItemName))
        {
          itemName = fullItemName;
        }
        client.DropItems(itemName);
      }
    }

    /// <summary>
    /// Withdraws specified items from nearby mundane, allowing for abbreviations and bulk withdrawals.
    /// </summary>
    /// <param name="client">The client requesting the withdrawal, which provides context for the operation and interaction with NPCs.</param>
    /// <param name="args">"all" for maximum number of items to withdraw, frog for all types of frog meat, spore for all types of spores,
    /// or the item to withdraw.</param>
    public void WithdrawItems(Client client, string[] args)
    {
      // Find the first nearby mundane NPC to interact with for withdrawing items.
      uint npcID = findCloseMundane(client);
      if (npcID == 0)
      {
        client.SendMessage("No nearby mundane found to withdraw items from.", "pink");
        return;
      }

      if (args is null || args.Length == 0)
      {
        client.withdrawmode = 1;
        client.DialogueRespond(npcID, "Withdraw");
      }
      else if (client.SafeToWalkFast)
      {
        // make withdrawing spores easy with 1 command.
        if (args[0].Equals("spore", StringComparison.CurrentCultureIgnoreCase))
        {
          client.DialogueRespond(npcID, "Withdraw Red Spore [10]");
          client.PopupClose(npcID);
          Thread.Sleep(1100);
          if (client.SafeToWalkFast) // Check if it's still safe to walk fast after Thread.Sleep
          {
            client.DialogueRespond(npcID, "Withdraw Grey Spore [10]");
            client.PopupClose(npcID);
          }
        }
        // make withdrawing frog meat easy with 1 command.
        else if (args[0].Equals("frog", StringComparison.CurrentCultureIgnoreCase))
        {
          client.DialogueRespond(npcID, "Withdraw Red Frog Meat [5]");
          client.PopupClose(npcID);
          Thread.Sleep(1100);
          if (client.SafeToWalkFast)
          {
            client.DialogueRespond(npcID, "Withdraw Grey Frog Meat [5]");
            client.PopupClose(npcID);
          }
          Thread.Sleep(1100);
          if (client.SafeToWalkFast)
          {
            client.DialogueRespond(npcID, "Withdraw Blue Frog Meat [5]");
            client.PopupClose(npcID);
          }
        }
        // Withdraw as much as possible of the specified item if "all" is used as parameter.
        else if (args.Length > 1 && args[0].Equals("all", StringComparison.CurrentCultureIgnoreCase))
        {
          args = args.Skip(1).ToArray(); // remove "all" from arguments
          string itemName = string.Join(" ", args).ToLower();
          if (ItemAbbreviations.TryGetValue(itemName, out string fullItemName))
          {
            itemName = fullItemName;
          }

          // If item is stackeable, withdraw the maximum stack.
          if (Server.ItemList.ContainsKey(itemName))
          {
            client.DialogueRespond(npcID, $"Withdraw {itemName} [{Server.ItemList[itemName].MaxStack}]");
            client.PopupClose(npcID);
          }
          else // withdraw as many as possible of the specified item if item is not stackable.
          {
            for (int i = 0; i < client.OpenSlotsCount(); i++)
            {
              if (client.SafeToWalkFast)
              {
                client.DialogueRespond(npcID, $"Withdraw {itemName}");
                client.PopupClose(npcID);
                Thread.Sleep(1100);
              }
            }
          }
          client.SendMessage($"Done withdrawing {itemName}");
        }
        else
        {
          string itemName = string.Join(" ", args).ToLower();
          if (ItemAbbreviations.TryGetValue(itemName, out string fullItemName))
          {
            itemName = fullItemName;
          }
          client.DialogueRespond(npcID, $"Withdraw {itemName}");
          client.PopupClose(npcID);
        }
      }
    }

    public void LegalWithdraw(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.SendMessage("Please provide an item to withdraw. You can also use abbreviations like 'suc' for 'succubus's hair'.", "pink");
        return;
      }

      string itemName = string.Join(" ", args).ToLower();
      if (ItemAbbreviations.TryGetValue(itemName, out string fullItemName))
      {
        itemName = fullItemName;
      }
      client.Withdraw(itemName, 1);
    }

    /// <summary>
    /// Withdraws up to max stack of items.
    /// </summary>
    /// <remarks>If the argument matches a known abbreviation, it is expanded to the full item name. The
    /// method processes item names by their prefixes to determine the appropriate category of items to withdraw. If the
    /// item is not recognized as a category, the method attempts to withdraw the maximum allowable stack of the
    /// specified item. The client is notified if no valid item is provided.</remarks>
    /// <param name="client">The client for whom the items will be withdrawn. This client receives messages and item withdrawals as a result
    /// of the operation.</param>
    /// <param name="args">Item name</param>
    public void LegalWithdrawAll(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.SendMessage("Please provide an item to withdraw. You can also use abbreviations like 'suc' for 'succubus's hair'.", "pink");
        return;
      }

      string itemName = string.Join(" ", args).ToLower();
      if (ItemAbbreviations.TryGetValue(itemName, out string fullItemName))
      {
        itemName = fullItemName;
      }

      // Get STR items
      if (itemName.StartsWith("cea"))
      {
        client.Withdraw("ceannlaidir leather gauntlet", 1);
        Thread.Sleep(1100);
        client.Withdraw("grim boots", 1);
        Thread.Sleep(1100);
        client.Withdraw("ceannlaidir leather gauntlet", 1);
        Thread.Sleep(1100);
        client.Withdraw("orc helmet", 1);
        Thread.Sleep(1100);
        client.Withdraw("ceannlaidir leather greaves", 1);
        Thread.Sleep(1100);
        client.Withdraw("cursed belt", 1);
        Thread.Sleep(1100);
        client.Withdraw("ceannlaidir wooden shield", 1);
        Thread.Sleep(1100);
        client.Withdraw("ceannlaidir gold earrings", 1);
      }
      else if (itemName.StartsWith("lua")) // Get INT items
      {
        client.Withdraw("luathas leather gauntlet", 1);
        Thread.Sleep(1100);
        client.Withdraw("luathas leather gauntlet", 1);
        Thread.Sleep(1100);
        client.Withdraw("luathas leather greaves", 1);
        Thread.Sleep(1100);
        client.Withdraw("luathas wooden shield", 1);
        Thread.Sleep(1100);
        client.Withdraw("luathas coral earrings", 1);
        Thread.Sleep(1100);
        client.Withdraw("peace boots", 1);
        Thread.Sleep(1100);
        client.Withdraw("great emerald sword", 1);
      }
      else if (itemName.StartsWith("gli")) // Get WIS items
      {
        client.Withdraw("glioca leather gauntlet", 1);
        Thread.Sleep(1100);
        client.Withdraw("glioca leather gauntlet", 1);
        Thread.Sleep(1100);
        client.Withdraw("glioca leather greaves", 1);
        Thread.Sleep(1100);
        client.Withdraw("glioca wooden shield", 1);
        Thread.Sleep(1100);
        client.Withdraw("glioca coral earrings", 1);
        Thread.Sleep(1100);
        client.Withdraw("glioca boots", 1);
        Thread.Sleep(1100);
        client.Withdraw("hy-brasyl belt", 1);
        Thread.Sleep(1100);
        client.Withdraw("great emerald sword", 1);
      }
      else if (itemName.StartsWith("cai")) // Get CON items
      {
        client.Withdraw("cail leather gauntlet", 1);
        Thread.Sleep(1100);
        client.Withdraw("cail leather gauntlet", 1);
        Thread.Sleep(1100);
        client.Withdraw("cail leather greaves", 1);
        Thread.Sleep(1100);
        client.Withdraw("cail wooden shield", 1);
        Thread.Sleep(1100);
        client.Withdraw("cail gold earrings", 1);
        Thread.Sleep(1100);
        client.Withdraw("cail boots", 1);
        Thread.Sleep(1100);
        client.Withdraw("dwarvish helmet", 1);
        Thread.Sleep(1100);
        client.Withdraw("hy-brasyl belt", 1);
      }
      else if (itemName.StartsWith("fio") && !itemName.StartsWith("fior")) // Get DEX items
      {
        client.Withdraw("fiosachd leather gauntlet", 1);
        Thread.Sleep(1100);
        client.Withdraw("fiosachd leather gauntlet", 1);
        Thread.Sleep(1100);
        client.Withdraw("fiosachd leather greaves", 1);
        Thread.Sleep(1100);
        client.Withdraw("fiosachd wooden shield", 1);
        Thread.Sleep(1100);
        client.Withdraw("fiosachd gold earrings", 1);
        Thread.Sleep(1100);
        client.Withdraw("silk boots", 1);
        Thread.Sleep(1100);
        client.Withdraw("cursed belt", 1);
      }
      else
      {
        int currentAmount = (int)client.ItemCount(itemName);
        int maxStack = Server.ItemList.ContainsKey(itemName) ? Server.ItemList[itemName].MaxStack : 30;
        int withdrawAmount = maxStack - currentAmount;
        client.Withdraw(itemName, withdrawAmount);
      }
    }


    /// <summary>
    /// Deposits specified items via nearby mundane.
    /// </summary>
    /// <remarks>If no nearby mundane NPC is found, the operation is aborted and a message is sent to the
    /// client. The method checks whether the client is in a safe state to perform the deposit operation before
    /// proceeding. Item abbreviations are resolved automatically if present.</remarks>
    /// <param name="client">The client instance representing the player performing the deposit operation. Must not be null.</param>
    /// <param name="args">An array of strings specifying the items to deposit. If the first element is 'all', all matching items in the
    /// inventory are deposited. If null or empty, initiates deposit mode for manual selection.</param>
    public void DepositItems(Client client, string[] args)
    {
      // Find the first nearby mundane NPC to interact with for withdrawing items.
      uint npcID = findCloseMundane(client);
      if (npcID == 0)
      {
        client.SendMessage("No nearby mundane found to deposit items to.", "pink");
        return;
      }

      if (args is null || args.Length == 0)
      {
        client.depositmode = 1;
        client.DialogueRespond(npcID, "Deposit");
      }
      else if (client.SafeToWalkFast)
      {
        // Deposit as much as possible of the specified item if "all" is used as parameter.
        if (args.Length > 1 && args[0].Equals("all", StringComparison.CurrentCultureIgnoreCase))
        {
          args = args.Skip(1).ToArray(); // remove "all" from arguments
          string itemName = string.Join(" ", args).ToLower();
          if (ItemAbbreviations.TryGetValue(itemName, out string fullItemName))
          {
            itemName = fullItemName;
          }

          for (int i = 0; i < client.Inventory.Length; i++)
          {
            Item item = client.Inventory[i];
            if (client.SafeToWalkFast)
            {
              if (item != null && item.Name.Equals(itemName, StringComparison.CurrentCultureIgnoreCase))
              {
                if (item.Amount > 1)
                {
                  client.DialogueRespond(npcID, $"Deposit {item.InventorySlot} [{item.Amount}]");
                  client.PopupClose(npcID);
                }
                else
                {
                  client.DialogueRespond(npcID, $"Deposit {item.InventorySlot}");
                  client.PopupClose(npcID);
                }
                Thread.Sleep(1100);
              }
            }
            else
            {
              client.SendMessage("Stopped depositing items to prevent illegal speeding issues.", "pink");
              break;
            }
          }
        }
        else if(args.Length > 0)
        {
          string itemName = string.Join(" ", args).ToLower();
          if (ItemAbbreviations.TryGetValue(itemName, out string fullItemName))
          {
            itemName = fullItemName;
          }

          foreach(Item item in  client.Inventory) 
          {
            if (item != null && item.Name.Equals(itemName, StringComparison.CurrentCultureIgnoreCase))
            {
              client.DialogueRespond(npcID, $"Deposit {item.InventorySlot}");
              client.PopupClose(npcID);
              break;
            }
          }

        }
      }
    }

    /// <summary>
    /// Depositing a single item.
    /// </summary>
    /// <remarks>If no item is specified in the arguments, a message is sent to the client requesting an item.
    /// Abbreviations for item names are supported and will be expanded to their full form if recognized.</remarks>
    /// <param name="client">The client making the deposit request. This parameter is used to send messages and perform the deposit
    /// operation.</param>
    /// <param name="args">The item to be deposited. The first element should specify the item name, which
    /// may include abbreviations.</param>
    public void LegalDeposit(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.SendMessage("Please provide an item to deposit. You can also use abbreviations like 'suc' for 'succubus's hair'.", "pink");
        return;
      }
      string itemName = string.Join(" ", args).ToLower();
      if (ItemAbbreviations.TryGetValue(itemName, out string fullItemName))
      {
        itemName = fullItemName;
      }
      client.Deposit(itemName, 1);
    }

    /// <summary>
    /// Deposits items, supporting both full item names and recognized abbreviations.
    /// </summary>
    /// <remarks>If the first argument matches a known prefix ('cea', 'lua', 'gli', 'cai', or 'fio'),
    /// all stat-items related to that god are deposited. Otherwise, the method attempts to resolve the argument as
    /// an item name or abbreviation and deposits all matching items. Abbreviations are supported for
    /// convenience.</remarks>
    /// <param name="client">The client instance whose inventory will be processed for item deposits.</param>
    /// <param name="args">Item (abbreviation), god (abbreviation)</param>
    public void LegalDepositAll(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.SendMessage("Please provide an item to deposit. You can also use abbreviations like 'suc' for 'succubus's hair'.", "pink");
        return;
      }

      // Deposit STR items
      if (args.Length == 1 && args[0].StartsWith("cea", StringComparison.CurrentCultureIgnoreCase))
      {
        foreach (Item item in client.Inventory)
        {
          if (item != null && !item.Name.Contains(" Prayer Necklace") 
                           && (item.Name.StartsWith("Ceannlaidir") 
                             || item.Name.Equals("Cursed Belt") 
                             || item.Name.Equals("Grim Boots") 
                             || item.Name.Equals("Orc Helmet")))
            client.Deposit(item.Name, 1);
        }
      }
      // Deposit INT items
      else if (args.Length == 1 && args[0].StartsWith("lua", StringComparison.CurrentCultureIgnoreCase))
      {
        foreach (Item item in client.Inventory)
        {
          if (item != null && !item.Name.Contains(" Prayer Necklace") 
                           && (item.Name.StartsWith("Luathas") 
                             || item.Name.Equals("Great Emerald Sword") 
                             || item.Name.Equals("Peace Boots")))
            client.Deposit(item.Name, 1);
        }
      }
      // Deposit WIS items
      else if (args.Length == 1 && args[0].StartsWith("gli", StringComparison.CurrentCultureIgnoreCase))
      {
        foreach (Item item in client.Inventory)
        {
          if (item != null && !item.Name.Contains(" Prayer Necklace")
                            && (item.Name.StartsWith("Glioca")
                              || item.Name.Equals("Hy-brasyl Belt")
                              || item.Name.Equals("Great Emerald Sword")))
            client.Deposit(item.Name, 1);
        }
      }
      // Deposit CON items
      else if (args.Length == 1 && args[0].StartsWith("cai", StringComparison.CurrentCultureIgnoreCase))
      {
        foreach (Item item in client.Inventory)
        {
          if (item != null && !item.Name.Contains(" Prayer Necklace") 
                          && (item.Name.StartsWith("Cail") 
                            || item.Name.Equals("Hy-brasyl Belt") 
                            || item.Name.Equals("Dwarvish Helmet")))
          client.Deposit(item.Name, 1);
        }
      }
      // Deposit DEX items
      else if (args.Length == 1 && args[0].StartsWith("fio", StringComparison.CurrentCultureIgnoreCase) && !args[0].StartsWith("fior", StringComparison.CurrentCultureIgnoreCase))
      {
        foreach (Item item in client.Inventory)
        {
          if (item != null && !item.Name.Contains(" Prayer Necklace")
                           && (item.Name.StartsWith("Fiosachd")
                             || item.Name.Equals("Cursed Belt")
                             || item.Name.Equals("Silk Boots")))
          client.Deposit(item.Name, 1);
        }
      }
      else
      {
        string itemName = string.Join(" ", args).ToLower();
        if (ItemAbbreviations.TryGetValue(itemName, out string fullItemName))
        {
          itemName = fullItemName;
        }
        foreach (Item item in client.Inventory)
        {
          if (item != null && item.Name.Equals(itemName, StringComparison.CurrentCultureIgnoreCase))
          {
            client.Deposit(itemName, (int)item.Amount);
          }
        }
      }
    }


    /// <summary>
    /// Send items to a recipient through a nearby mundane NPC.
    /// </summary>
    /// <remarks>If no nearby mundane NPC is found, a message is sent to the client indicating the failure.
    /// The method requires the user to start the dialogue with the NPC using the '/send' command. If the arguments are
    /// invalid, appropriate messages are displayed to guide the user.</remarks>
    /// <param name="client">The client instance representing the user initiating the item sending process.</param>
    /// <param name="args">Can be null, empty or aisling with item to send</param>
    public void SendItems(Client client, string[] args)
    {

      // Find the first nearby mundane NPC to interact with for withdrawing items.
      uint npcID = findCloseMundane(client);
      if (npcID == 0)
      {
        client.SendMessage("No nearby mundane found to send a parcel.", "pink");
        return;
      }

      // Only /send starts parcel sending dialogue.
      if (args is null || args.Length == 0)
      {
        client.DialogueRespond(npcID, "Send Parcel");
        client.sendmode = 1;
      }
      else if (args.Length == 1)
      {
        client.SendMessage("Please use '/send' to start parcel sending or '/send aisling item'.", "pink");
        return;
      }
      else if (client.SafeToWalkFast && args.Length > 1)
      {
        string receiverName = args[0];
        args = args.Skip(1).ToArray(); // remove receiver name from arguments

        string itemName = string.Join(" ", args).ToLower();
        if (ItemAbbreviations.TryGetValue(itemName, out string fullItemName))
        {
          itemName = fullItemName;
        }

        client.DialogueRespond(npcID, $"Send Parcel {receiverName} {itemName}");
        client.PopupClose(npcID);
      }
    }

    /// <summary>
    /// Receives one or more parcels from a nearby mundane.
    /// </summary>
    /// <remarks>If no nearby mundane NPC is found, a message is sent to the client and no parcels are
    /// received. When requesting all parcels, the method continues to receive parcels until none remain. This method
    /// may cause a delay when receiving multiple parcels due to repeated interactions.</remarks>
    /// <param name="client">The client requesting to receive parcels. Must not be null.</param>
    /// <param name="args">Can be null or "all" for all parcels</param>
    public void ReceiveItems(Client client, string[] args)
    {
      // Find the first nearby mundane NPC to interact with for receiving parcels.
      uint npcID = findCloseMundane(client);
      if (npcID == 0)
      {
        client.SendMessage("No nearby mundane found to receive parcels from.", "pink");
        return;
      }

      if (args is null || args.Length == 0)
      {
        client.DialogueRespond(npcID, "Receive Parcel");
        client.PopupClose(npcID);
      }
      else if (args.Length == 1 && args[0].Equals("all", StringComparison.CurrentCultureIgnoreCase))
      {
        client.hasparcels = true;
        while (client.hasparcels)
        {
          client.DialogueRespond(npcID, "Receive Parcel");
          client.PopupClose(npcID);
          Thread.Sleep(1100);
        }
      }
    }

    /// <summary>
    /// Repair all with the nearest mundane NPC.
    /// </summary>
    /// <remarks>If no nearby mundane NPC is found, a message is sent to the client indicating the failure to
    /// locate a repair option. The method sets the client's repair mode to true and records the amount of gold the
    /// client had before the repair process.</remarks>
    /// <param name="client">The client instance representing the player requesting the repair.</param>
    /// <param name="args"></param>
    public void RepairAll(Client client, string[] args)
    {
      // Find the first nearby mundane NPC to interact with for repairing items.
      uint npcID = findCloseMundane(client);
      if (npcID == 0)
      {
        client.SendMessage("No nearby mundane found to repair items.", "pink");
        return;
      }

      client.repairmode = true;
      client.goldbefore = client.Statistics.Gold;
      client.DialogueRespond(npcID, "Fix All");
    }

    /// <summary>
    /// Buy items from a nearby mundane.
    /// </summary>
    /// <remarks>If no item is specified, a message is sent to the client prompting for an item. If no nearby
    /// mundane NPC is found, an error message is sent. Item abbreviations are supported for convenience.</remarks>
    /// <param name="client">The client requesting the purchase, which is used to send messages and execute the buy operation.</param>
    /// <param name="args">Can be item name (abbreviated), or item name + amount to buy</param>
    public void BuyItems(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.SendMessage("Please provide an item to buy. You can also use abbreviations like 'suc' for 'succubus's hair'.", "pink");
        return;
      }

      // Find the first nearby mundane NPC to interact with for buying items.
      uint npcID = findCloseMundane(client);
      if (npcID == 0)
      {
        client.SendMessage("No nearby mundane found to buy items from.", "pink");
        return;
      }

      string itemName = string.Join(" ", args).ToLower();
      if (ItemAbbreviations.TryGetValue(itemName, out string fullItemName))
      {
        itemName = fullItemName;
      }

      // Buy 1 by default
      uint stackSize = 1;
      // or use provided stack size
      if (args.Length == 2)
      {
        uint.TryParse(args[1], out stackSize);
      }
      // or max stack size
      else if (Server.ItemList.ContainsKey(itemName))
      {
        stackSize = (uint)Server.ItemList[itemName].MaxStack;
      }

      client.BuyItem(itemName, stackSize);
    }

    /// <summary>
    /// Displays the current gold.
    /// </summary>
    /// <remarks>This method prompts the client with a message and then sends a formatted message indicating
    /// the client's current gold amount.</remarks>
    /// <param name="client">The client whose gold balance is to be displayed. Cannot be null.</param>
    /// <param name="args"></param>
    public void ShowMoney(Client client, string[] args)
    {
      client.SkillSpellCaption("How many Coinss have I?");

      client.SendMessage($"You have {client.Statistics.Gold:N} gold.", "grey");
    }

    /// <summary>
    /// Toggles the hide.
    /// </summary>
    /// <remarks>Must have either the 'Hide' or 'White Bat Stance' spell to change their visibility
    /// state. When using the 'all' argument, it goes through all open clients and also checks for Vanishing Elixir.</remarks>
    /// <param name="client">The client instance whose visibility state is to be modified. The client must possess the required spell to
    /// change visibility.</param>
    /// <param name="args">Can be null or "all"</param>
    public void Hide(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        if (client.HasSpell("Hide") || client.HasSpell("White Bat Stance"))
          client.Tab.selfhide.Checked = !client.Tab.selfhide.Checked;
      }
      else
      {
        if (args[0].Equals("all", StringComparison.CurrentCultureIgnoreCase))
        {
          foreach(Client runningClient in Server.Clients)
          {
            Hide(runningClient, new string[] { "checkAll" });
          }
        }
        else if (args[0].Equals("checkAll", StringComparison.CurrentCultureIgnoreCase))
        {
          if (client.HasSpell("Hide") || client.HasSpell("White Bat Stance"))
          {
            if (client.Tab.selfhide.Checked == false)
            {
              // check for vanishing elixer.
              if (client.HasItem("Vanishing Elixir"))
              {
                client.UseItem("Vanishing Elixir");
              }
            }
            client.Tab.selfhide.Checked = !client.Tab.selfhide.Checked;
          }
        }
      }
    }

    /// <summary>
    /// Give labor to specified player.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args">Player</param>
    public void GiveLabor(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.SendMessage("Please provide a player to give labor to.", "pink");
        return;
      }

      client.Tab.laborname.Text = args[0];
      client.Tab.laborbutton.Text = "Stop";
      client.Tab.autowalker_locales.Text = "Nearest Bank";
      client.Tab.walksettings.Value = 250M;
      client.Tab.autowalker_button.Text = "Stop";
      client.autowalkon = true;
      client.Tab.btnPlay.Enabled = false;
      client.Tab.btnStop.Enabled = true;
      client.pause = false;
    }

    /// <summary>
    /// Groups the specified players, alts, or friends 
    /// </summary>
    /// <param name="client">The client that initiates the grouping operation.</param>
    /// <param name="args">The first element can be 'alts', 'friends' or a specific player name.
    /// </param>
    public void Group(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.SendMessage("Please provide player/alts/friends to group with.", "pink");
        return;
      }

      byte groupNice = 2;
      if (args[0].Equals("alts", StringComparison.CurrentCulture))
      {
        foreach (Client runningClient in Server.Clients)
        {
          client.ForceGroup(runningClient.Name, groupNice);
        }
      }
      else if (args[0].Equals("friends", StringComparison.CurrentCulture) && Server.friendlist != null)
      {
        Player[] playerArray = client.AnyPlayer();
        foreach (Player player in client.AnyPlayer())
        {
          if (player != null && player.ID != client.PlayerID 
                             && player.IsOnScreen 
                             && !client.GroupMembers.Contains(player.Name, StringComparer.CurrentCultureIgnoreCase) 
                             && Server.friendlist.Contains(player.Name, StringComparer.CurrentCultureIgnoreCase))
          {
            client.ForceGroup(player.Name, groupNice);
          }
        }
      }
      else
      {
        client.ForceGroup(args[0], groupNice);  
      }
    }

    /// <summary>
    /// Forcegroups the specified players, alts, or friends 
    /// </summary>
    /// <param name="client">The client that initiates the grouping operation.</param>
    /// <param name="args">The first element can be 'alts', 'friends' or a specific player name.
    /// </param>

    public void ForceGroup(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.SendMessage("Please provide player/alts to forcegroup with.", "pink");
        return;
      }

      byte forceGroup = 3;
      if (args[0].Equals("alts", StringComparison.CurrentCulture))
      {
        foreach (Client runningClient in Server.Clients)
        {
          ForceGroup(runningClient, new string[] { client.Name });
        }
      }
      else
      {
        client.ForceGroup(args[0], forceGroup);
      }
    }

    /// <summary>
    /// Cast fiosachd prayer dia bean
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public void CastDiabean(Client client, string[] args)
    {
      if (client.cancast && client.HasItem("Fiosachd Prayer Necklace") && client.HasSpell("Fiosachd Prayer"))
      {
        client.MacroCast("Fiosachd Prayer", new uint?());
        Thread.Sleep(1100);
        client.Drop(client.ServerLocation.X, client.ServerLocation.Y, client.ItemSlot("Fiosachd Prayer Necklace"), 1);
        Thread.Sleep(500);
        client.Pickup(client.ServerLocation.X, client.ServerLocation.Y);
        Thread.Sleep(500);
        uint currentnpcpopupId = client.CurrentnpcpopupID;
        client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)35, (byte)0, (byte)92, (byte)1, (byte)2, (byte)4);
        client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)35, (byte)0, (byte)248, (byte)1, (byte)2, (byte)4);
        client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)35, (byte)2, (byte)94, (byte)1, (byte)3, (byte)4);
      }
    }

    /// <summary>
    /// Cast Fiosachd group hide
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public void CastFioHide(Client client, string[] args)
    {
      if (client.cancast && client.HasItem("Fiosachd Prayer Necklace") && client.HasSpell("Fiosachd Prayer"))
      {
        client.MacroCast("Fiosachd Prayer", new uint?());
        Thread.Sleep(1100);
        client.Drop(client.ServerLocation.X, client.ServerLocation.Y, client.ItemSlot("Fiosachd Prayer Necklace"), 1);
        Thread.Sleep(500);
        client.Pickup(client.ServerLocation.X, client.ServerLocation.Y);
        Thread.Sleep(500);
        uint currentnpcpopupId = client.CurrentnpcpopupID;
        client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)35, (byte)0, (byte)92, (byte)1, (byte)2, (byte)4);
        client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)35, (byte)0, (byte)248, (byte)1, (byte)2, (byte)4);
        client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)35, (byte)2, (byte)94, (byte)1, (byte)3, (byte)4);
      }
    }

    /// <summary>
    /// Cast Gramail prayer reflect
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public void CastGramReflect(Client client, string[] args)
    {
      if (client.cancast && client.HasItem("Gramail Prayer Necklace") && client.HasSpell("Gramail Prayer"))
      {
        client.MacroCast("Gramail Prayer", new uint?());
        Thread.Sleep(1100);
        client.Drop(client.ServerLocation.X, client.ServerLocation.Y, client.ItemSlot("Gramail Prayer Necklace"), 1);
        Thread.Sleep(500);
        client.Pickup(client.ServerLocation.X, client.ServerLocation.Y);
        Thread.Sleep(500);
        uint currentnpcpopupId = client.CurrentnpcpopupID;
        client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)34, (byte)0, (byte)92, (byte)1, (byte)2, (byte)4);
        client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)34, (byte)0, (byte)248, (byte)1, (byte)2, (byte)4);
        client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)34, (byte)2, (byte)109, (byte)1, (byte)2, (byte)4);
      }
    }

    /// <summary>
    /// Cast Gramail prayer Ao Sith (remove all spells)
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public void CastGramAoSith(Client client, string[] args)
    {
      if (client.cancast && client.HasItem("Gramail Prayer Necklace") && client.HasSpell("Gramail Prayer"))
      {
        client.MacroCast("Gramail Prayer", new uint?());
        Thread.Sleep(1100);
        client.Drop(client.ServerLocation.X, client.ServerLocation.Y, client.ItemSlot("Gramail Prayer Necklace"), 1);
        Thread.Sleep(500);
        client.Pickup(client.ServerLocation.X, client.ServerLocation.Y);
        Thread.Sleep(500);
        uint currentnpcpopupId = client.CurrentnpcpopupID;
        client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)34, (byte)0, (byte)92, (byte)1, (byte)2, (byte)4);
        client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)34, (byte)0, (byte)248, (byte)1, (byte)2, (byte)4);
        client.PopupRespond(new uint?(currentnpcpopupId), (byte)2, (byte)34, (byte)2, (byte)108, (byte)1, (byte)1, (byte)4);
      }
    }

    public void Higgle(Client client, string[] args)
    {
      client.SendMessage("Pause program to stop this task.");
      client.Tab.btnPlay.Enabled = false;
      client.Tab.btnStop.Enabled = true;
      client.pause = false;

      int batchesOfWine = 0;
      while (!client.outoflabor)
      {
        if (!client.HasItem("Wine") || client.ItemAmount("Wine") < 15U)
        {
          foreach (Npc npc in client.NearbyNpcs(Npc.NpcType.Mundane))
          {
            if (npc != null)
            {
              client.DialogueRespond(new uint?(npc.ID), "Higgle");
              client.PopupRespond(new uint?(npc.ID), (byte)1, (byte)125, (byte)0, (byte)19, (byte)1, (byte)2);
              client.PopupRespond(new uint?(npc.ID), (byte)1, (byte)125, (byte)0, (byte)26, (byte)1, (byte)2);
              client.PopupRespond(new uint?(npc.ID), (byte)1, (byte)125, (byte)0, (byte)45, (byte)1, (byte)2);
              client.PopupRespond(new uint?(npc.ID), (byte)1, (byte)125, (byte)0, (byte)102, (byte)1, (byte)4);
              Thread.Sleep(1000);
              break;
            }
          }
        }
        else if (client.ItemAmount("Wine") == 15U)
        {
          client.DropItems("Wine");
          batchesOfWine++;
          Thread.Sleep(1000);
        }
        Thread.Sleep(200);
        if (batchesOfWine >= 12)
        {
          client.SendMessage("done higgling 180 wine");
        }
        else if (client.pause)
          break;
      }
      client.SendMessage("Out of labor.");
      client.Tab.btnPlay.Enabled = true;
      client.Tab.btnStop.Enabled = false;
      client.pause = true;
    }


    /// <summary>
    /// Walk to ToC Warrior and become warrior.
    /// </summary>
    /// <param name="client"></param>
    public void ToCWarrior(Client client, string[] args)
    {
      client.warrior = true;
      client.Tab.autowalker_locales.SelectedItem = "Mileth";
      client.Tab.walklocaleslist.SelectedItem = "ToC Warrior";
      InitWalking(client);
    }

    /// <summary>
    /// Walk to ToC Monk and become monk.
    /// </summary>
    /// <param name="client"></param>

    public void ToCMonk(Client client, string[] args)
    {
      client.monk = true;
      client.Tab.autowalker_locales.SelectedItem = "Mileth";
      client.Tab.walklocaleslist.SelectedItem = "ToC Monk";
      InitWalking(client);
    }

    /// <summary>
    /// Walk to ToC Rogue and become rogue.
    /// </summary>
    /// <param name="client"></param>
    public void ToCRogue(Client client, string[] args)
    {
      client.rogue = true;
      client.Tab.autowalker_locales.SelectedItem = "Mileth";
      client.Tab.walklocaleslist.SelectedItem = "ToC Rogue";
      InitWalking(client);
    }

    /// <summary>
    /// Walk to ToC Priest and become priest.
    /// </summary>
    /// <param name="client"></param>
    public void ToCPriest(Client client, string[] args)
    {
      client.priest = true;
      client.Tab.autowalker_locales.SelectedItem = "Mileth";
      client.Tab.walklocaleslist.SelectedItem = "ToC Priest";
      InitWalking(client);
    }

    /// <summary>
    /// Walk to ToC Wizard and become wizard.
    /// </summary>
    /// <param name="client"></param>
    public void ToCWizard(Client client, string[] args)
    {
      client.wizard = true;
      client.Tab.autowalker_locales.SelectedItem = "Mileth";
      client.Tab.walklocaleslist.SelectedItem = "ToC Wizard";
      InitWalking(client);
    }

    /// <summary>
    /// Ascend HP or MP.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args">Either "hp" or "mp"</param>
    public void Ascend(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.SendMessage("Please provide an argument: either \"hp\" or \"mp\".", "pink");
        return;
      }

      if (args[0].Equals("hp", StringComparison.CurrentCultureIgnoreCase))
      {
        AscendHP(client, null);
        return;
      }
      if (args.Length > 0 && args[0].Equals("mp", StringComparison.CurrentCultureIgnoreCase))
      {
        AscendMP(client, null);
        return;
      }
      client.SendMessage("Provide either \"mp\" or \"hp\" as an argument.", "grey");
    }

    /// <summary>
    /// Ascend and put all XP into HP.
    /// </summary>
    /// <param name="client"></param>
    public void AscendHP(Client client, string[] args)
    {
      client.Tab.AscendOptions.instantascend.Checked = true;
      client.Tab.AscendOptions.ascendhp.Checked = true;
      client.Tab.AscendOptions.ascendbutton.Text = "Stop";
    }

    /// <summary>
    /// Ascend and put all XP into MP.
    /// </summary>
    /// <param name="client"></param>
    public void AscendMP(Client client, string[] args)
    {
      client.Tab.AscendOptions.instantascend.Checked = true;
      client.Tab.AscendOptions.ascendmp.Checked = true;
      client.Tab.AscendOptions.ascendbutton.Text = "Stop";
    }

    /// <summary>
    /// Activates the dojo tab client.
    /// </summary>
    /// <param name="client">The client for which to activate the dojo tab. Cannot be null.</param>
    public void Dojo(Client client, string[] args)
    {
      if (client.Tab.dojo.Checked)
        client.Tab.dojo.Checked = false;
      client.Tab.dojo.Checked = true;
    }

    /// <summary>
    /// Initieates teaching the dugon at Sapphire Stream and walking to right stone
    /// </summary>
    /// <remarks>The correct dugon is known.</remarks>
    /// <param name="client"></param>
    public void TeachDugon(Client client, string[] args)
    {
      client.RequestGroupList();
      client.SendMessage("wait a sec...");
      Thread.Sleep(1000);
      client.Speak("sabonim, please teach me the " + client.currentdugon + " dugon");
    }

    /// <summary>
    /// Complete teaching the dugon at Sapphire Stream.
    /// </summary>
    /// <remarks>The correct dugon is known.</remarks>
    /// <param name="client"></param>
    public void DoneDugon(Client client, string[] args)
    {
      client.Speak("sabonim, i understand the " + client.currentdugon + " dugon");
    }

    /// <summary>
    /// Remove all monsters from spell configuration of all clients.
    /// </summary>
    /// <param name="client"></param>
    public void RemoveMonsters(Client client, string[] args)
    {
      Client[] runningClients = Server.Alts.Values.ToArray<Client>();
      foreach (Client runningClient in Server.Clients)
      {
        if (runningClient.Tab.allMonsters != null)
        {
          --runningClient.Tab.spellMonsters.SelectedIndex;
          runningClient.Tab.spellMonsters.TabPages.Remove(runningClient.Tab.allMonsters);
          runningClient.Tab.allMonsters = null;
          runningClient.Tab.newmonster.Enabled = true;
          runningClient.Tab.newallmonsters.Enabled = true;
          runningClient.Tab.newmonsterbyplayer.Enabled = true;
          runningClient.Tab.createnewmonster.Enabled = true;
        }
      }
      client.SendMessage("Removed All Monsters Tabs from all clients", "grey");
    }

    /// <summary>
    /// Initiates the Dark Maze quest, if the required beothaich deum is in the inventory.
    /// </summary>
    /// <param name="client">The client instance for which to initiate the Dark Maze entry. Cannot be null.</param>
    public void DarkMaze(Client client, string[] args)
    {
      if (!client.HasItem("beothaich deum") && !client.HasItem("Red Potion"))
      {
        client.SendMessage("Get some beothaich deum");
      }
      else
      {
        client.darkmaze = true;
        client.Tab.autowalker_locales.SelectedItem = "Loures";
        client.Tab.walklocaleslist.SelectedItem = "Dungeon (aite)";
        InitWalking(client);
      }
    }

    /// <summary>
    /// Initiates the Lost Ruins Law quest.
    /// </summary>
    /// <remarks>Client must each have 5 giant remains.</remarks>
    /// <param name="client"></param>
    public void Law(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.Tab.LoadTemplate("default");
        client.lawquest = true;
        client.Tab.fastwalk.Checked = true;
        client.Tab.autowalker_locales.Text = "Lost Ruins";
        client.Tab.walklocaleslist.SelectedItem = "Nairn";
        InitWalking(client);

        client.LastnpcpopupID = 0U;
      }
      else
      {
        if (args[0].Equals("all", StringComparison.CurrentCultureIgnoreCase))
        {
          foreach(Client runningClient in Server.Clients)
          {
            Law(runningClient, null);
          }
        }
      }
    }

    /// <summary>
    /// Initiates the Mother's Love quest.
    /// </summary>
    /// <param name="client"></param>
    public void MothersLove(Client client, string[] args)
    {
      client.molo = true;
      client.Tab.autowalker_locales.SelectedItem = "Nearest Restaurant";
      InitWalking(client);

    }

    /// <summary>
    /// Initiates the Hwarone/Lost Ruins stone slab quest.
    /// </summary>
    /// <param name="client">The client instance for which to start the stone slab quest. Cannot be null.</param>
    public void StoneSlabs(Client client, string[] args)
    {
      client.slabquest = true;
      client.Tab.autowalker_locales.SelectedItem = "Hwarone";
      client.Tab.walklocaleslist.SelectedItem = "Inn";
      InitWalking(client);
    }

    /// <summary>
    /// Complete the Water Dungeon quest.
    /// </summary>
    /// <param name="client">The client instance that receives the command execution response.</param>
    /// <param name="args">"all" or null.</param>
    public void WaterDungeon(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.Speak("Water Spirit, I have done what you have asked of me.", 1);
      }
      else if (args[0].Equals("all", StringComparison.CurrentCultureIgnoreCase))
      {
        foreach (Client runningClient in Server.Clients)
        {
          WaterDungeon(runningClient, new string[] { null });
        }
      }
    }


    /// <summary>
    /// Initiates the letter quest, if client is grouped with another player.
    /// </summary>
    /// <param name="client"></param>
    public void TheLetter(Client client, string[] args)
    {
      if (client.GroupMembers.Count<string>() == 1)
      {
        foreach (Client runningClient in Server.Clients)
        {
          if (runningClient.Name.ToLower() == client.GroupMembers[0].ToLower())
          {
            runningClient.letterquest = 1;
            runningClient.theletter = true;
            if (client.MapInfo.Number != 168 && client.MapInfo.Number != 393 && client.MapInfo.Number != 134 && client.MapInfo.Number != 136 && client.MapInfo.Number != 115 && client.MapInfo.Number != 118 && client.MapInfo.Number != 122 && client.MapInfo.Number != 303 && client.MapInfo.Number != 3041)
            {
              runningClient.Tab.autowalker_locales.SelectedItem = "Mileth";
              runningClient.Tab.walklocaleslist.SelectedItem = "Restaurant";
              InitWalking(runningClient);
            }
          }
        }
      }

      client.letterquest = 1;
      client.theletter = true;
      if (client.MapInfo.Number != 168 && client.MapInfo.Number != 393 && client.MapInfo.Number != 134 && client.MapInfo.Number != 136 && client.MapInfo.Number != 115 && client.MapInfo.Number != 118 && client.MapInfo.Number != 122 && client.MapInfo.Number != 303 && client.MapInfo.Number != 3041)
      {
        client.Tab.autowalker_locales.SelectedItem = "Mileth";
        client.Tab.walklocaleslist.SelectedItem = "Restaurant";
        InitWalking(client);
      }
    }

    /// <summary>
    /// Initiates the Dragon Scale Sword quest.
    /// </summary>
    /// <remarks>Needs to have a tentacle.</remarks>
    /// <param name="client"></param>
    public void DragonScaleSword(Client client, string[] args)
    {
      client.Tab.autowalker_locales.SelectedItem = "Mileth";
      client.Tab.walklocaleslist.SelectedItem = "Tavern";
      InitWalking(client);
    }

    /// <summary>
    /// Initiates getting the giant pearl for the Medusa creant quest.
    /// </summary>
    /// <param name="client"></param>
    public void GiantPearl(Client client, string[] args)
    {
      client.giantpearl = true;
      client.Tab.looton.Checked = true;
      client.Tab.autowalker_locales.SelectedItem = "Lynith";
      client.Tab.walklocaleslist.SelectedItem = "Giant Pearl";
      InitWalking(client);

    }

    /// <summary>
    /// Initiates buying the 2nd half talisman for the Medusa creant quest.
    /// </summary>
    /// <param name="client"></param>
    public void HalfTalisman(Client client, string[] args)
    {
      client.buy2ndtalisman = true;
      //if (!client.BotThread.IsAlive)
      //  client.BotThread.Start();
      client.pause = false;
      client.Tab.btnPlay.Enabled = false;
      client.Tab.btnStop.Enabled = true;
    }

    /// <summary>
    /// Get sunprotection in August event.
    /// </summary>
    /// <param name="client"></param>
    public void SunProtection(Client client, string[] args)
    {
      client.claimsunprotection = true;
      client.Tab.autowalker_locales.SelectedItem = "Loures";
      client.Tab.walklocaleslist.SelectedItem = "Francis (summer)";
      InitWalking(client);
    }

    /// <summary>
    /// Get paradise outfit in August event.
    /// </summary>
    /// <param name="client"></param>
    public void BeachAttire(Client client, string[] args)
    {
      client.claimbeachattire = true;
      if (client.HasSkill("swimming"))
      {
        client.Tab.autowalker_locales.SelectedItem = "Lynith";
        client.Tab.walklocaleslist.SelectedItem = "Paradise";
      }
      else
      {
        client.learnswim = true;
        client.SendMessage("Learning to swim at mileth inn first");
        client.Tab.autowalker_locales.SelectedItem = "Mileth";
        client.Tab.walklocaleslist.SelectedItem = "Inn";
      }
      InitWalking(client);
    }

    /// <summary>
    /// Make a wish in February event.
    /// </summary>
    /// <param name="client"></param>
    public void MakeAWish(Client client, string[] args)
    {
      client.makeawish = true;
      client.Tab.autowalker_locales.SelectedItem = "Mileth";
      client.Tab.walklocaleslist.SelectedItem = "Church";
      InitWalking(client);
    }


    /// <summary>
    /// Attack specific targets.
    /// </summary>
    /// <remarks>Implemented: Count, Countess
    /// </remarks>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public void Attack(Client client, string[] args)
    {
      if (args is null || args.Length == 0)
      {
        client.SendMessage("Should eiter attack Count or Countess", "pink");
        return;
      }

      if (args[0].Equals("count", StringComparison.CurrentCultureIgnoreCase))
      {
        AttackCount(client, null);
        return;
      }
      if (args[0].Equals("countess", StringComparison.CurrentCultureIgnoreCase))
      {
        AttackCountess(client, null);
        return;
      }
    }

    public void AttackCount(Client client, string[] args)
    {
      if (Program.MainForm.attackcount)
      {
        foreach (Client runningClient in Server.Clients)
        { 
          if (runningClient.Tab.Monsters != null)
          {
            foreach (targetMonster targetMonster in runningClient.targetmonster)
            {
              if (targetMonster != null && targetMonster.Text.Equals("814"))
              {
                if (AttackBossSpells.Contains(targetMonster.attack1type.Text))
                  targetMonster.attack1.Checked = false;
                if (AttackBossSpells.Contains(targetMonster.attack2type.Text))
                  targetMonster.attack2.Checked = false;
              }
            }
          }
        }
        Program.MainForm.attackcount = false;
        client.SendMessage("No longer attacking Count.", "pink");
      }
      else
      {
        foreach (Client runningClient in Server.Clients)
        {
          if (runningClient.Tab.Monsters != null)
          {
            foreach (targetMonster targetMonster in runningClient.targetmonster)
            {
              if (targetMonster != null && targetMonster.Text.Equals("814"))
              {
                if (AttackBossSpells.Contains(targetMonster.attack1type.Text))
                  targetMonster.attack1.Checked = true;
                if (AttackBossSpells.Contains(targetMonster.attack2type.Text))
                  targetMonster.attack2.Checked = true;
              }
            }
          }
        }
        Program.MainForm.attackcount = true;
        client.SendMessage("Now set to attack Count.", "pink");
      }
    }

    public void AttackCountess(Client client, string[] args)
    {
      if (Program.MainForm.attackcountess)
      {
        foreach (Client runningClient in Server.Clients)
        {
          if (runningClient.Tab.Monsters != null)
          {
            foreach (targetMonster targetMonster in runningClient.targetmonster)
            {
              if (targetMonster != null && targetMonster.Text.Equals("815"))
              {
                if (AttackBossSpells.Contains(targetMonster.attack1type.Text))
                  targetMonster.attack1.Checked = false;
                if (AttackBossSpells.Contains(targetMonster.attack2type.Text))
                  targetMonster.attack2.Checked = false;
              }
            }
          }
        }
        Program.MainForm.attackcount = false;
        client.SendMessage("No longer attacking Countess.", "pink");
      }
      else
      {
        foreach (Client runningClient in Server.Clients)
        {
          if (runningClient.Tab.Monsters != null)
          {
            foreach (targetMonster targetMonster in runningClient.targetmonster)
            {
              if (targetMonster != null && targetMonster.Text.Equals("815"))
              {
                if (AttackBossSpells.Contains(targetMonster.attack1type.Text))
                  targetMonster.attack1.Checked = true;
                if (AttackBossSpells.Contains(targetMonster.attack2type.Text))
                  targetMonster.attack2.Checked = true;
              }
            }
          }
        }
        Program.MainForm.attackcount = true;
        client.SendMessage("Now set to attack Countess.", "pink");
      }
    }

    /// <summary>
    /// Yule Logs in November event.
    /// </summary>
    /// <param name="client"></param>
    public void YuleLogs(Client client, string[] args)
    {
      client.yulequest = true;
      client.Tab.autowalker_locales.SelectedItem = "Suomi";
      client.Tab.walklocaleslist.SelectedItem = "Weapon Shop";
      InitWalking(client);
      client.Tab.mediumwalk.Checked = true;
      client.LastnpcpopupID = 0U;
    }

    /// <summary>
    /// Frosty gift in December event.
    /// </summary>
    /// <param name="client"></param>
    public void Frosty(Client client, string[] args)
    {
      client.frostygift = true;
      client.Tab.autowalker_locales.SelectedItem = "Loures";
      client.Tab.walklocaleslist.SelectedItem = "Frosty (x-mas)";
      InitWalking(client);
      client.Tab.mediumwalk.Checked = true;
    }

    /// <summary>
    /// Filthy Erbie quest in December event.
    /// </summary>
    /// <remarks>Walks to mother erbie to complete the quest but does not check the killing of 20 filthy erbies</remarks>
    /// <param name="client">The client instance to configure and initiate walking. Cannot be null.</param>
    public void FilthyErbies(Client client, string[] args)
    {
      client.megprize = true;
      client.Tab.autowalker_locales.SelectedItem = "Mt Merry";
      client.Tab.walklocaleslist.SelectedItem = "Mother Erbie";
      InitWalking(client);
    }

    /// <summary>
    /// Something with counting filthy erbies.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public void CountUpErbies(Client client, string[] args)
    {
      foreach (string groupMember in client.GroupMembers)
      {
        if (groupMember != string.Empty && client.GroupCounter.ContainsKey(groupMember.ToLower()))
          ++client.GroupCounter[groupMember.ToLower()].FilthyErbieCount;
      }

    }
    /// <summary>
    /// Something with counting filthy erbies.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public void CountDownErbies(Client client, string[] args)
    {
      foreach (string groupMember in client.GroupMembers)
      {
        if (groupMember != string.Empty && client.GroupCounter.ContainsKey(groupMember.ToLower()))
          --client.GroupCounter[groupMember.ToLower()].FilthyErbieCount;
      }

    }
    /// <summary>
    /// Something with counting filthy erbies.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public void ShowCountedErbies(Client client, string[] args)
    {
      string text = string.Empty;
      foreach (string groupMember in client.GroupMembers)
      {
        if (groupMember != string.Empty && client.GroupCounter.ContainsKey(groupMember.ToLower()))
          text = text + groupMember + " : " + client.GroupCounter[groupMember.ToLower()].FilthyErbieCount.ToString() + "\n";
      }
      if (text != string.Empty)
        client.SendMessage(text, (byte)8);

    }
  }
}
