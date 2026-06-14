using Moveo_backend.Rental.Domain.Model.Commands;
using Moveo_backend.Rental.Domain.Model.ValueObjects;

namespace Moveo_backend.Rental.Domain.Services;

public interface IRentalService
{
    // Queries
    Task<Model.Aggregates.Rental?> GetByIdAsync(int id);
    Task<IEnumerable<Model.Aggregates.Rental>> GetAllAsync();
    Task<IEnumerable<Model.Aggregates.Rental>> GetFilteredAsync(int? renterId, int? ownerId, int? vehicleId, string? status);
    Task<IEnumerable<Model.Aggregates.Rental>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Model.Aggregates.Rental>> GetActiveAsync();

    // Disponibilidad por fechas
    Task<IReadOnlyList<BusyRange>> GetBusyRangesAsync(int vehicleId, DateTime from, DateTime to);
    Task<HashSet<int>> GetBusyVehicleIdsAsync(IEnumerable<int> vehicleIds, DateTime start, DateTime end);

    // Commands
    Task<Model.Aggregates.Rental> CreateAsync(CreateRentalCommand command);
    Task<Model.Aggregates.Rental?> UpdateAsync(UpdateRentalCommand command);
    Task<Model.Aggregates.Rental?> PatchAsync(PatchRentalCommand command);
    Task<bool> DeleteAsync(int id);

    Task<bool> IsVehicleCurrentlyRentedAsync(int vehicleId);
}
