using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Moveo_backend.Notification.Domain.Model.Aggregate;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace Moveo_backend.Notification.Interfaces.REST;

[ApiController]
[Route("api/v1/users/{userId:int}/devices")]
[Produces("application/json")]
[SwaggerTag("Registro de dispositivos para push notifications (FCM)")]
public class DevicesController(AppDbContext context) : ControllerBase
{
    /// <summary>
    /// Registra (o reactiva) el token FCM/APNs del dispositivo del usuario. El envío efectivo
    /// del push depende de configurar credenciales de Firebase en el servidor (ver handoff).
    /// </summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Register device token", OperationId = "RegisterDevice")]
    [SwaggerResponse(StatusCodes.Status201Created, "Dispositivo registrado")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "token es obligatorio")]
    public async Task<IActionResult> Register(int userId, [FromBody] RegisterDeviceResource resource)
    {
        if (string.IsNullOrWhiteSpace(resource.Token))
            return BadRequest(new { error = "invalid_request", message = "token es obligatorio" });

        var existing = await context.DeviceTokens.FirstOrDefaultAsync(d => d.Token == resource.Token);
        if (existing == null)
        {
            existing = new DeviceToken(userId, resource.Token, resource.Platform ?? "android");
            context.DeviceTokens.Add(existing);
        }
        else
        {
            existing.Reactivate(userId, resource.Platform ?? "android");
        }
        await context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new
        {
            id = existing.Id,
            userId = existing.UserId,
            platform = existing.Platform,
            active = existing.Active
        });
    }

    /// <summary>Lista los dispositivos activos del usuario.</summary>
    [HttpGet]
    [SwaggerOperation(Summary = "List user devices", OperationId = "GetUserDevices")]
    public async Task<IActionResult> List(int userId)
    {
        var devices = await context.DeviceTokens
            .Where(d => d.UserId == userId && d.Active)
            .Select(d => new { d.Id, d.Platform, d.CreatedAt })
            .ToListAsync();
        return Ok(devices);
    }

    /// <summary>Da de baja un token (logout del dispositivo).</summary>
    [HttpDelete("{token}")]
    [SwaggerOperation(Summary = "Unregister device token", OperationId = "UnregisterDevice")]
    public async Task<IActionResult> Unregister(int userId, string token)
    {
        var device = await context.DeviceTokens
            .FirstOrDefaultAsync(d => d.UserId == userId && d.Token == token);
        if (device == null) return NotFound();

        device.Deactivate();
        await context.SaveChangesAsync();
        return NoContent();
    }
}

public record RegisterDeviceResource(string Token, string? Platform = "android");
