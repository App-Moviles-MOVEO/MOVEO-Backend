using Moveo_backend.Rental.Domain.Model.ValueObjects;

namespace Moveo_backend.Rental.Domain.Model.Commands;

public record UpdateVehicleCommand(
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
    Money DailyPrice,
    Money DepositAmount,
    Location Location,
    string Status,
    string? Description,
    List<string> Features,
    List<string> Restrictions,
    List<string> Images);

public record PatchVehicleCommand(
    int Id,
    int? OwnerId = null,
    string? Brand = null,
    string? Model = null,
    int? Year = null,
    string? Color = null,
    string? Transmission = null,
    string? FuelType = null,
    int? Seats = null,
    string? LicensePlate = null,
    Money? DailyPrice = null,
    Money? DepositAmount = null,
    Location? Location = null,
    string? Status = null,
    string? Description = null,
    List<string>? Features = null,
    List<string>? Restrictions = null,
    List<string>? Images = null);

public record DeleteVehicleCommand(int Id);