namespace Moveo_backend.Adventure.Domain.Model.Aggregate;

/// <summary>
/// Solicitud de asiento de un pasajero sobre una ruta de carpool (US16).
/// El flujo es: PENDING -> (owner acepta) CONFIRMED, o (owner rechaza) REJECTED.
/// CONFIRMED -> (owner o pasajero quita) CANCELLED.
/// El cupo se descuenta de la ruta solo al pasar a CONFIRMED y se libera al rechazar/quitar.
/// </summary>
public class RoutePassenger
{
    public int Id { get; private set; }
    public int RouteId { get; private set; }
    public int PassengerId { get; private set; }
    public string Status { get; private set; } = StatusPending; // PENDING | CONFIRMED | REJECTED | CANCELLED
    public int Seats { get; private set; } = 1;
    public DateTime RequestedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public const string StatusPending = "PENDING";
    public const string StatusConfirmed = "CONFIRMED";
    public const string StatusRejected = "REJECTED";
    public const string StatusCancelled = "CANCELLED";

    // Constructor vacío para EF Core
    protected RoutePassenger()
    {
    }

    public RoutePassenger(int routeId, int passengerId, int seats)
    {
        RouteId = routeId;
        PassengerId = passengerId;
        Seats = seats < 1 ? 1 : seats;
        Status = StatusPending;
        RequestedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Solicitud activa = ocupa un lugar lógico en la lista (pendiente o confirmada).</summary>
    public bool IsActive => Status == StatusPending || Status == StatusConfirmed;

    public void Confirm()
    {
        Status = StatusConfirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        Status = StatusRejected;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = StatusCancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
