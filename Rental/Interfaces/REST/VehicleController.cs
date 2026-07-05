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
using Moveo_backend.Shared.Infrastructure.Storage;
using Moveo_backend.Notification.Domain.Model.Commands;
using Moveo_backend.Notification.Domain.Services;

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
    private readonly IFileStorageService _fileStorage;
    private readonly INotificationCommandService _notificationCommandService;

    public VehiclesController(
        IVehicleService vehicleService,
        IRentalService rentalService,
        AppDbContext context,
        IFileStorageService fileStorage,
        INotificationCommandService notificationCommandService)
    {
        _vehicleService = vehicleService;
        _rentalService = rentalService;
        _context = context;
        _fileStorage = fileStorage;
        _notificationCommandService = notificationCommandService;
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
            resource.BodyType,
            resource.Documents?.PropertyCardFront,
            resource.Documents?.PropertyCardBack,
            resource.Documents?.Soat
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
            resource.BodyType,
            resource.Documents?.PropertyCardFront,
            resource.Documents?.PropertyCardBack,
            resource.Documents?.Soat
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
            resource.BodyType,
            resource.Documents?.PropertyCardFront,
            resource.Documents?.PropertyCardBack,
            resource.Documents?.Soat
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

    /// <summary>
    /// Sube imágenes del vehículo (multipart/form-data, campo "files"), las agrega a la galería
    /// y devuelve las URLs públicas resultantes. Reemplaza el envío de rutas locales/blob.
    /// </summary>
    [HttpPost("{id:int}/images")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(60_000_000)]
    [SwaggerOperation(
        Summary = "Upload vehicle images",
        Description = "Sube una o más imágenes del vehículo y devuelve sus URLs públicas.",
        OperationId = "UploadVehicleImages")]
    [SwaggerResponse(StatusCodes.Status201Created, "Imágenes subidas")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Sin archivos o archivo inválido")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Vehículo no encontrado")]
    public async Task<IActionResult> UploadImages(int id)
    {
        var vehicle = await _vehicleService.GetByIdAsync(id);
        if (vehicle == null)
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Vehículo no encontrado" } });

        var files = Request.Form.Files;
        if (files.Count == 0)
            return BadRequest(new { error = "no_files", message = "Envíe al menos un archivo en el campo 'files'" });

        var urls = new List<string>();
        foreach (var file in files)
        {
            if (!_fileStorage.IsAllowedFile(file, out var error))
                return BadRequest(new { error = "invalid_file", message = error });
            urls.Add(await _fileStorage.SaveAsync(file, $"vehicles/{id}", "img"));
        }

        vehicle.AddImages(urls);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new { urls, images = vehicle.Images });
    }

    /// <summary>
    /// Sube documentos de propiedad (multipart/form-data): propertyCardFront, propertyCardBack, soat.
    /// Persiste sus URLs y pasa la acreditación (ownershipStatus) a "pending".
    /// </summary>
    [HttpPost("{id:int}/documents")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(40_000_000)]
    [SwaggerOperation(
        Summary = "Upload vehicle ownership documents (US05)",
        Description = "Sube tarjeta de propiedad (frente/reverso) y SOAT; deja ownershipStatus en 'pending'.",
        OperationId = "UploadVehicleDocuments")]
    [SwaggerResponse(StatusCodes.Status200OK, "Documentos guardados", typeof(VehicleResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Ningún documento enviado o inválido")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Vehículo no encontrado")]
    public async Task<IActionResult> UploadDocuments(
        int id,
        IFormFile? propertyCardFront,
        IFormFile? propertyCardBack,
        IFormFile? soat)
    {
        var vehicle = await _vehicleService.GetByIdAsync(id);
        if (vehicle == null)
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Vehículo no encontrado" } });

        if (propertyCardFront == null && propertyCardBack == null && soat == null)
            return BadRequest(new { error = "no_files", message = "Envíe al menos un documento (propertyCardFront, propertyCardBack, soat)" });

        var frontUrl = await SaveDocAsync(id, "property_front", propertyCardFront);
        var backUrl = await SaveDocAsync(id, "property_back", propertyCardBack);
        var soatUrl = await SaveDocAsync(id, "soat", soat);

        vehicle.SubmitDocuments(frontUrl, backUrl, soatUrl);
        await _context.SaveChangesAsync();

        return Ok(await EnrichOneAsync(vehicle));
    }

    /// <summary>
    /// Resolución del admin sobre la acreditación de propiedad: approved | rejected (con motivo).
    /// </summary>
    [HttpPatch("{id:int}/ownership-status")]
    [SwaggerOperation(
        Summary = "Set vehicle ownership status (admin, US05)",
        Description = "Marca la acreditación como approved/rejected. Con rejected se guarda el motivo y se notifica al owner.",
        OperationId = "SetVehicleOwnershipStatus")]
    [SwaggerResponse(StatusCodes.Status200OK, "Estado actualizado", typeof(VehicleResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Estado inválido")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Vehículo no encontrado")]
    public async Task<IActionResult> SetOwnershipStatus(int id, [FromBody] OwnershipStatusResource resource)
    {
        var allowed = new[] { "pending", "approved", "rejected" };
        if (string.IsNullOrWhiteSpace(resource.Status) || !allowed.Contains(resource.Status))
            return BadRequest(new { error = "invalid_status", message = "status debe ser pending, approved o rejected" });

        var vehicle = await _vehicleService.GetByIdAsync(id);
        if (vehicle == null)
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Vehículo no encontrado" } });

        vehicle.SetOwnershipStatus(resource.Status, resource.RejectionReason);
        await _context.SaveChangesAsync();

        // Avisar al owner el resultado de la revisión.
        var (title, body) = resource.Status == "approved"
            ? ("Vehículo acreditado", "La propiedad de tu vehículo fue verificada correctamente.")
            : resource.Status == "rejected"
                ? ("Acreditación rechazada", $"La acreditación de tu vehículo fue rechazada. Motivo: {resource.RejectionReason ?? "no especificado"}.")
                : ("Acreditación en revisión", "Tu vehículo está pendiente de acreditación.");
        await _notificationCommandService.Handle(new CreateNotificationCommand(
            vehicle.OwnerId, title, body, "vehicle", vehicle.Id, "vehicle", null, null, null));

        return Ok(await EnrichOneAsync(vehicle));
    }

    // Guarda un documento del vehículo y devuelve su URL (o null si no se envió).
    private async Task<string?> SaveDocAsync(int vehicleId, string prefix, IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;
        if (!_fileStorage.IsAllowedFile(file, out var error))
            throw new InvalidOperationException(error);
        return await _fileStorage.SaveAsync(file, $"vehicles/{vehicleId}/documents", prefix);
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
