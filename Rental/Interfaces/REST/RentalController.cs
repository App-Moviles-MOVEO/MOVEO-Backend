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

    // -------------------- Mapeo + enriquecimiento (vehicleName/vehicleImage) --------------------

    private async Task<RentalResource> ToResourceAsync(Domain.Model.Aggregates.Rental rental)
    {
        var vehicle = await _context.Vehicles
            .Where(v => v.Id == rental.VehicleId)
            .Select(v => new { v.Brand, v.Model, v.ImagesJson })
            .FirstOrDefaultAsync();

        string? vehicleName = vehicle != null ? $"{vehicle.Brand} {vehicle.Model}" : null;
        string? vehicleImage = vehicle != null ? FirstImage(vehicle.ImagesJson) : null;

        return Map(rental, vehicleName, vehicleImage);
    }

    private async Task<List<RentalResource>> ToResourcesAsync(List<Domain.Model.Aggregates.Rental> rentals)
    {
        if (rentals.Count == 0) return new List<RentalResource>();

        var vehicleIds = rentals.Select(r => r.VehicleId).Distinct().ToList();
        var vehicles = await _context.Vehicles
            .Where(v => vehicleIds.Contains(v.Id))
            .Select(v => new { v.Id, v.Brand, v.Model, v.ImagesJson })
            .ToDictionaryAsync(v => v.Id);

        return rentals.Select(r =>
        {
            vehicles.TryGetValue(r.VehicleId, out var v);
            string? name = v != null ? $"{v.Brand} {v.Model}" : null;
            string? image = v != null ? FirstImage(v.ImagesJson) : null;
            return Map(r, name, image);
        }).ToList();
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

    private static RentalResource Map(Domain.Model.Aggregates.Rental rental, string? vehicleName, string? vehicleImage) =>
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
            vehicleImage
        );
}
