namespace Moveo_backend.Adventure.Interfaces.REST.Resources;

public record AdventureRouteResource(
    int Id,
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
    string? VehicleName,
    string? ImageUrl,
    List<string> Tags,
    bool Featured,
    int? MaxCapacity,
    double Rating,
    int ReviewsCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
