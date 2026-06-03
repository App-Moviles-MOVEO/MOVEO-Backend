using Moveo_backend.Adventure.Domain.Model.Aggregate;
using Moveo_backend.Adventure.Domain.Model.Queries;

namespace Moveo_backend.Adventure.Domain.Services;

public interface IAdventureRouteQueryService
{
    Task<AdventureRoute?> Handle(GetAdventureRouteByIdQuery query);
    Task<IEnumerable<AdventureRoute>> Handle(GetAllAdventureRoutesQuery query);
    Task<IEnumerable<AdventureRoute>> Handle(GetAdventureRoutesByOwnerIdQuery query);
    Task<IEnumerable<AdventureRoute>> Handle(GetAdventureRoutesByTypeQuery query);
    Task<IEnumerable<AdventureRoute>> Handle(GetFeaturedAdventureRoutesQuery query);
    Task<IEnumerable<AdventureRoute>> Handle(GetAdventureRoutesByDifficultyQuery query);
}
