using Moveo_backend.Rental.Domain.Model.Commands;

namespace Moveo_backend.Rental.Domain.Services;

public interface IRentalService
{
    // Queries
    Task<Model.Aggregates.Rental?> GetByIdAsync(int id);
    Task<IEnumerable<Model.Aggregates.Rental>> GetAllAsync();
    Task<IEnumerable<Model.Aggregates.Rental>> GetFilteredAsync(int? renterId, int? ownerId, int? vehicleId, string? status);
    Task<IEnumerable<Model.Aggregates.Rental>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Model.Aggregates.Rental>> GetActiveAsync();

    // Commands
    Task<Model.Aggregates.Rental> CreateAsync(CreateRentalCommand command);
    Task<Model.Aggregates.Rental?> UpdateAsync(UpdateRentalCommand command);
    Task<Model.Aggregates.Rental?> PatchAsync(PatchRentalCommand command);
    Task<bool> DeleteAsync(int id);

    Task<bool> IsVehicleCurrentlyRentedAsync(int vehicleId);
}
