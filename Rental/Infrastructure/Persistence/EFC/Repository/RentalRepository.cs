using Microsoft.EntityFrameworkCore;
using Moveo_backend.Rental.Domain;
using Moveo_backend.Rental.Domain.Model.ValueObjects;
using Moveo_backend.Rental.Domain.Repositories;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace Moveo_backend.Rental.Infrastructure.Persistence.EFC.Repository;

public class RentalRepository : IRentalRepository
{
    private readonly AppDbContext _context;

    public RentalRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Rental>> GetAllAsync()
    {
        return await _context.Rentals.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Rental>> GetFilteredAsync(
        int? renterId, int? ownerId, int? vehicleId, string? status)
    {
        var query = _context.Rentals.AsNoTracking().AsQueryable();
        
        if (renterId.HasValue)
            query = query.Where(r => r.RenterId == renterId.Value);
        if (ownerId.HasValue)
            query = query.Where(r => r.OwnerId == ownerId.Value);
        if (vehicleId.HasValue)
            query = query.Where(r => r.VehicleId == vehicleId.Value);
        if (!string.IsNullOrEmpty(status))
            query = query.Where(r => r.Status == status);
            
        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Rental>> GetActiveAsync()
    {
        return await _context.Rentals
            .Where(r => r.Status == "active")
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Domain.Model.Aggregates.Rental?> GetByIdAsync(int id)
    {
        return await _context.Rentals.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Rental>> GetByUserIdAsync(int userId)
    {
        return await _context.Rentals
            .Where(r => r.RenterId == userId || r.OwnerId == userId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(Domain.Model.Aggregates.Rental rental)
    {
        await _context.Rentals.AddAsync(rental);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Domain.Model.Aggregates.Rental rental)
    {
        _context.Rentals.Update(rental);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var rental = await _context.Rentals.FindAsync(id);
        if (rental == null) return false;
        
        _context.Rentals.Remove(rental);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsVehicleCurrentlyRentedAsync(int vehicleId)
    {
        return await _context.Rentals.AnyAsync(r => r.VehicleId == vehicleId && r.Status == "active");
    }

    public async Task<IReadOnlyList<BusyRange>> GetOverlappingRangesAsync(
        int vehicleId, DateTime start, DateTime end, int? excludeRentalId = null)
    {
        // Solapamiento estándar con rangos [start, end): start < existente.End AND existente.Start < end
        var query = _context.Rentals.AsNoTracking()
            .Where(r => r.VehicleId == vehicleId
                        && RentalStatuses.Blocking.Contains(r.Status)
                        && start < r.EndDate
                        && r.StartDate < end);

        if (excludeRentalId.HasValue)
            query = query.Where(r => r.Id != excludeRentalId.Value);

        return await query
            .OrderBy(r => r.StartDate)
            .Select(r => new BusyRange(r.StartDate, r.EndDate))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<BusyRange>> GetBusyRangesAsync(int vehicleId, DateTime from, DateTime to)
    {
        return await _context.Rentals.AsNoTracking()
            .Where(r => r.VehicleId == vehicleId
                        && RentalStatuses.Blocking.Contains(r.Status)
                        && r.EndDate > from
                        && r.StartDate < to)
            .OrderBy(r => r.StartDate)
            .Select(r => new BusyRange(r.StartDate, r.EndDate))
            .ToListAsync();
    }

    public async Task<HashSet<int>> GetBusyVehicleIdsAsync(IEnumerable<int> vehicleIds, DateTime start, DateTime end)
    {
        var ids = vehicleIds.ToList();
        if (ids.Count == 0) return new HashSet<int>();

        var busy = await _context.Rentals.AsNoTracking()
            .Where(r => ids.Contains(r.VehicleId)
                        && RentalStatuses.Blocking.Contains(r.Status)
                        && start < r.EndDate
                        && r.StartDate < end)
            .Select(r => r.VehicleId)
            .Distinct()
            .ToListAsync();

        return busy.ToHashSet();
    }
}