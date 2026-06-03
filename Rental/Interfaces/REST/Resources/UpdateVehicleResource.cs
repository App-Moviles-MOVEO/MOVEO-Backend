namespace Moveo_backend.Rental.Interfaces.REST.Resources;

public record UpdateVehicleResource(
    int OwnerId,
    string Brand,
    string Model,
    int Year,
    string Color,
    string Transmission,
    string FuelType,
    int Seats,
    string LicensePlate,
    VehicleLocationResource Location,
    decimal DailyPrice,
    decimal? DepositAmount,
    string Status,
    string? Description,
    List<string>? Images,
    List<string>? Features,
    List<string>? Restrictions
);

public record PatchVehicleResource(
    int? OwnerId = null,
    string? Brand = null,
    string? Model = null,
    int? Year = null,
    string? Color = null,
    string? Transmission = null,
    string? FuelType = null,
    int? Seats = null,
    string? LicensePlate = null,
    VehicleLocationResource? Location = null,
    decimal? DailyPrice = null,
    decimal? DepositAmount = null,
    string? Status = null,
    string? Description = null,
    List<string>? Images = null,
    List<string>? Features = null,
    List<string>? Restrictions = null
);