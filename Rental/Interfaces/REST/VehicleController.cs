using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Moveo_backend.Rental.Domain.Services;
using Moveo_backend.Rental.Interfaces.REST.Resources;
using Moveo_backend.Rental.Interfaces.REST.Transform;
using Moveo_backend.Rental.Domain.Model.ValueObjects;
using Moveo_backend.Rental.Domain.Model.Commands;

namespace Moveo_backend.Rental.Interfaces.REST;

[ApiController]
[Route("api/v1/vehicles")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Vehicle Endpoints")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    /// <summary>
    /// Get all vehicles with optional filters
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all vehicles",
        Description = "Retrieves all vehicles with optional filtering by ownerId, status, price range, and district",
        OperationId = "GetAllVehicles"
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Vehicles retrieved successfully", typeof(IEnumerable<VehicleResource>))]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? ownerId,
        [FromQuery] string? status,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? district)
    {
        var vehicles = await _vehicleService.GetFilteredAsync(ownerId, status, minPrice, maxPrice, district);
        var resources = vehicles.Select(VehicleResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
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
        return Ok(VehicleResourceFromEntityAssembler.ToResourceFromEntity(vehicle));
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
            resource.Images ?? new List<string>()
        );

        var vehicle = await _vehicleService.CreateVehicleAsync(command);
        var result = VehicleResourceFromEntityAssembler.ToResourceFromEntity(vehicle);
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
            resource.Images ?? new List<string>()
        );

        var vehicle = await _vehicleService.UpdateVehicleAsync(command);
        if (vehicle == null)
        {
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Vehículo no encontrado" } });
        }
        return Ok(VehicleResourceFromEntityAssembler.ToResourceFromEntity(vehicle));
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
            resource.Images
        );

        var vehicle = await _vehicleService.PatchVehicleAsync(command);
        if (vehicle == null)
        {
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Vehículo no encontrado" } });
        }
        return Ok(VehicleResourceFromEntityAssembler.ToResourceFromEntity(vehicle));
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
}