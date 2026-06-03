using Moveo_backend.Rental.Domain.Model.Aggregates;


namespace Moveo_backend.Rental.Domain.Services;

public interface IVehicleQueryService
{
    Task<Vehicle?> GetByIdAsync(Guid id);
    Task<IEnumerable<Vehicle>> GetAllAsync();
    Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync();
    Task<IEnumerable<Vehicle>> GetByOwnerIdAsync(Guid ownerId);
}