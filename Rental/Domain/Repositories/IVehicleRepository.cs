using Moveo_backend.Rental.Domain.Model.Aggregates;

namespace Moveo_backend.Rental.Domain.Repositories;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(int id);
    Task<IEnumerable<Vehicle>> GetByOwnerIdAsync(int ownerId);
    Task<IEnumerable<Vehicle>> GetAllAsync();
    Task<IEnumerable<Vehicle>> GetFilteredAsync(
        int? ownerId = null,
        string? status = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? district = null);

    Task AddAsync(Vehicle vehicle);
    Task UpdateAsync(Vehicle vehicle);
    Task<bool> DeleteAsync(int id);
}