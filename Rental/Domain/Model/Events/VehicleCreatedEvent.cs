namespace Moveo_backend.Rental.Domain.Model.Events;

public record VehicleCreatedEvent(
    Guid VehicleId,
    Guid OwnerId,
    string Brand,
    string Model,
    int Year,
    string Color,
    string Transmission,
    string FuelType,
    int Seats,
    decimal DailyPrice,
    decimal DepositAmount,
    string Location);