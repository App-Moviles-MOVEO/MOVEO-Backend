namespace Moveo_backend.Adventure.Domain.Model.Commands;

/// <summary>
/// El owner quita a un pasajero confirmado: pasa a CANCELLED y libera el cupo.
/// </summary>
public record RemoveRoutePassengerCommand(int RouteId, int PassengerId, int OwnerId);
