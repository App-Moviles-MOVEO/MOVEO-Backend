using Moveo_backend.Rental.Domain.Model.ValueObjects;

namespace Moveo_backend.Rental.Domain.Exceptions;

/// <summary>Datos inválidos al crear/actualizar una reserva. Mapea a HTTP 400.</summary>
public class RentalValidationException : Exception
{
    public RentalValidationException(string message) : base(message) { }
}

/// <summary>El vehículo solicitado no existe. Mapea a HTTP 404.</summary>
public class VehicleNotFoundException : Exception
{
    public VehicleNotFoundException(string message = "Vehículo no encontrado") : base(message) { }
}

/// <summary>El vehículo no está en estado `active` (suspendido/eliminado). Mapea a HTTP 409.</summary>
public class VehicleNotActiveException : Exception
{
    public VehicleNotActiveException(string message = "El vehículo no está disponible para alquiler")
        : base(message) { }
}

/// <summary>
/// El vehículo ya tiene una reserva que se solapa con el rango pedido. Mapea a HTTP 409.
/// </summary>
public class VehicleNotAvailableException : Exception
{
    public IReadOnlyList<BusyRange> ConflictingRanges { get; }

    public VehicleNotAvailableException(IReadOnlyList<BusyRange> conflictingRanges)
        : base("El vehículo ya tiene una reserva en ese rango de fechas")
    {
        ConflictingRanges = conflictingRanges;
    }
}
