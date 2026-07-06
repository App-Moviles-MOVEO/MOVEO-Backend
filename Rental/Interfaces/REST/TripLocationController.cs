using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Moveo_backend.Rental.Domain.Model.Aggregates;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace Moveo_backend.Rental.Interfaces.REST;

[ApiController]
[Route("api/v1/rentals/{rentalId:int}/location")]
[Produces("application/json")]
[SwaggerTag("Tracking en tiempo real del viaje (US06/US07)")]
public class TripLocationController(AppDbContext context) : ControllerBase
{
    /// <summary>Publica la posición actual del viaje (lo llama quien conduce). Upsert por rental.</summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Push trip location", OperationId = "PushTripLocation")]
    [SwaggerResponse(StatusCodes.Status200OK, "Posición registrada")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Alquiler no encontrado")]
    public async Task<IActionResult> Push(int rentalId, [FromBody] TripLocationResource resource)
    {
        if (!await context.Rentals.AnyAsync(r => r.Id == rentalId))
            return NotFound(new { error = "rental_not_found", message = "Alquiler no encontrado" });

        var location = await context.TripLocations.FirstOrDefaultAsync(l => l.RentalId == rentalId);
        if (location == null)
        {
            location = new TripLocation(rentalId, resource.Lat, resource.Lng);
            location.Update(resource.Lat, resource.Lng, resource.Heading, resource.Speed, resource.Progress, resource.EtaText);
            context.TripLocations.Add(location);
        }
        else
        {
            location.Update(resource.Lat, resource.Lng, resource.Heading, resource.Speed, resource.Progress, resource.EtaText);
        }

        await context.SaveChangesAsync();
        return Ok(ToResource(location));
    }

    /// <summary>Obtiene la última posición conocida del viaje (lo consulta el pasajero).</summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Get trip location", OperationId = "GetTripLocation")]
    [SwaggerResponse(StatusCodes.Status200OK, "Última posición")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Sin posición registrada")]
    public async Task<IActionResult> Get(int rentalId)
    {
        var location = await context.TripLocations.FirstOrDefaultAsync(l => l.RentalId == rentalId);
        if (location == null)
            return NotFound(new { error = "no_location", message = "Aún no hay posición para este viaje" });
        return Ok(ToResource(location));
    }

    private static object ToResource(TripLocation l) => new
    {
        rentalId = l.RentalId,
        lat = l.Lat,
        lng = l.Lng,
        heading = l.Heading,
        speed = l.Speed,
        progress = l.Progress,
        etaText = l.EtaText,
        updatedAt = l.UpdatedAt
    };
}

public record TripLocationResource(
    double Lat,
    double Lng,
    double? Heading = null,
    double? Speed = null,
    double? Progress = null,
    string? EtaText = null
);
