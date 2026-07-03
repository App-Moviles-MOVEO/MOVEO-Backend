namespace Moveo_backend.Adventure.Domain.Model;

/// <summary>
/// Excepción de negocio de carpooling que transporta un código de error estable
/// (ej. "no_seats_available") y el status HTTP que el controller debe devolver.
/// </summary>
public class CarpoolException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }

    public CarpoolException(string code, string message, int statusCode) : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }

    // Códigos estables (contrato con el frontend)
    public static CarpoolException RouteNotFound() =>
        new("route_not_found", "La ruta no existe", 404);

    public static CarpoolException NotRouteOwner() =>
        new("not_route_owner", "El ownerId no coincide con el dueño de la ruta", 403);

    public static CarpoolException RouteNotActive() =>
        new("route_not_active", "La ruta no está activa; no se pueden gestionar pasajeros", 409);

    public static CarpoolException NoSeatsAvailable() =>
        new("no_seats_available", "No hay asientos disponibles", 409);

    public static CarpoolException AlreadyRequested() =>
        new("already_requested", "El pasajero ya tiene una solicitud activa en esta ruta", 409);

    public static CarpoolException PassengerNotFound() =>
        new("passenger_not_found", "No existe una solicitud activa de este pasajero en la ruta", 404);

    public static CarpoolException WomenOnlyRoute() =>
        new("women_only_route", "La ruta es solo para mujeres", 403);

    public static CarpoolException CommunityRestricted() =>
        new("community_restricted", "La ruta está restringida a una comunidad", 403);
}
