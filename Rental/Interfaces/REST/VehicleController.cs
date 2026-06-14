using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Moveo_backend.Rental.Domain.Services;
using Moveo_backend.Rental.Domain.Model.Aggregates;
using Moveo_backend.Rental.Interfaces.REST.Resources;
using Moveo_backend.Rental.Interfaces.REST.Transform;
using Moveo_backend.Rental.Domain.Model.ValueObjects;
using Moveo_backend.Rental.Domain.Model.Commands;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace Moveo_backend.Rental.Interfaces.REST;

[ApiController]
[Route("api/v1/vehicles")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Vehicle Endpoints")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;
    private readonly IRentalService _rentalService;
    private readonly AppDbContext _context;

    public VehiclesController(IVehicleService vehicleService, IRentalService rentalService, AppDbContext context)
    {
        _vehicleService = vehicleService;
        _rentalService = rentalService;
        _context = context;
    }

    /// <summary>
    /// Get all vehicles with optional filters
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all vehicles",
        Description = "Retrieves all vehicles with optional filtering by ownerId, status, price range, district, bodyType, " +
                      "transmission and fuelType. Si se envían startDate y endDate, excluye vehículos con reservas " +
                      "(pending/accepted/active) que se solapen con ese rango. Soporta orden por distancia (lat/lng/sort) y paginación.",
        OperationId = "GetAllVehicles"
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Vehicles retrieved successfully", typeof(IEnumerable<VehicleResource>))]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? ownerId,
        [FromQuery] string? status,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? district,
        [FromQuery] string? bodyType,
        [FromQuery] string? transmission,
        [FromQuery] string? fuelType,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] double? lat,
        [FromQuery] double? lng,
        [FromQuery] string? sort,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var vehicles = (await _vehicleService.GetFilteredAsync(
            ownerId, status, minPrice, maxPrice, district, bodyType, transmission, fuelType)).ToList();

        // P3 — Filtro por disponibilidad en el rango [startDate, endDate)
        if (startDate.HasValue && endDate.HasValue)
        {
            var start = ToUtc(startDate.Value);
            var end = ToUtc(endDate.Value);
            if (end <= start)
                return BadRequest(new { error = "invalid_request", message = "endDate debe ser posterior a startDate" });

            var busyIds = await _rentalService.GetBusyVehicleIdsAsync(vehicles.Select(v => v.Id), start, end);
            if (busyIds.Count > 0)
                vehicles = vehicles.Where(v => !busyIds.Contains(v.Id)).ToList();
        }

        // P6.1 — Orden por cercanía usando el lat/lng de cada vehículo
        if (lat.HasValue && lng.HasValue && string.Equals(sort, "distance", StringComparison.OrdinalIgnoreCase))
        {
            vehicles = vehicles
                .OrderBy(v => HaversineKm(lat.Value, lng.Value, v.Location.Lat, v.Location.Lng))
                .ToList();
        }

        // P6.3 — Paginación opcional (si no se envía, devuelve todo como antes)
        if (page.HasValue || pageSize.HasValue)
        {
            var p = page.GetValueOrDefault(1) < 1 ? 1 : page.GetValueOrDefault(1);
            var size = pageSize.GetValueOrDefault(20);
            if (size < 1) size = 20;
            vehicles = vehicles.Skip((p - 1) * size).Take(size).ToList();
        }

        var resources = await EnrichManyAsync(vehicles);
        return Ok(resources);
    }

    /// <summary>
    /// Disponibilidad de un vehículo: rangos ocupados para pintar el calendario (P2).
    /// </summary>
    [HttpGet("{id:int}/availability")]
    [SwaggerOperation(
        Summary = "Get vehicle availability",
        Description = "Devuelve los rangos ocupados (busyRanges) del vehículo en la ventana [from, to). " +
                      "Default: desde hoy hasta +3 meses. Solo cuentan reservas pending/accepted/active.",
        OperationId = "GetVehicleAvailability"
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Availability retrieved successfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Vehicle not found")]
    public async Task<IActionResult> GetAvailability(
        int id,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var vehicle = await _vehicleService.GetByIdAsync(id);
        if (vehicle == null)
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Vehículo no encontrado" } });

        var fromDate = from.HasValue ? ToUtc(from.Value) : DateTime.UtcNow.Date;
        var toDate = to.HasValue ? ToUtc(to.Value) : fromDate.AddMonths(3);
        if (toDate <= fromDate)
            return BadRequest(new { error = "invalid_request", message = "to debe ser posterior a from" });

        var busyRanges = await _rentalService.GetBusyRangesAsync(id, fromDate, toDate);
        return Ok(new { vehicleId = id, busyRanges });
    }

    // Interpreta fechas entrantes (sin zona) como UTC; respeta las que ya traen zona.
    private static DateTime ToUtc(DateTime value) =>
        value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();

    // Distancia en km entre dos coordenadas (fórmula de Haversine).
    private static double HaversineKm(double lat1, double lng1, double lat2, double lng2)
    {
        const double earthRadiusKm = 6371.0;
        double dLat = (lat2 - lat1) * Math.PI / 180.0;
        double dLng = (lng2 - lng1) * Math.PI / 180.0;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                   Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        return earthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    /// <summary>
    /// Get vehicle by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [SwaggerOperation(
        Summary = "Get vehicle by ID",
        Description = "Retrieves a specific vehicle by its ID",
        OperationId = "GetVehicleById"
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Vehicle retrieved successfully", typeof(VehicleResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Vehicle not found")]
    public async Task<IActionResult> GetById(int id)
    {
        var vehicle = await _vehicleService.GetByIdAsync(id);
        if (vehicle == null)
        {
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Vehículo no encontrado" } });
        }
        return Ok(await EnrichOneAsync(vehicle));
    }

    /// <summary>
    /// Create a new vehicle
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new vehicle",
        Description = "Creates a new vehicle with the provided information",
        OperationId = "CreateVehicle"
    )]
    [SwaggerResponse(StatusCodes.Status201Created, "Vehicle created successfully", typeof(VehicleResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data")]
    public async Task<IActionResult> Create([FromBody] CreateVehicleResource resource)
    {
        var command = new CreateVehicleCommand(
            resource.OwnerId,
            resource.Brand,
            resource.Model,
            resource.Year,
            resource.Color,
            resource.Transmission,
            resource.FuelType,
            resource.Seats,
            resource.LicensePlate,
            new Money(resource.DailyPrice),
            new Money(resource.DepositAmount ?? 0),
            new Location(
                resource.Location.District,
                resource.Location.Address,
                resource.Location.Lat,
                resource.Location.Lng
            ),
            resource.Description,
            resource.Features ?? new List<string>(),
            resource.Restrictions ?? new List<string>(),
            resource.Images ?? new List<string>(),
            resource.BodyType
        );

        var vehicle = await _vehicleService.CreateVehicleAsync(command);
        var result = await EnrichOneAsync(vehicle);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update a vehicle completely
    /// </summary>
    [HttpPut("{id:int}")]
    [SwaggerOperation(
        Summary = "Update vehicle completely",
        Description = "Updates all fields of an existing vehicle",
        OperationId = "UpdateVehicle"
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Vehicle updated successfully", typeof(VehicleResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Vehicle not found")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVehicleResource resource)
    {
        var command = new UpdateVehicleCommand(
            id,
            resource.OwnerId,
            resource.Brand,
            resource.Model,
            resource.Year,
            resource.Color,
            resource.Transmission,
            resource.FuelType,
            resource.Seats,
            resource.LicensePlate,
            new Money(resource.DailyPrice),
            new Money(resource.DepositAmount ?? 0),
            new Location(
                resource.Location.District,
                resource.Location.Address,
                resource.Location.Lat,
                resource.Location.Lng
            ),
            resource.Status,
            resource.Description,
            resource.Features ?? new List<string>(),
            resource.Restrictions ?? new List<string>(),
            resource.Images ?? new List<string>(),
            resource.BodyType
        );

        var vehicle = await _vehicleService.UpdateVehicleAsync(command);
        if (vehicle == null)
        {
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Vehículo no encontrado" } });
        }
        return Ok(await EnrichOneAsync(vehicle));
    }

    /// <summary>
    /// Partially update a vehicle
    /// </summary>
    [HttpPatch("{id:int}")]
    [SwaggerOperation(
        Summary = "Partially update vehicle",
        Description = "Updates only the specified fields of a vehicle",
        OperationId = "PatchVehicle"
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Vehicle updated successfully", typeof(VehicleResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Vehicle not found")]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchVehicleResource resource)
    {
        var command = new PatchVehicleCommand(
            id,
            resource.OwnerId,
            resource.Brand,
            resource.Model,
            resource.Year,
            resource.Color,
            resource.Transmission,
            resource.FuelType,
            resource.Seats,
            resource.LicensePlate,
            resource.DailyPrice.HasValue ? new Money(resource.DailyPrice.Value) : null,
            resource.DepositAmount.HasValue ? new Money(resource.DepositAmount.Value) : null,
            resource.Location != null ? new Location(
                resource.Location.District,
                resource.Location.Address,
                resource.Location.Lat,
                resource.Location.Lng
            ) : null,
            resource.Status,
            resource.Description,
            resource.Features,
            resource.Restrictions,
            resource.Images,
            resource.BodyType
        );

        var vehicle = await _vehicleService.PatchVehicleAsync(command);
        if (vehicle == null)
        {
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Vehículo no encontrado" } });
        }
        return Ok(await EnrichOneAsync(vehicle));
    }

    /// <summary>
    /// Delete a vehicle
    /// </summary>
    [HttpDelete("{id:int}")]
    [SwaggerOperation(
        Summary = "Delete vehicle",
        Description = "Deletes a vehicle by its ID",
        OperationId = "DeleteVehicle"
    )]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Vehicle deleted successfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Vehicle not found")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _vehicleService.DeleteVehicleAsync(id);
        if (!deleted)
        {
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Vehículo no encontrado" } });
        }
        return NoContent();
    }

    // -------------------- Enriquecimiento (ownerName, rating, reviewsCount) --------------------

    private async Task<VehicleResource> EnrichOneAsync(Vehicle vehicle)
    {
        var ownerName = await _context.Users
            .Where(u => u.Id == vehicle.OwnerId)
            .Select(u => u.FirstName + " " + u.LastName)
            .FirstOrDefaultAsync();

        var ratings = await _context.Reviews
            .Where(r => r.VehicleId == vehicle.Id)
            .Select(r => r.Rating)
            .ToListAsync();

        var avg = ratings.Count > 0 ? Math.Round(ratings.Average(), 1) : 0;
        return VehicleResourceFromEntityAssembler.ToResourceFromEntity(vehicle, ownerName, avg, ratings.Count);
    }

    private async Task<List<VehicleResource>> EnrichManyAsync(List<Vehicle> vehicles)
    {
        if (vehicles.Count == 0) return new List<VehicleResource>();

        var ownerIds = vehicles.Select(v => v.OwnerId).Distinct().ToList();
        var vehicleIds = vehicles.Select(v => v.Id).Distinct().ToList();

        var ownerNames = await _context.Users
            .Where(u => ownerIds.Contains(u.Id))
            .Select(u => new { u.Id, Name = u.FirstName + " " + u.LastName })
            .ToDictionaryAsync(x => x.Id, x => x.Name);

        var reviewStats = (await _context.Reviews
            .Where(r => r.VehicleId != null && vehicleIds.Contains(r.VehicleId!.Value))
            .Select(r => new { VehicleId = r.VehicleId!.Value, r.Rating })
            .ToListAsync())
            .GroupBy(x => x.VehicleId)
            .ToDictionary(g => g.Key, g => new { Avg = Math.Round(g.Average(x => x.Rating), 1), Count = g.Count() });

        return vehicles.Select(v =>
        {
            ownerNames.TryGetValue(v.OwnerId, out var name);
            var hasStats = reviewStats.TryGetValue(v.Id, out var stats);
            return VehicleResourceFromEntityAssembler.ToResourceFromEntity(
                v,
                name,
                hasStats ? stats!.Avg : 0,
                hasStats ? stats!.Count : 0);
        }).ToList();
    }
}
