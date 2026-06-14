using Moveo_backend.Rental.Domain.Model.ValueObjects;

namespace Moveo_backend.Rental.Domain.Repositories;

public interface IRentalRepository
{
    Task<Model.Aggregates.Rental?> GetByIdAsync(int id);
    Task<IEnumerable<Model.Aggregates.Rental>> GetAllAsync();
    Task<IEnumerable<Model.Aggregates.Rental>> GetFilteredAsync(int? renterId, int? ownerId, int? vehicleId, string? status);
    Task<IEnumerable<Model.Aggregates.Rental>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Model.Aggregates.Rental>> GetActiveAsync();

    Task AddAsync(Model.Aggregates.Rental rental);
    Task UpdateAsync(Model.Aggregates.Rental rental);
    Task<bool> DeleteAsync(int id);

    Task<bool> IsVehicleCurrentlyRentedAsync(int vehicleId);

    /// <summary>
    /// Rangos de reservas que BLOQUEAN (pending/accepted/active) y se solapan con [start, end)
    /// para un vehículo. Usa fin exclusivo. Permite excluir una reserva (al actualizar).
    /// </summary>
    Task<IReadOnlyList<BusyRange>> GetOverlappingRangesAsync(int vehicleId, DateTime start, DateTime end, int? excludeRentalId = null);

    /// <summary>
    /// Rangos ocupados de un vehículo dentro de la ventana [from, to) — para pintar el calendario.
    /// </summary>
    Task<IReadOnlyList<BusyRange>> GetBusyRangesAsync(int vehicleId, DateTime from, DateTime to);

    /// <summary>
    /// IDs de vehículos (del conjunto dado) que tienen alguna reserva bloqueante solapada con [start, end).
    /// Para excluirlos del catálogo filtrado por fechas.
    /// </summary>
    Task<HashSet<int>> GetBusyVehicleIdsAsync(IEnumerable<int> vehicleIds, DateTime start, DateTime end);
}