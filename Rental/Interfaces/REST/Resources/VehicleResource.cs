namespace Moveo_backend.Rental.Interfaces.REST.Resources;

/// <summary>Documentos de propiedad del vehículo (US05).</summary>
public record VehicleDocumentsResource(
    string? PropertyCardFront,
    string? PropertyCardBack,
    string? Soat
);

/// <summary>Cuerpo para que un admin resuelva la acreditación de propiedad (US05).</summary>
public record OwnershipStatusResource(
    string Status,
    string? RejectionReason = null
);

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
    DateTime UpdatedAt,
    // Campos enriquecidos vía JOIN/cálculo para la app móvil
    string? BodyType = null,
    string? OwnerName = null,
    double Rating = 0,
    int ReviewsCount = 0,
    // US05 — documentos de propiedad y estado de acreditación
    VehicleDocumentsResource? Documents = null,
    string OwnershipStatus = "not_submitted",
    string? OwnershipRejectionReason = null
);