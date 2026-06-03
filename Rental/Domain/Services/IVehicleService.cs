using Moveo_backend.Rental.Domain.Model.Aggregates;
using Moveo_backend.Rental.Domain.Model.Commands;

namespace Moveo_backend.Rental.Domain.Services;

public interface IVehicleService
{
    // Queries
    Task<Vehicle?> GetByIdAsync(int id);
    Task<IEnumerable<Vehicle>> GetAllAsync();
    Task<IEnumerable<Vehicle>> GetFilteredAsync(
        int? ownerId = null,
        string? status = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? district = null);
    Task<IEnumerable<Vehicle>> GetByOwnerIdAsync(int ownerId);

    // Commands
    Task<Vehicle> CreateVehicleAsync(CreateVehicleCommand command);
    Task<Vehicle?> UpdateVehicleAsync(UpdateVehicleCommand command);
    Task<Vehicle?> PatchVehicleAsync(PatchVehicleCommand command);
    Task<bool> DeleteVehicleAsync(int id);
}