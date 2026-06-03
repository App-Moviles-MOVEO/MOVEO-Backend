namespace Moveo_backend.Chat.Interfaces.REST.Resources;

public record CreateMessageResource(
    int SenderId,
    int ReceiverId,
    string Content
);

public record MessageResource(
    int Id,
    int SenderId,
    int ReceiverId,
    string Content,
    bool Read,
    DateTime CreatedAt,
    DateTime? ReadAt
);

/// <summary>
/// Resumen de una conversación (para la lista de chats del usuario).
/// </summary>
public record ConversationResource(
    int OtherUserId,
    string? OtherUserName,
    string? OtherUserAvatar,
    string LastMessage,
    DateTime LastMessageAt,
    int UnreadCount
);
