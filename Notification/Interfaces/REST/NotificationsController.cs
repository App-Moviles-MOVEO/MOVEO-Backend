using Microsoft.AspNetCore.Mvc;
using Moveo_backend.Notification.Domain.Model.Commands;
using Moveo_backend.Notification.Domain.Model.Queries;
using Moveo_backend.Notification.Domain.Services;
using Moveo_backend.Notification.Interfaces.REST.Resources;
using Moveo_backend.Notification.Interfaces.REST.Transform;

namespace Moveo_backend.Notification.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
public class NotificationsController(
    INotificationCommandService notificationCommandService,
    INotificationQueryService notificationQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllNotifications(
        [FromQuery] int? userId = null,
        [FromQuery] bool? read = null)
    {
        IEnumerable<Domain.Model.Aggregate.Notification> notifications;
        
        if (userId.HasValue)
        {
            if (read.HasValue && !read.Value)
            {
                var query = new GetUnreadNotificationsByUserIdQuery(userId.Value);
                notifications = await notificationQueryService.Handle(query);
            }
            else
            {
                var query = new GetNotificationsByUserIdQuery(userId.Value);
                notifications = await notificationQueryService.Handle(query);
            }
        }
        else
        {
            var query = new GetAllNotificationsQuery();
            notifications = await notificationQueryService.Handle(query);
        }
        
        var resources = notifications.Select(NotificationResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetNotificationById(int id)
    {
        var query = new GetNotificationByIdQuery(id);
        var notification = await notificationQueryService.Handle(query);
        if (notification is null) return NotFound();
        var resource = NotificationResourceFromEntityAssembler.ToResourceFromEntity(notification);
        return Ok(resource);
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetNotificationsByUserId(int userId)
    {
        var query = new GetNotificationsByUserIdQuery(userId);
        var notifications = await notificationQueryService.Handle(query);
        var resources = notifications.Select(NotificationResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("user/{userId:int}/unread")]
    public async Task<IActionResult> GetUnreadNotificationsByUserId(int userId)
    {
        var query = new GetUnreadNotificationsByUserIdQuery(userId);
        var notifications = await notificationQueryService.Handle(query);
        var resources = notifications.Select(NotificationResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpPost]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationResource resource)
    {
        var command = CreateNotificationCommandFromResourceAssembler.ToCommandFromResource(resource);
        var notification = await notificationCommandService.Handle(command);
        if (notification is null) return BadRequest();
        var notificationResource = NotificationResourceFromEntityAssembler.ToResourceFromEntity(notification);
        return CreatedAtAction(nameof(GetNotificationById), new { id = notification.Id }, notificationResource);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateNotification(int id, [FromBody] UpdateNotificationResource resource)
    {
        if (resource.Read == true)
        {
            var command = new MarkNotificationAsReadCommand(id);
            var notification = await notificationCommandService.Handle(command);
            if (notification is null) return NotFound();
            var updatedResource = NotificationResourceFromEntityAssembler.ToResourceFromEntity(notification);
            return Ok(updatedResource);
        }
        return BadRequest();
    }

    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkNotificationAsRead(int id)
    {
        var command = new MarkNotificationAsReadCommand(id);
        var notification = await notificationCommandService.Handle(command);
        if (notification is null) return NotFound();
        var resource = NotificationResourceFromEntityAssembler.ToResourceFromEntity(notification);
        return Ok(resource);
    }

    [HttpPut("user/{userId:int}/read-all")]
    public async Task<IActionResult> MarkAllNotificationsAsRead(int userId)
    {
        var command = new MarkAllNotificationsAsReadCommand(userId);
        var result = await notificationCommandService.Handle(command);
        if (!result) return BadRequest();
        return Ok(new { message = "All notifications marked as read" });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        var command = new DeleteNotificationCommand(id);
        var result = await notificationCommandService.Handle(command);
        if (!result) return NotFound();
        return NoContent();
    }
}
