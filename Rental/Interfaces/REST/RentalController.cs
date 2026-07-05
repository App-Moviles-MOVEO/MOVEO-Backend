using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moveo_backend.Rental.Domain.Exceptions;
using Moveo_backend.Rental.Domain.Model.Commands;
using Moveo_backend.Rental.Domain.Services;
using Moveo_backend.Rental.Interfaces.REST.Resources;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Moveo_backend.Payment.Domain.Model.Commands;
using Moveo_backend.Payment.Domain.Services;
using Moveo_backend.Payment.Interfaces.REST.Transform;

namespace Moveo_backend.Rental.Interfaces.REST;

[ApiController]
[Route("api/v1/rentals")]
public class RentalsController : ControllerBase
{
    private readonly IRentalService _rentalService;
    private readonly IPaymentCommandService _paymentCommandService;
    private readonly AppDbContext _context;

    public RentalsController(
        IRentalService rentalService,
        IPaymentCommandService paymentCommandService,
        AppDbContext context)
    {
        _rentalService = rentalService;
        _paymentCommandService = paymentCommandService;
        _context = context;
    }

    // GET /api/v1/rentals
    [HttpGet]
    public async Task<IActionResult> GetAllRentals(
        [FromQuery] int? renterId,
        [FromQuery] int? ownerId,
        [FromQuery] int? vehicleId,
        [FromQuery] string? status)
    {
        var rentals = (await _rentalService.GetFilteredAsync(renterId, ownerId, vehicleId, status)).ToList();
        return Ok(await ToResourcesAsync(rentals));
    }

    // GET /api/v1/rentals/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetRentalById(int id)
    {
        var rental = await _rentalService.GetByIdAsync(id);
        if (rental == null) return NotFound();
        return Ok(await ToResourceAsync(rental));
    }

    // POST /api/v1/rentals
    [HttpPost]
    public async Task<IActionResult> CreateRental([FromBody] CreateRentalResource resource)
    {
        var command = new CreateRentalCommand(
            resource.VehicleId,
            resource.RenterId,
            resource.OwnerId,
            resource.StartDate,
            resource.EndDate,
            resource.TotalPrice,
            resource.PickupLocation,
            resource.ReturnLocation,
            resource.Notes,
            resource.AdventureRouteId
        );

        try
        {
            var rental = await _rentalService.CreateAsync(command);
            var rentalResource = await ToResourceAsync(rental);
            return CreatedAtAction(nameof(GetRentalById), new { id = rental.Id }, rentalResource);
        }
        catch (RentalValidationException ex)
        {
            return BadRequest(new { error = "invalid_request", message = ex.Message });
        }
        catch (VehicleNotFoundException ex)
        {
            return NotFound(new { error = "vehicle_not_found", message = ex.Message });
        }
        catch (VehicleNotActiveException ex)
        {
            return Conflict(new { error = "vehicle_not_active", message = ex.Message });
        }
        catch (VehicleNotAvailableException ex)
        {
            return Conflict(new
            {
                error = "vehicle_not_available",
                message = ex.Message,
                conflictingRanges = ex.ConflictingRanges
            });
        }
    }

    // PUT /api/v1/rentals/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateRental(int id, [FromBody] UpdateRentalResource resource)
    {
        var command = new UpdateRentalCommand(
            id,
            resource.VehicleId,
            resource.RenterId,
            resource.OwnerId,
            resource.StartDate,
            resource.EndDate,
            resource.TotalPrice,
            resource.Status,
            resource.PickupLocation,
            resource.ReturnLocation,
            resource.Notes,
            resource.AdventureRouteId,
            resource.VehicleRated,
            resource.VehicleRating,
            resource.AcceptedAt,
            resource.CompletedAt
        );

        var rental = await _rentalService.UpdateAsync(command);
        if (rental == null) return NotFound();
        return Ok(await ToResourceAsync(rental));
    }

    // PATCH /api/v1/rentals/{id}
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> PatchRental(int id, [FromBody] PatchRentalResource resource)
    {
        var command = new PatchRentalCommand(
            id,
            resource.Status,
            resource.VehicleRated,
            resource.VehicleRating,
            resource.AcceptedAt,
            resource.CompletedAt
        );

        var rental = await _rentalService.PatchAsync(command);
        if (rental == null) return NotFound();
        return Ok(await ToResourceAsync(rental));
    }

    // DELETE /api/v1/rentals/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteRental(int id)
    {
        var deleted = await _rentalService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    // GET /api/v1/rentals/user/{userId}
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        var rentals = (await _rentalService.GetByUserIdAsync(userId)).ToList();
        return Ok(await ToResourcesAsync(rentals));
    }

    // GET /api/v1/rentals/active
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveRentals()
    {
        var rentals = (await _rentalService.GetActiveAsync()).ToList();
        return Ok(await ToResourcesAsync(rentals));
    }

    // POST /api/v1/rentals/{id}/pay  -> crea y completa el pago de la reserva en un solo paso
    [HttpPost("{id:int}/pay")]
    public async Task<IActionResult> PayRental(int id, [FromBody] PayRentalResource resource)
    {
        var rental = await _rentalService.GetByIdAsync(id);
        if (rental == null) return NotFound(new { message = "Reserva no encontrada" });

        var amount = resource.Amount ?? rental.TotalPrice;
        var method = string.IsNullOrWhiteSpace(resource.PaymentMethod) ? "yape" : resource.PaymentMethod!;

        var createCommand = new CreatePaymentCommand(
            PayerId: rental.RenterId,
            RecipientId: rental.OwnerId,
            RentalId: rental.Id,
            Amount: amount,
            Currency: resource.Currency ?? "PEN",
            Method: method,
            Type: resource.Type ?? "rental_payment",
            Description: resource.Description
        );

        var payment = await _paymentCommandService.Handle(createCommand);
        if (payment == null) return BadRequest(new { message = "No se pudo crear el pago" });

        // Marcar el pago como completado (Yape es pago directo)
        var completed = await _paymentCommandService.Handle(new PatchPaymentCommand(
            payment.Id,
            Status: "completed",
            TransactionId: resource.TransactionId,
            CompletedAt: DateTime.UtcNow
        )) ?? payment;

        var result = new
        {
            rental = await ToResourceAsync(rental),
            payment = PaymentResourceFromEntityAssembler.ToResourceFromEntity(completed)
        };
        return Ok(result);
    }

    // GET /api/v1/rentals/{id}/invoice  -> comprobante con numeración correlativa oficial
    [HttpGet("{id:int}/invoice")]
    public async Task<IActionResult> GetInvoice(int id)
    {
        var rental = await _rentalService.GetByIdAsync(id);
        if (rental == null) return NotFound(new { message = "Reserva no encontrada" });

        // El comprobante se emite sobre el pago completado de la reserva.
        var payment = await _context.Payments
            .Where(p => p.RentalId == id && p.Status == "completed" && p.Type != "refund")
            .OrderBy(p => p.Id)
            .FirstOrDefaultAsync();
        if (payment == null)
            return UnprocessableEntity(new { error = "no_completed_payment", message = "La reserva no tiene un pago completado" });

        // Numeración correlativa determinística: WPE-{año}-{id de pago con padding}.
        var invoiceNumber = $"WPE-{payment.CreatedAt:yyyy}-{payment.Id:D6}";

        var renter = await _context.Users.Where(u => u.Id == rental.RenterId)
            .Select(u => new { u.FirstName, u.LastName, u.Dni, u.Email }).FirstOrDefaultAsync();
        var vehicle = await _context.Vehicles.Where(v => v.Id == rental.VehicleId)
            .Select(v => new { v.Brand, v.Model, v.LicensePlate }).FirstOrDefaultAsync();

        var days = Math.Max(1, (int)Math.Ceiling((rental.EndDate - rental.StartDate).TotalDays));

        return Ok(new
        {
            invoiceNumber,
            issuedAt = payment.CompletedAt ?? payment.CreatedAt,
            rentalId = rental.Id,
            status = "issued",
            customer = renter == null ? null : new
            {
                fullName = $"{renter.FirstName} {renter.LastName}".Trim(),
                dni = renter.Dni,
                email = renter.Email
            },
            vehicle = vehicle == null ? null : new
            {
                name = $"{vehicle.Brand} {vehicle.Model}",
                licensePlate = vehicle.LicensePlate
            },
            period = new { start = rental.StartDate, end = rental.EndDate, days },
            payment = new
            {
                id = payment.Id,
                method = payment.Method,
                currency = payment.Currency,
                transactionId = payment.TransactionId
            },
            amount = new
            {
                total = payment.Amount,
                currency = payment.Currency
            }
        });
    }

    // -------------------- Mapeo + enriquecimiento (vehicleName/vehicleImage) --------------------

    private async Task<RentalResource> ToResourceAsync(Domain.Model.Aggregates.Rental rental)
    {
        var vehicle = await _context.Vehicles
            .Where(v => v.Id == rental.VehicleId)
            .Select(v => new { v.Brand, v.Model, v.ImagesJson })
            .FirstOrDefaultAsync();

        string? vehicleName = vehicle != null ? $"{vehicle.Brand} {vehicle.Model}" : null;
        string? vehicleImage = vehicle != null ? FirstImage(vehicle.ImagesJson) : null;

        var renters = await LoadRentersAsync(new[] { rental.RenterId });
        renters.TryGetValue(rental.RenterId, out var renter);

        return Map(rental, vehicleName, vehicleImage, renter);
    }

    private async Task<List<RentalResource>> ToResourcesAsync(List<Domain.Model.Aggregates.Rental> rentals)
    {
        if (rentals.Count == 0) return new List<RentalResource>();

        var vehicleIds = rentals.Select(r => r.VehicleId).Distinct().ToList();
        var vehicles = await _context.Vehicles
            .Where(v => vehicleIds.Contains(v.Id))
            .Select(v => new { v.Id, v.Brand, v.Model, v.ImagesJson })
            .ToDictionaryAsync(v => v.Id);

        var renters = await LoadRentersAsync(rentals.Select(r => r.RenterId).Distinct());

        return rentals.Select(r =>
        {
            vehicles.TryGetValue(r.VehicleId, out var v);
            string? name = v != null ? $"{v.Brand} {v.Model}" : null;
            string? image = v != null ? FirstImage(v.ImagesJson) : null;
            renters.TryGetValue(r.RenterId, out var renter);
            return Map(r, name, image, renter);
        }).ToList();
    }

    // Carga el resumen (nombre, avatar, reputación, KYC) de los arrendatarios en una sola pasada.
    private async Task<Dictionary<int, RenterSummaryResource>> LoadRentersAsync(IEnumerable<int> renterIds)
    {
        var ids = renterIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<int, RenterSummaryResource>();

        var users = await _context.Users
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.Avatar, u.KycStatus })
            .ToListAsync();

        // Reputación = promedio de las reseñas recibidas como usuario.
        var reputations = (await _context.UserReviews
                .Where(r => ids.Contains(r.ReviewedUserId))
                .Select(r => new { r.ReviewedUserId, r.Rating })
                .ToListAsync())
            .GroupBy(x => x.ReviewedUserId)
            .ToDictionary(g => g.Key, g => Math.Round(g.Average(x => (double)x.Rating), 2));

        return users.ToDictionary(
            u => u.Id,
            u => new RenterSummaryResource(
                u.Id,
                $"{u.FirstName} {u.LastName}".Trim(),
                u.Avatar,
                reputations.TryGetValue(u.Id, out var rep) ? rep : 0,
                u.KycStatus));
    }

    private static string? FirstImage(string? imagesJson)
    {
        if (string.IsNullOrWhiteSpace(imagesJson)) return null;
        try
        {
            var list = System.Text.Json.JsonSerializer.Deserialize<List<string>>(imagesJson);
            return list != null && list.Count > 0 ? list[0] : null;
        }
        catch
        {
            return null;
        }
    }

    private static RentalResource Map(
        Domain.Model.Aggregates.Rental rental,
        string? vehicleName,
        string? vehicleImage,
        RenterSummaryResource? renter) =>
        new(
            rental.Id,
            rental.VehicleId,
            rental.RenterId,
            rental.OwnerId,
            rental.StartDate,
            rental.EndDate,
            rental.TotalPrice,
            rental.Status,
            rental.PickupLocation,
            rental.ReturnLocation,
            rental.Notes,
            rental.AdventureRouteId,
            rental.VehicleRated,
            rental.VehicleRating,
            rental.CreatedAt,
            rental.AcceptedAt,
            rental.CompletedAt,
            vehicleName,
            vehicleImage,
            renter
        );
}
