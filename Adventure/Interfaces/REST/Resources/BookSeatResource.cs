namespace Moveo_backend.Adventure.Interfaces.REST.Resources;

/// <summary>
/// Cuerpo para reservar asiento(s) de carpool (POST /adventure-routes/{routeId}/book).
/// PassengerId es obligatorio (US16): crea una solicitud en estado PENDING. El descuento de
/// cupo ocurre al aceptar (accept). El flujo legacy sin PassengerId fue eliminado.
/// </summary>
public record BookSeatResource(int Seats = 1, int PassengerId = 0);
