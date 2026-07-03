namespace Moveo_backend.Adventure.Domain.Model.Queries;

/// <summary>
/// Lista las solicitudes (PENDING/CONFIRMED) de una ruta. Requiere el ownerId que debe
/// coincidir con el dueño de la ruta.
/// </summary>
public record GetRoutePassengersQuery(int RouteId, int OwnerId);
