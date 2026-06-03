using Moveo_backend.Rental.Domain.Model.Aggregates;
using Moveo_backend.Rental.Domain.Services;

namespace Moveo_backend.Rental.Application.QueryServices;

public class VehicleQueryService
{
    private readonly IVehicleService _vehicleService;

    public VehicleQueryService(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    public Task<IEnumerable<Vehicle>> GetAllAsync() => _vehicleService.GetAllAsync();
    public Task<Vehicle?> GetByIdAsync(int id) => _vehicleService.GetByIdAsync(id);
    public Task<IEnumerable<Vehicle>> GetFilteredAsync(
        int? ownerId = null,
        string? status = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? district = null) => _vehicleService.GetFilteredAsync(ownerId, status, minPrice, maxPrice, district);
    public Task<IEnumerable<Vehicle>> GetByOwnerIdAsync(int ownerId) => _vehicleService.GetByOwnerIdAsync(ownerId);
}