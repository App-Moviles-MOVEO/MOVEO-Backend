using Microsoft.EntityFrameworkCore;
using Moveo_backend.Adventure.Domain.Model.Aggregate;
using Moveo_backend.Adventure.Domain.Repositories;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace Moveo_backend.Adventure.Infrastructure.Persistence.EFC.Repositories;

public class AdventureRouteRepository(AppDbContext context) 
    : BaseRepository<AdventureRoute>(context), IAdventureRouteRepository
{
    public async Task<IEnumerable<AdventureRoute>> FindByOwnerIdAsync(int ownerId)
    {
        return await Context.Set<AdventureRoute>()
            .Where(route => route.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<AdventureRoute>> FindByTypeAsync(string type)
    {
        return await Context.Set<AdventureRoute>()
            .Where(route => route.Type == type)
            .ToListAsync();
    }

    public async Task<IEnumerable<AdventureRoute>> FindFeaturedAsync()
    {
        return await Context.Set<AdventureRoute>()
            .Where(route => route.Featured)
            .ToListAsync();
    }

    public async Task<IEnumerable<AdventureRoute>> FindByDifficultyAsync(string difficulty)
    {
        return await Context.Set<AdventureRoute>()
            .Where(route => route.Difficulty == difficulty)
            .ToListAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await Context.Set<AdventureRoute>()
            .AnyAsync(route => route.Name == name);
    }
}
