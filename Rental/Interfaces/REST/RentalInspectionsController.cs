using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Moveo_backend.Rental.Domain.Model.Aggregates;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Moveo_backend.Shared.Infrastructure.Storage;

namespace Moveo_backend.Rental.Interfaces.REST;

[ApiController]
[Route("api/v1/rentals/{rentalId:int}/inspections")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Checklist fotográfico de inspección de alquileres (US12)")]
public class RentalInspectionsController(
    AppDbContext context,
    IFileStorageService fileStorage) : ControllerBase
{
    /// <summary>
    /// Registra una inspección PRE o POST con sus fotos (multipart/form-data). Cada archivo
    /// se sube por su nombre de campo (front, rearSide, ...) y se guarda como punto del checklist.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(120_000_000)]
    [SwaggerOperation(Summary = "Create rental inspection", OperationId = "CreateRentalInspection")]
    [SwaggerResponse(StatusCodes.Status201Created, "Inspección registrada")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Sin fotos o tipo inválido")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Alquiler no encontrado")]
    public async Task<IActionResult> Create(
        int rentalId,
        [FromForm] string type,
        [FromForm] int? createdById,
        [FromForm] string? notes)
    {
        var rentalExists = await context.Rentals.AnyAsync(r => r.Id == rentalId);
        if (!rentalExists) return NotFound(new { error = "rental_not_found", message = "Alquiler no encontrado" });

        var normalizedType = (type ?? "").ToUpperInvariant();
        if (normalizedType != "PRE" && normalizedType != "POST")
            return BadRequest(new { error = "invalid_type", message = "type debe ser PRE o POST" });

        var files = Request.Form.Files;
        if (files.Count == 0)
            return BadRequest(new { error = "no_files", message = "Envíe al menos una foto (campos: front, rearSide, ...)" });

        var photos = new Dictionary<string, string>();
        foreach (var file in files)
        {
            if (!fileStorage.IsAllowedFile(file, out var error))
                return BadRequest(new { error = "invalid_file", message = error });
            // Normaliza nombres tipo "photos[front]" -> "front".
            var point = file.Name.Replace("photos[", "").Replace("]", "").Trim();
            if (string.IsNullOrWhiteSpace(point)) point = $"photo_{photos.Count + 1}";
            photos[point] = await fileStorage.SaveAsync(file, $"rentals/{rentalId}/inspections", $"{normalizedType.ToLowerInvariant()}_{point}");
        }

        var inspection = new RentalInspection(rentalId, normalizedType, photos, createdById, notes);
        context.RentalInspections.Add(inspection);
        await context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, ToResource(inspection));
    }

    /// <summary>Lista las inspecciones de un alquiler (para mostrarlas en disputas/incidentes).</summary>
    [HttpGet]
    [SwaggerOperation(Summary = "List rental inspections", OperationId = "GetRentalInspections")]
    [SwaggerResponse(StatusCodes.Status200OK, "Lista de inspecciones")]
    public async Task<IActionResult> List(int rentalId, [FromQuery] string? type)
    {
        var query = context.RentalInspections.Where(i => i.RentalId == rentalId);
        if (!string.IsNullOrWhiteSpace(type))
        {
            var t = type.ToUpperInvariant();
            query = query.Where(i => i.Type == t);
        }

        var inspections = await query.OrderBy(i => i.CreatedAt).ToListAsync();
        return Ok(inspections.Select(ToResource));
    }

    private static object ToResource(RentalInspection i) => new
    {
        id = i.Id,
        rentalId = i.RentalId,
        type = i.Type,
        createdById = i.CreatedById,
        notes = i.Notes,
        photos = i.Photos,
        createdAt = i.CreatedAt
    };
}
