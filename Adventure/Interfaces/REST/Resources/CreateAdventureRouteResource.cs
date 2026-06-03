namespace Moveo_backend.Adventure.Interfaces.REST.Resources;

public record CreateAdventureRouteResource(
    int OwnerId,
    string Name,
    string Title,
    string Description,
    string StartLocation,
    string EndLocation,
    string Type,
    int Duration,
    string Difficulty,
    decimal EstimatedCost,
    string? VehicleName = null,
    string? ImageUrl = null,
    List<string>? Tags = null,
    bool Featured = false,
    int? MaxCapacity = null,
    // Carpool (opcional)
    DateTime? DepartureDate = null,
    string? DepartureTime = null,
    int? SeatsTotal = null,
    int? SeatsAvailable = null,
    decimal? PricePerSeat = null,
    bool OnlyWomen = false,
    string? Community = null,
    double? Lat = null,
    double? Lng = null
);
