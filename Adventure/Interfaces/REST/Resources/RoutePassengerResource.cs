namespace Moveo_backend.Adventure.Interfaces.REST.Resources;

/// <summary>
/// Solicitud de asiento expuesta al frontend. Los nombres de campo respetan el contrato del
/// parser del frontend (RoutePassenger.fromJson): id, passengerId, fullName, avatarUrl,
/// reputation, verificationStatus ("VERIFIED"/"UNVERIFIED"), status ("PENDING"/"CONFIRMED"/...).
/// </summary>
public record RoutePassengerResource(
    int Id,
    int PassengerId,
    string FullName,
    string? AvatarUrl,
    double Reputation,
    string VerificationStatus,
    string Status,
    int Seats,
    DateTime RequestedAt
);
