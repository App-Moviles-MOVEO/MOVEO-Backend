namespace Moveo_backend.Rental.Interfaces.REST.Resources;

public record VehicleLocationResource(
    string District,
    string Address,
    double Lat,
    double Lng
);

public record CreateVehicleResource(
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
    string? Description,
    List<string>? Images,
    List<string>? Features,
    List<string>? Restrictions
);