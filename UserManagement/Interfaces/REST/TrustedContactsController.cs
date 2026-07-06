using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Moveo_backend.UserManagement.Domain.Model.Aggregates;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace Moveo_backend.UserManagement.Interfaces.REST;

[ApiController]
[Route("api/v1/users/{userId:int}/trusted-contacts")]
[Produces("application/json")]
[SwaggerTag("Contactos de confianza (US10)")]
public class TrustedContactsController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(int userId)
    {
        var contacts = await context.TrustedContacts
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
        return Ok(contacts.Select(ToResource));
    }

    [HttpPost]
    public async Task<IActionResult> Create(int userId, [FromBody] TrustedContactResource resource)
    {
        if (string.IsNullOrWhiteSpace(resource.Name) || string.IsNullOrWhiteSpace(resource.Phone))
            return BadRequest(new { error = "invalid_request", message = "name y phone son obligatorios" });

        var contact = new TrustedContact(userId, resource.Name, resource.Phone, resource.Relationship);
        context.TrustedContacts.Add(contact);
        await context.SaveChangesAsync();
        return StatusCode(StatusCodes.Status201Created, ToResource(contact));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int userId, int id)
    {
        var contact = await context.TrustedContacts.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (contact == null) return NotFound();
        context.TrustedContacts.Remove(contact);
        await context.SaveChangesAsync();
        return NoContent();
    }

    private static object ToResource(TrustedContact c) => new
    {
        id = c.Id,
        userId = c.UserId,
        name = c.Name,
        phone = c.Phone,
        relationship = c.Relationship,
        createdAt = c.CreatedAt
    };
}

public record TrustedContactResource(string Name, string Phone, string? Relationship = null);
