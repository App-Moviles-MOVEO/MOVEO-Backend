namespace Moveo_backend.Rental.Domain.Queries;

public record GetAvailableVehiclesQuery(
    string? Location = null,
    string? Brand = null,
    string? FuelType = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null);