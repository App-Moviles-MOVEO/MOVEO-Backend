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
}