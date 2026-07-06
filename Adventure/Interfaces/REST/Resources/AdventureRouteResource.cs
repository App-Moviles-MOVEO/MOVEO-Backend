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
    DateTime UpdatedAt,
    // Carpool
    DateTime? DepartureDate = null,
    string? DepartureTime = null,
    int? SeatsTotal = null,
    int? SeatsAvailable = null,
    decimal? PricePerSeat = null,
    bool OnlyWomen = false,
    string? Community = null,
    double? Lat = null,
    double? Lng = null,
    string Status = "active",
    // Pasajeros de carpool (US16). Solo se rellena en GET /adventure-routes/{id}; en listados va null.
    List<RoutePassengerResource>? Passengers = null,
    // US17 — agrupador de series recurrentes (null si la ruta no es recurrente).
    string? RecurrenceGroupId = null
);

/// <summary>
/// Cuerpo para crear una serie de rutas recurrentes semanales (US17).
/// Weekdays usa 1=Lunes .. 7=Domingo. Weeks = número de semanas a generar.
/// </summary>
public record RecurringRouteResource(
    CreateAdventureRouteResource Route,
    List<int> Weekdays,
    int Weeks
);
