using Microsoft.EntityFrameworkCore;
using Moveo_backend.Adventure.Domain.Model.Aggregate;
using Moveo_backend.Adventure.Domain.Repositories;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace Moveo_backend.Adventure.Infrastructure.Persistence.EFC.Repositories;

public class RoutePassengerRepository(AppDbContext context)
    : BaseRepository<RoutePassenger>(context), IRoutePassengerRepository
{
    public async Task<IEnumerable<RoutePassenger>> FindByRouteIdAsync(int routeId)
    {
        return await Context.Set<RoutePassenger>()
            .Where(p => p.RouteId == routeId)
            .OrderBy(p => p.RequestedAt)
            .ToListAsync();
    }

    public async Task<RoutePassenger?> FindActiveByRouteAndPassengerAsync(int routeId, int passengerId)
    {
        return await Context.Set<RoutePassenger>()
            .Where(p => p.RouteId == routeId
                        && p.PassengerId == passengerId
                        && (p.Status == RoutePassenger.StatusPending || p.Status == RoutePassenger.StatusConfirmed))
            .OrderByDescending(p => p.RequestedAt)
            .FirstOrDefaultAsync();
    }
}
