using Moveo_backend.Rental.Domain.Model.Aggregates;
using Moveo_backend.Rental.Domain.Model.Commands;
using Moveo_backend.Rental.Domain.Services;

namespace Moveo_backend.Rental.Application.CommandServices;

public class VehicleCommandService : IVehicleCommandService
{
    private readonly IVehicleService _vehicleService;

    public VehicleCommandService(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    public Task<Vehicle> CreateVehicleAsync(CreateVehicleCommand command)
        => _vehicleService.CreateVehicleAsync(command);

    public Task<Vehicle?> UpdateVehicleAsync(UpdateVehicleCommand command)
        => _vehicleService.UpdateVehicleAsync(command);

    public Task<Vehicle?> PatchVehicleAsync(PatchVehicleCommand command)
        => _vehicleService.PatchVehicleAsync(command);

    public Task<bool> DeleteVehicleAsync(int id)
        => _vehicleService.DeleteVehicleAsync(id);
}