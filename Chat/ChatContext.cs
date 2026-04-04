using Flintstones;

namespace Slowpoke.Chat
{
  public class ChatContext
  {
    public Client Client;
    public byte Type;
    public uint SenderId;
    public string RawMessage;
    public string Message;

    /// <summary>
    /// Parsing chat information from a server packet.
    /// </summary>
    /// <param name="client">The client associated with the chat message. Cannot be null.</param>
    /// <param name="msg">The server packet containing the chat data to parse. Must be positioned at the start of the chat message data.</param>
    /// <returns>A ChatContext object containing the parsed chat type, sender ID, raw message, and processed message text.</returns>
    public static ChatContext FromPacket(Client client, ServerPacket msg)
    {
      var type = msg.ReadByte();
      var sender = msg.ReadUInt32();
      var raw = msg.ReadString(msg.ReadByte());

      var message = raw;

      if (type == 0 || type == 1)
        message = raw.Remove(0, raw.IndexOf(" ") + 1);

      return new ChatContext
      {
        Client = client,
        Type = type,
        SenderId = sender,
        RawMessage = raw,
        Message = message
      };
    }
  }
}
