using Moveo_backend.Adventure.Domain.Model.Aggregate;
using Moveo_backend.Adventure.Domain.Model.Queries;
using Moveo_backend.Adventure.Domain.Repositories;
using Moveo_backend.Adventure.Domain.Services;

namespace Moveo_backend.Adventure.Application.Internal.QueryServices;

public class AdventureRouteQueryService(IAdventureRouteRepository adventureRouteRepository) 
    : IAdventureRouteQueryService
{
    public async Task<AdventureRoute?> Handle(GetAdventureRouteByIdQuery query)
    {
        return await adventureRouteRepository.FindByIdAsync(query.Id);
    }

    public async Task<IEnumerable<AdventureRoute>> Handle(GetAllAdventureRoutesQuery query)
    {
        return await adventureRouteRepository.ListAsync();
    }

    public async Task<IEnumerable<AdventureRoute>> Handle(GetAdventureRoutesByOwnerIdQuery query)
    {
        return await adventureRouteRepository.FindByOwnerIdAsync(query.OwnerId);
    }

    public async Task<IEnumerable<AdventureRoute>> Handle(GetAdventureRoutesByTypeQuery query)
    {
        return await adventureRouteRepository.FindByTypeAsync(query.Type);
    }

    public async Task<IEnumerable<AdventureRoute>> Handle(GetFeaturedAdventureRoutesQuery query)
    {
        return await adventureRouteRepository.FindFeaturedAsync();
    }

    public async Task<IEnumerable<AdventureRoute>> Handle(GetAdventureRoutesByDifficultyQuery query)
    {
        return await adventureRouteRepository.FindByDifficultyAsync(query.Difficulty);
    }
}
