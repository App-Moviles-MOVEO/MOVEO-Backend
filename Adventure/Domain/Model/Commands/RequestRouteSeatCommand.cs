namespace Moveo_backend.Adventure.Domain.Model.Commands;

/// <summary>
/// El pasajero pide un asiento en una ruta de carpool. Crea la solicitud en estado PENDING
/// (no descuenta cupo todavía; el descuento ocurre al aceptar).
/// </summary>
public record RequestRouteSeatCommand(int RouteId, int PassengerId, int Seats = 1);
