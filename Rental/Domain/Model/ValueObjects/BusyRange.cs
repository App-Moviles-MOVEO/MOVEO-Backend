namespace Moveo_backend.Rental.Domain.Model.ValueObjects;

/// <summary>
/// Rango de fechas en el que un vehículo está ocupado por una reserva.
/// Convención: [StartDate, EndDate) — fin exclusivo, todo en UTC (ISO 8601).
/// </summary>
public record BusyRange(DateTime StartDate, DateTime EndDate);
