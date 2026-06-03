namespace Moveo_backend.Adventure.Interfaces.REST.Resources;

/// <summary>
/// Cuerpo para reservar asientos de carpool (POST /adventure-routes/{routeId}/book).
/// </summary>
public record BookSeatResource(int Seats = 1);
