using Moveo_backend.Rental.Domain.Model.Aggregates;

namespace Moveo_backend.Rental.Domain.Services;

public interface IRentalQueryService
{
    Task<Model.Aggregates.Rental?> GetByIdAsync(Guid id);
    Task<IEnumerable<Model.Aggregates.Rental>> GetAllAsync();
    Task<IEnumerable<Model.Aggregates.Rental>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Model.Aggregates.Rental>> GetActiveAsync();
    Task<bool> IsVehicleCurrentlyRentedAsync(Guid vehicleId);
}