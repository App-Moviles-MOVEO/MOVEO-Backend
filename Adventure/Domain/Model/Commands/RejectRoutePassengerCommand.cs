namespace Moveo_backend.Adventure.Domain.Model.Commands;

/// <summary>
/// El owner rechaza la solicitud de un pasajero: pasa a REJECTED y libera el cupo si estaba confirmado.
/// </summary>
public record RejectRoutePassengerCommand(int RouteId, int PassengerId, int OwnerId);
