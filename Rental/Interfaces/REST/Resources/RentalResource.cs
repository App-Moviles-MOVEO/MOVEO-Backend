namespace Moveo_backend.Rental.Interfaces.REST.Resources;

/// <summary>Resumen del arrendatario embebido en las respuestas de /rentals (evita N+1 en la app).</summary>
public record RenterSummaryResource(
    int Id,
    string FullName,
    string? AvatarUrl,
    double Reputation,
    string KycStatus
);

public record RentalResource(
    int Id,
    int VehicleId,
    int RenterId,
    int OwnerId,
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalPrice,
    string Status,
    string? PickupLocation,
    string? ReturnLocation,
    string? Notes,
    int? AdventureRouteId,
    bool? VehicleRated,
    int? VehicleRating,
    DateTime CreatedAt,
    DateTime? AcceptedAt,
    DateTime? CompletedAt,
    // Campos enriquecidos vía JOIN con Vehicles para la app móvil
    string? VehicleName = null,
    string? VehicleImage = null,
    // Resumen del arrendatario embebido (evita el GET /users/{id} extra)
    RenterSummaryResource? Renter = null
);