namespace Moveo_backend.Rental.Interfaces.REST.Resources;

public record VehicleResource(
    int Id,
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
    List<string>? Restrictions,
    DateTime CreatedAt,
    DateTime UpdatedAt
);