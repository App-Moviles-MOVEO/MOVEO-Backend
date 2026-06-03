using Microsoft.EntityFrameworkCore;
using Moveo_backend.Rental.Domain.Model.Aggregates;
using Moveo_backend.Rental.Domain.Repositories;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace Moveo_backend.Rental.Infrastructure.Persistence.EFC.Repository;

public class VehicleRepository : IVehicleRepository
{
    private readonly AppDbContext _context;

    public VehicleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Vehicle>> GetAllAsync()
    {
        return await _context.Vehicles.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Vehicle>> GetByOwnerIdAsync(int ownerId)
    {
        return await _context.Vehicles
            .Where(v => v.OwnerId == ownerId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Vehicle>> GetFilteredAsync(
        int? ownerId = null,
        string? status = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? district = null)
    {
        var query = _context.Vehicles.AsQueryable();

        if (ownerId.HasValue) 
            query = query.Where(v => v.OwnerId == ownerId.Value);
        if (!string.IsNullOrEmpty(status)) 
            query = query.Where(v => v.Status == status);
        if (minPrice.HasValue) 
            query = query.Where(v => v.DailyPrice.Amount >= minPrice.Value);
        if (maxPrice.HasValue) 
            query = query.Where(v => v.DailyPrice.Amount <= maxPrice.Value);
        if (!string.IsNullOrEmpty(district)) 
            query = query.Where(v => v.Location.District.Contains(district));

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<Vehicle?> GetByIdAsync(int id)
    {
        return await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task AddAsync(Vehicle vehicle)
    {
        await _context.Vehicles.AddAsync(vehicle);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Vehicle vehicle)
    {
        _context.Vehicles.Update(vehicle);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle == null) return false;
        
        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync();
        return true;
    }
}
