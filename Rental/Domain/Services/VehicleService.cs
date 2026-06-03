using Moveo_backend.Rental.Domain.Model.Aggregates;
using Moveo_backend.Rental.Domain.Model.Commands;
using Moveo_backend.Rental.Domain.Repositories;

namespace Moveo_backend.Rental.Domain.Services;

public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicleRepository;

    public VehicleService(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public Task<IEnumerable<Vehicle>> GetAllAsync() => _vehicleRepository.GetAllAsync();

    public Task<IEnumerable<Vehicle>> GetFilteredAsync(
        int? ownerId = null,
        string? status = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? district = null)
    {
        return _vehicleRepository.GetFilteredAsync(ownerId, status, minPrice, maxPrice, district);
    }

    public Task<Vehicle?> GetByIdAsync(int id) => _vehicleRepository.GetByIdAsync(id);

    public Task<IEnumerable<Vehicle>> GetByOwnerIdAsync(int ownerId) => 
        _vehicleRepository.GetByOwnerIdAsync(ownerId);

    public async Task<Vehicle> CreateVehicleAsync(CreateVehicleCommand command)
    {
        var vehicle = new Vehicle(
            command.OwnerId,
            command.Brand,
            command.Model,
            command.Year,
            command.Color,
            command.Transmission,
            command.FuelType,
            command.Seats,
            command.LicensePlate,
            command.DailyPrice,
            command.DepositAmount,
            command.Location,
            command.Description,
            command.Features,
            command.Restrictions,
            command.Images
        );

        await _vehicleRepository.AddAsync(vehicle);
        return vehicle;
    }

    public async Task<Vehicle?> UpdateVehicleAsync(UpdateVehicleCommand command)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(command.Id);
        if (vehicle == null) return null;

        vehicle.UpdateDetails(
            command.OwnerId,
            command.Brand,
            command.Model,
            command.Year,
            command.Color,
            command.Transmission,
            command.FuelType,
            command.Seats,
            command.LicensePlate,
            command.DailyPrice,
            command.DepositAmount,
            command.Location,
            command.Status,
            command.Description,
            command.Features,
            command.Restrictions,
            command.Images
        );

        await _vehicleRepository.UpdateAsync(vehicle);
        return vehicle;
    }

    public async Task<Vehicle?> PatchVehicleAsync(PatchVehicleCommand command)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(command.Id);
        if (vehicle == null) return null;

        vehicle.PartialUpdate(
            command.DailyPrice?.Amount,
            command.Status,
            command.Description,
            command.Features,
            command.Restrictions,
            command.Images
        );

        await _vehicleRepository.UpdateAsync(vehicle);
        return vehicle;
    }

    public Task<bool> DeleteVehicleAsync(int id) => _vehicleRepository.DeleteAsync(id);
}
