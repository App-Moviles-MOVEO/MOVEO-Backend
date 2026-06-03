using Moveo_backend.Rental.Domain.Model.Commands;
using Moveo_backend.Rental.Domain.Repositories;

namespace Moveo_backend.Rental.Domain.Services;

public class RentalService : IRentalService
{
    private readonly IRentalRepository _rentalRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public RentalService(IRentalRepository rentalRepository, IVehicleRepository vehicleRepository)
    {
        _rentalRepository = rentalRepository;
        _vehicleRepository = vehicleRepository;
    }

    // Queries
    public Task<Model.Aggregates.Rental?> GetByIdAsync(int id) =>
        _rentalRepository.GetByIdAsync(id);

    public Task<IEnumerable<Model.Aggregates.Rental>> GetAllAsync() =>
        _rentalRepository.GetAllAsync();

    public Task<IEnumerable<Model.Aggregates.Rental>> GetFilteredAsync(int? renterId, int? ownerId, int? vehicleId, string? status) =>
        _rentalRepository.GetFilteredAsync(renterId, ownerId, vehicleId, status);

    public Task<IEnumerable<Model.Aggregates.Rental>> GetByUserIdAsync(int userId) =>
        _rentalRepository.GetByUserIdAsync(userId);

    public Task<IEnumerable<Model.Aggregates.Rental>> GetActiveAsync() =>
        _rentalRepository.GetActiveAsync();

    public Task<bool> IsVehicleCurrentlyRentedAsync(int vehicleId) =>
        _rentalRepository.IsVehicleCurrentlyRentedAsync(vehicleId);

    // Commands
    public async Task<Model.Aggregates.Rental> CreateAsync(CreateRentalCommand command)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(command.VehicleId);
        if (vehicle == null)
            throw new InvalidOperationException("Vehicle not found");

        var rental = new Model.Aggregates.Rental(
            command.VehicleId,
            command.RenterId,
            command.OwnerId,
            command.StartDate,
            command.EndDate,
            command.TotalPrice,
            command.PickupLocation,
            command.ReturnLocation,
            command.Notes,
            command.AdventureRouteId
        );

        await _rentalRepository.AddAsync(rental);
        return rental;
    }

    public async Task<Model.Aggregates.Rental?> UpdateAsync(UpdateRentalCommand command)
    {
        var rental = await _rentalRepository.GetByIdAsync(command.Id);
        if (rental == null) return null;

        rental.Update(
            command.VehicleId,
            command.RenterId,
            command.OwnerId,
            command.StartDate,
            command.EndDate,
            command.TotalPrice,
            command.Status,
            command.PickupLocation,
            command.ReturnLocation,
            command.Notes,
            command.AdventureRouteId,
            command.VehicleRated,
            command.VehicleRating,
            command.AcceptedAt,
            command.CompletedAt
        );

        await _rentalRepository.UpdateAsync(rental);
        return rental;
    }

    public async Task<Model.Aggregates.Rental?> PatchAsync(PatchRentalCommand command)
    {
        var rental = await _rentalRepository.GetByIdAsync(command.Id);
        if (rental == null) return null;

        rental.PartialUpdate(
            command.Status,
            command.VehicleRated,
            command.VehicleRating,
            command.AcceptedAt,
            command.CompletedAt
        );

        await _rentalRepository.UpdateAsync(rental);
        return rental;
    }

    public Task<bool> DeleteAsync(int id) =>
        _rentalRepository.DeleteAsync(id);
}
