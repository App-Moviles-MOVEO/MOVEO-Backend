using Moveo_backend.Adventure.Domain.Model.Aggregate;
using Moveo_backend.Shared.Domain.Repositories;

namespace Moveo_backend.Adventure.Domain.Repositories;

public interface IRoutePassengerRepository : IBaseRepository<RoutePassenger>
{
    /// <summary>Todas las solicitudes de una ruta (cualquier estado).</summary>
    Task<IEnumerable<RoutePassenger>> FindByRouteIdAsync(int routeId);

    /// <summary>Solicitud activa (PENDING o CONFIRMED) de un pasajero en una ruta, si existe.</summary>
    Task<RoutePassenger?> FindActiveByRouteAndPassengerAsync(int routeId, int passengerId);
}
