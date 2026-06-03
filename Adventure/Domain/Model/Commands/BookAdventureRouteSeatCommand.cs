namespace Moveo_backend.Adventure.Domain.Model.Commands;

/// <summary>
/// Reserva asientos de carpool en una ruta (descuenta de SeatsAvailable).
/// </summary>
public record BookAdventureRouteSeatCommand(int Id, int Seats = 1);
