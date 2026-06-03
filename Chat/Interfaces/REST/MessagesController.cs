using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moveo_backend.Chat.Domain.Model.Aggregate;
using Moveo_backend.Chat.Interfaces.REST.Resources;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Swashbuckle.AspNetCore.Annotations;

namespace Moveo_backend.Chat.Interfaces.REST;

[ApiController]
[Route("api/v1/messages")]
[Produces("application/json")]
[SwaggerTag("Chat 1 a 1 entre usuarios")]
public class MessagesController : ControllerBase
{
    private readonly AppDbContext _context;

    public MessagesController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Conversación entre dos usuarios (ordenada por fecha ascendente).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetConversation(
        [FromQuery] int userId,
        [FromQuery] int otherUserId)
    {
        if (userId <= 0 || otherUserId <= 0)
            return BadRequest(new { message = "userId y otherUserId son requeridos" });

        var messages = await _context.Messages
            .Where(m =>
                (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                (m.SenderId == otherUserId && m.ReceiverId == userId))
            .OrderBy(m => m.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        return Ok(messages.Select(ToResource));
    }

    /// <summary>
    /// Lista de conversaciones del usuario (último mensaje + no leídos por interlocutor).
    /// </summary>
    [HttpGet("conversations/{userId:int}")]
    public async Task<IActionResult> GetConversations(int userId)
    {
        var messages = await _context.Messages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .AsNoTracking()
            .ToListAsync();

        if (messages.Count == 0) return Ok(Array.Empty<ConversationResource>());

        var conversations = messages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g =>
            {
                var last = g.OrderByDescending(m => m.CreatedAt).First();
                var unread = g.Count(m => m.ReceiverId == userId && !m.Read);
                return new { OtherUserId = g.Key, Last = last, Unread = unread };
            })
            .OrderByDescending(c => c.Last.CreatedAt)
            .ToList();

        var otherIds = conversations.Select(c => c.OtherUserId).ToList();
        var users = await _context.Users
            .Where(u => otherIds.Contains(u.Id))
            .Select(u => new { u.Id, Name = u.FirstName + " " + u.LastName, u.Avatar })
            .ToDictionaryAsync(u => u.Id);

        var result = conversations.Select(c =>
        {
            users.TryGetValue(c.OtherUserId, out var u);
            return new ConversationResource(
                c.OtherUserId,
                u?.Name,
                u?.Avatar,
                c.Last.Content,
                c.Last.CreatedAt,
                c.Unread
            );
        });

        return Ok(result);
    }

    /// <summary>
    /// Enviar un mensaje.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] CreateMessageResource resource)
    {
        if (resource.SenderId <= 0 || resource.ReceiverId <= 0)
            return BadRequest(new { message = "senderId y receiverId son requeridos" });
        if (string.IsNullOrWhiteSpace(resource.Content))
            return BadRequest(new { message = "El contenido del mensaje no puede estar vacío" });

        var message = new Message(resource.SenderId, resource.ReceiverId, resource.Content);
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetConversation),
            new { userId = message.SenderId, otherUserId = message.ReceiverId },
            ToResource(message));
    }

    /// <summary>
    /// Marcar un mensaje como leído.
    /// </summary>
    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var message = await _context.Messages.FindAsync(id);
        if (message is null) return NotFound();

        message.MarkAsRead();
        await _context.SaveChangesAsync();
        return Ok(ToResource(message));
    }

    /// <summary>
    /// Marcar como leídos todos los mensajes recibidos de otro usuario.
    /// </summary>
    [HttpPut("read")]
    public async Task<IActionResult> MarkConversationAsRead(
        [FromQuery] int userId,
        [FromQuery] int otherUserId)
    {
        var messages = await _context.Messages
            .Where(m => m.ReceiverId == userId && m.SenderId == otherUserId && !m.Read)
            .ToListAsync();

        foreach (var m in messages) m.MarkAsRead();
        await _context.SaveChangesAsync();

        return Ok(new { updated = messages.Count });
    }

    private static MessageResource ToResource(Message m) =>
        new(m.Id, m.SenderId, m.ReceiverId, m.Content, m.Read, m.CreatedAt, m.ReadAt);
}
