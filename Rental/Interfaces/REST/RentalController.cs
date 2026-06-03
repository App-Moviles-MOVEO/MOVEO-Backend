using Microsoft.AspNetCore.Mvc;
using Moveo_backend.Rental.Domain.Model.Commands;
using Moveo_backend.Rental.Domain.Services;
using Moveo_backend.Rental.Interfaces.REST.Resources;

namespace Moveo_backend.Rental.Interfaces.REST;

[ApiController]
[Route("api/v1/rentals")]
public class RentalsController : ControllerBase
{
    private readonly IRentalService _rentalService;

    public RentalsController(IRentalService rentalService)
    {
        _rentalService = rentalService;
    }

    // GET /api/v1/rentals
    [HttpGet]
    public async Task<IActionResult> GetAllRentals(
        [FromQuery] int? renterId,
        [FromQuery] int? ownerId,
        [FromQuery] int? vehicleId,
        [FromQuery] string? status)
    {
        var rentals = await _rentalService.GetFilteredAsync(renterId, ownerId, vehicleId, status);
        var resources = rentals.Select(ToResource);
        return Ok(resources);
    }

    // GET /api/v1/rentals/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetRentalById(int id)
    {
        var rental = await _rentalService.GetByIdAsync(id);
        if (rental == null) return NotFound();
        return Ok(ToResource(rental));
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

        var rental = await _rentalService.CreateAsync(command);
        var rentalResource = ToResource(rental);
        return CreatedAtAction(nameof(GetRentalById), new { id = rental.Id }, rentalResource);
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
        return Ok(ToResource(rental));
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
        return Ok(ToResource(rental));
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
        var rentals = await _rentalService.GetByUserIdAsync(userId);
        var resources = rentals.Select(ToResource);
        return Ok(resources);
    }

    // GET /api/v1/rentals/active
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveRentals()
    {
        var rentals = await _rentalService.GetActiveAsync();
        var resources = rentals.Select(ToResource);
        return Ok(resources);
    }

    private static RentalResource ToResource(Domain.Model.Aggregates.Rental rental) =>
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
            rental.CompletedAt
        );
}
