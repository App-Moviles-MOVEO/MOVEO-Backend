using Moveo_backend.Adventure.Application.Internal;
using Moveo_backend.Adventure.Domain.Model.Aggregate;
using Moveo_backend.Adventure.Domain.Model.Queries;

namespace Moveo_backend.Adventure.Domain.Services;

public interface IRoutePassengerQueryService
{
    /// <summary>Devuelve la ruta y sus pasajeros (PENDING/CONFIRMED) enriquecidos. Lanza CarpoolException si no es el owner.</summary>
    Task<(AdventureRoute Route, List<RoutePassengerView> Passengers)> Handle(GetRoutePassengersQuery query);

    /// <summary>Pasajeros (PENDING/CONFIRMED) de una ruta, sin validar owner (uso interno para embeber en GET /{id}).</summary>
    Task<List<RoutePassengerView>> GetPassengersForRoute(int routeId);
}
