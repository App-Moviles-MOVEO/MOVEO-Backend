namespace Moveo_backend.Chat.Domain.Model.Aggregate;

/// <summary>
///     Message Aggregate Root — chat 1 a 1 entre dos usuarios.
/// </summary>
public class Message
{
    public int Id { get; private set; }
    public int SenderId { get; private set; }
    public int ReceiverId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public bool Read { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ReadAt { get; private set; }

    // Constructor para EF Core
    protected Message() { }

    public Message(int senderId, int receiverId, string content)
    {
        SenderId = senderId;
        ReceiverId = receiverId;
        Content = content;
        Read = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        if (Read) return;
        Read = true;
        ReadAt = DateTime.UtcNow;
    }
}
