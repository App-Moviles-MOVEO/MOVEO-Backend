using Moveo_backend.Adventure.Domain.Model.Aggregate;
using Moveo_backend.Shared.Domain.Repositories;

namespace Moveo_backend.Adventure.Domain.Repositories;

public interface IAdventureRouteRepository : IBaseRepository<AdventureRoute>
{
    Task<IEnumerable<AdventureRoute>> FindByOwnerIdAsync(int ownerId);
    Task<IEnumerable<AdventureRoute>> FindByTypeAsync(string type);
    Task<IEnumerable<AdventureRoute>> FindFeaturedAsync();
    Task<IEnumerable<AdventureRoute>> FindByDifficultyAsync(string difficulty);
    Task<bool> ExistsByNameAsync(string name);
}
