using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Moveo_backend.Support.Domain.Model.Aggregate;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace Moveo_backend.Support.Interfaces.REST;

[ApiController]
[Route("api/v1/partnerships")]
[Produces("application/json")]
[SwaggerTag("Solicitudes de alianza corporativa (US46)")]
public class PartnershipsController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? userId, [FromQuery] string? status)
    {
        var query = context.Partnerships.AsQueryable();
        if (userId.HasValue) query = query.Where(p => p.UserId == userId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(p => p.Status == status);
        var items = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        return Ok(items.Select(ToResource));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await context.Partnerships.FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();
        return Ok(ToResource(p));
    }

    /// <summary>Crea la solicitud y la evalúa automáticamente (approved/pending).</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePartnershipResource resource)
    {
        if (string.IsNullOrWhiteSpace(resource.CompanyName) || string.IsNullOrWhiteSpace(resource.Ruc))
            return BadRequest(new { error = "invalid_request", message = "companyName y ruc son obligatorios" });

        var partnership = new Partnership(resource.UserId, resource.CompanyName, resource.Ruc,
            resource.ContactName ?? "", resource.ContactEmail, resource.ContactPhone, resource.FleetSize);
        context.Partnerships.Add(partnership);
        await context.SaveChangesAsync();
        return StatusCode(StatusCodes.Status201Created, ToResource(partnership));
    }

    /// <summary>Resuelve manualmente una solicitud en revisión (approved/rejected).</summary>
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Patch(int id, [FromBody] ResolvePartnershipResource resource)
    {
        var p = await context.Partnerships.FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();
        if (resource.Status != "approved" && resource.Status != "rejected")
            return BadRequest(new { error = "invalid_status", message = "status debe ser approved o rejected" });

        p.Resolve(resource.Status, resource.ReviewNote);
        await context.SaveChangesAsync();
        return Ok(ToResource(p));
    }

    private static object ToResource(Partnership p) => new
    {
        id = p.Id,
        userId = p.UserId,
        companyName = p.CompanyName,
        ruc = p.Ruc,
        contactName = p.ContactName,
        contactEmail = p.ContactEmail,
        contactPhone = p.ContactPhone,
        fleetSize = p.FleetSize,
        status = p.Status,
        reviewNote = p.ReviewNote,
        createdAt = p.CreatedAt,
        reviewedAt = p.ReviewedAt
    };
}

public record CreatePartnershipResource(
    int UserId,
    string CompanyName,
    string Ruc,
    string? ContactName,
    string? ContactEmail,
    string? ContactPhone,
    int FleetSize
);

public record ResolvePartnershipResource(string Status, string? ReviewNote = null);
