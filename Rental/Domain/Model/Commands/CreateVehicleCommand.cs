using Moveo_backend.Rental.Domain.Model.ValueObjects;

namespace Moveo_backend.Rental.Domain.Model.Commands;

public record CreateVehicleCommand(
    int OwnerId,
    string Brand,
    string Model,
    int Year,
    string Color,
    string Transmission,
    string FuelType,
    int Seats,
    string LicensePlate,
    Money DailyPrice,
    Money DepositAmount,
    Location Location,
    string? Description,
    List<string> Features,
    List<string> Restrictions,
    List<string> Images);