namespace Moveo_backend.Adventure.Interfaces.REST.Resources;

/// <summary>
/// Respuesta de GET /adventure-routes/{routeId}/passengers (US16 §4.1).
/// </summary>
public record RoutePassengersResponse(
    int RouteId,
    int? SeatsTotal,
    int? SeatsAvailable,
    List<RoutePassengerResource> Passengers
);
