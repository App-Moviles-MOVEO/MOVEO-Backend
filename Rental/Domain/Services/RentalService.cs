using Moveo_backend.Rental.Domain.Exceptions;
using Moveo_backend.Rental.Domain.Model.Commands;
using Moveo_backend.Rental.Domain.Model.ValueObjects;
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

    // Disponibilidad por fechas
    public Task<IReadOnlyList<BusyRange>> GetBusyRangesAsync(int vehicleId, DateTime from, DateTime to) =>
        _rentalRepository.GetBusyRangesAsync(vehicleId, from, to);

    public Task<HashSet<int>> GetBusyVehicleIdsAsync(IEnumerable<int> vehicleIds, DateTime start, DateTime end) =>
        _rentalRepository.GetBusyVehicleIdsAsync(vehicleIds, start, end);

    // Commands
    public async Task<Model.Aggregates.Rental> CreateAsync(CreateRentalCommand command)
    {
        // --- Validaciones de fechas y reglas de negocio (P1) ---
        if (command.EndDate <= command.StartDate)
            throw new RentalValidationException("endDate debe ser posterior a startDate");

        if (command.StartDate < DateTime.UtcNow.Date)
            throw new RentalValidationException("startDate no puede estar en el pasado");

        if (command.RenterId == command.OwnerId)
            throw new RentalValidationException("El dueño no puede reservar su propio vehículo");

        var vehicle = await _vehicleRepository.GetByIdAsync(command.VehicleId);
        if (vehicle == null)
            throw new VehicleNotFoundException();

        if (vehicle.Status != RentalStatuses.Active)
            throw new VehicleNotActiveException();

        // --- Verificación de solapamiento de fechas (la única protección real ante carreras) ---
        var conflicts = await _rentalRepository.GetOverlappingRangesAsync(
            command.VehicleId, command.StartDate, command.EndDate);
        if (conflicts.Count > 0)
            throw new VehicleNotAvailableException(conflicts);

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
