using Flintstones;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Slowpoke.Spells
{
  public class SpellAddContext
  {
    public Client Client;
    public int Slot;
    public int Icon;
    public int Type;
    public string Name;
    public string Prompt;
    public int CastLines;
    public int CurrentLevel;
    public int MaximumLevel;
    public string[] Captions;

    public static SpellAddContext FromPacket(Client client, ServerPacket msg)
    {
      var slot = (int)msg.ReadByte();
      var icon = (int)msg.ReadUInt16();
      var type = (int)msg.ReadByte();
      var spellInfo = msg.ReadString((int)msg.ReadByte());
      var prompt = msg.ReadString((int)msg.ReadByte());
      var castLines = (int)msg.ReadByte();
      string name = "";
      int currentLevel = 0;
      int maximumLevel = 0;

      Match match = Regex.Match(spellInfo, "(.*?) \\(Lev:(\\d+)\\/(\\d+)\\)");
      if (match.Success)
      {
        name = match.Groups[1].Value;
        currentLevel = int.Parse(match.Groups[2].Value);
        maximumLevel = int.Parse(match.Groups[3].Value);
      }

      Logger.Debug($"Spell context: Name:{name}-{currentLevel}/{maximumLevel}, slot:{slot}, icon:{icon}, type:{type}, lines:{castLines}, prompt:{prompt}");

      return new SpellAddContext
      {
        Client = client,
        Name = name,
        CastLines = castLines,
        Slot = slot,
        Captions = ReadCaptions(client.Name, name),
        CurrentLevel = currentLevel,
        MaximumLevel = maximumLevel,
        Prompt = prompt,
        Type = type,
        Icon = icon,
      };
    }

    public static string[] ReadCaptions(string clientName, string spellName)
    {
      string[] captions = new string[10];
      string filePath = Path.Combine(Options.DarkAgesDirectoryName, clientName, "SpellBook.cfg");
      if (!File.Exists(filePath))
        return captions;

      StreamReader streamReader = new StreamReader((Stream)File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));
      while (!streamReader.EndOfStream)
      {
        // Find spell name in file and read captions
        if (streamReader.ReadLine().Equals(spellName, StringComparison.CurrentCultureIgnoreCase))
        {
          for (int index = 0; index < captions.Length; ++index)
            captions[index] = streamReader.ReadLine().Split(':')[1];
        }
      }
      streamReader.Close();

      return captions;
    }

  }
}
