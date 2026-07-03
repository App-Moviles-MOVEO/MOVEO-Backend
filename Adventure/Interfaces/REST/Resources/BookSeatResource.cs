namespace Moveo_backend.Adventure.Interfaces.REST.Resources;

/// <summary>
/// Cuerpo para reservar asiento(s) de carpool (POST /adventure-routes/{routeId}/book).
/// Si se envía PassengerId (US16), crea una solicitud en estado PENDING (sin descontar cupo;
/// el descuento ocurre al aceptar). Si no, conserva el comportamiento legacy (descuenta cupo).
/// </summary>
public record BookSeatResource(int Seats = 1, int PassengerId = 0);
