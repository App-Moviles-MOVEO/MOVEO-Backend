namespace Moveo_backend.Adventure.Domain.Model.Commands;

/// <summary>
/// El owner (conductor) acepta la solicitud de un pasajero: pasa a CONFIRMED y descuenta el cupo.
/// </summary>
public record AcceptRoutePassengerCommand(int RouteId, int PassengerId, int OwnerId);
