namespace Moveo_backend.Adventure.Domain.Model.Commands;

/// <summary>Inicia una ruta (active/full -> in_progress). Requiere ownerId (dueño).</summary>
public record StartAdventureRouteCommand(int RouteId, int OwnerId);

/// <summary>Completa una ruta (in_progress -> completed). Requiere ownerId.</summary>
public record CompleteAdventureRouteCommand(int RouteId, int OwnerId);

/// <summary>Cancela una ruta, libera cupos y notifica a los pasajeros. Requiere ownerId.</summary>
public record CancelAdventureRouteCommand(int RouteId, int OwnerId);
