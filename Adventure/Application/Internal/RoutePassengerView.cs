using Moveo_backend.Adventure.Domain.Model.Aggregate;

namespace Moveo_backend.Adventure.Application.Internal;

/// <summary>
/// Solicitud de pasajero enriquecida con datos del usuario (nombre, avatar, reputación,
/// verificación) para exponerla por la API sin que el frontend tenga que hacer requests extra.
/// </summary>
public record RoutePassengerView(
    RoutePassenger Passenger,
    string FullName,
    string? AvatarUrl,
    double Reputation,
    bool Verified
);
