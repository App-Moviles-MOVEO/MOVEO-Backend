namespace Moveo_backend.Rental.Domain;

/// <summary>
/// Estados de una reserva y reglas de disponibilidad por fechas.
/// </summary>
public static class RentalStatuses
{
    public const string Pending = "pending";
    public const string Accepted = "accepted";
    public const string Active = "active";
    public const string Cancelled = "cancelled";
    public const string Completed = "completed";

    /// <summary>
    /// Estados que BLOQUEAN las fechas de un vehículo (ocupan el rango).
    /// `cancelled` y `completed` liberan las fechas y no aparecen aquí.
    /// </summary>
    public static readonly string[] Blocking = { Pending, Accepted, Active };
}
