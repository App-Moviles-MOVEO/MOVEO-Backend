using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Moveo_backend.Adventure.Domain.Model;
using Moveo_backend.Adventure.Domain.Model.Commands;
using Moveo_backend.Adventure.Domain.Model.Queries;
using Moveo_backend.Adventure.Domain.Services;
using Moveo_backend.Adventure.Interfaces.REST.Resources;
using Moveo_backend.Adventure.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;

namespace Moveo_backend.Adventure.Interfaces.REST;

[ApiController]
[Route("api/v1/adventure-routes")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Adventure Route Endpoints")]
public class AdventureRoutesController(
    IAdventureRouteCommandService adventureRouteCommandService,
    IAdventureRouteQueryService adventureRouteQueryService,
    IRoutePassengerCommandService routePassengerCommandService,
    IRoutePassengerQueryService routePassengerQueryService) : ControllerBase
{
    [HttpGet("{routeId:int}")]
    [SwaggerOperation(
        Summary = "Get Adventure Route by Id",
        Description = "Get an adventure route by its unique identifier (incluye el array passengers de carpool)",
        OperationId = "GetAdventureRouteById"
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "The adventure route was found", typeof(AdventureRouteResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The adventure route was not found")]
    public async Task<IActionResult> GetAdventureRouteById([FromRoute] int routeId)
    {
        var adventureRoute = await adventureRouteQueryService.Handle(new GetAdventureRouteByIdQuery(routeId));
        if (adventureRoute is null) return NotFound();

        var resource = AdventureRouteResourceFromEntityAssembler.ToResourceFromEntity(adventureRoute);

        // US16 — embeber pasajeros (PENDING/CONFIRMED) en el detalle de la ruta.
        var passengers = await routePassengerQueryService.GetPassengersForRoute(routeId);
        resource = resource with
        {
            Passengers = passengers.Select(RoutePassengerResourceFromViewAssembler.ToResourceFromView).ToList()
        };

        return Ok(resource);
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Get All Adventure Routes",
        Description = "Get all adventure routes with optional filters",
        OperationId = "GetAllAdventureRoutes"
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "The list of adventure routes", typeof(IEnumerable<AdventureRouteResource>))]
    public async Task<IActionResult> GetAllAdventureRoutes(
        [FromQuery] int? ownerId = null,
        [FromQuery] string? type = null,
        [FromQuery] string? difficulty = null,
        [FromQuery] bool? featured = null,
        [FromQuery] bool? onlyWomen = null,
        [FromQuery] string? community = null)
    {
        IEnumerable<Domain.Model.Aggregate.AdventureRoute> routes;

        if (ownerId.HasValue)
        {
            routes = await adventureRouteQueryService.Handle(new GetAdventureRoutesByOwnerIdQuery(ownerId.Value));
        }
        else if (!string.IsNullOrEmpty(type))
        {
            routes = await adventureRouteQueryService.Handle(new GetAdventureRoutesByTypeQuery(type));
        }
        else if (!string.IsNullOrEmpty(difficulty))
        {
            routes = await adventureRouteQueryService.Handle(new GetAdventureRoutesByDifficultyQuery(difficulty));
        }
        else if (featured == true)
        {
            routes = await adventureRouteQueryService.Handle(new GetFeaturedAdventureRoutesQuery());
        }
        else
        {
            routes = await adventureRouteQueryService.Handle(new GetAllAdventureRoutesQuery());
        }

        // Filtro de carpool: solo mujeres
        if (onlyWomen == true)
        {
            routes = routes.Where(r => r.OnlyWomen);
        }

        // Filtro de carpool: comunidad/grupo
        if (!string.IsNullOrWhiteSpace(community))
        {
            routes = routes.Where(r =>
                string.Equals(r.Community, community, StringComparison.OrdinalIgnoreCase));
        }

        var resources = routes.Select(AdventureRouteResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpPost("{routeId:int}/book")]
    [SwaggerOperation(
        Summary = "Request a carpool seat (US16)",
        Description = "Crea una solicitud de asiento en estado PENDING (retiene cupo). El descuento definitivo " +
                      "ocurre al aceptar (accept). Requiere passengerId; el flujo legacy sin passengerId fue eliminado " +
                      "porque corrompía el aforo (descontaba asientos sin registrar al pasajero).",
        OperationId = "BookAdventureRouteSeat"
    )]
    [SwaggerResponse(StatusCodes.Status201Created, "Solicitud de asiento creada (PENDING)", typeof(RoutePassengerResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Falta passengerId")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "No elegible (ruta solo mujeres / comunidad restringida)")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Ruta no encontrada")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Sin cupo / solicitud duplicada / ruta no activa")]
    public async Task<IActionResult> BookSeat([FromRoute] int routeId, [FromBody] BookSeatResource resource)
    {
        // A partir de US16 el passengerId es obligatorio: sin él no se puede aprobar ni mostrar
        // al pasajero, y el flujo antiguo descontaba cupo dejando passengers vacío (bug reportado).
        if (resource is not { PassengerId: > 0 })
            return BadRequest(new
            {
                error = "passenger_id_required",
                message = "passengerId es obligatorio para reservar un asiento."
            });

        var seats = resource.Seats < 1 ? 1 : resource.Seats;

        try
        {
            var view = await routePassengerCommandService.Handle(
                new RequestRouteSeatCommand(routeId, resource.PassengerId, seats));
            return StatusCode(StatusCodes.Status201Created,
                RoutePassengerResourceFromViewAssembler.ToResourceFromView(view));
        }
        catch (CarpoolException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Code, message = ex.Message });
        }
    }

    [HttpGet("{routeId:int}/passengers")]
    [SwaggerOperation(
        Summary = "List route passengers (US16)",
        Description = "Lista las solicitudes PENDING/CONFIRMED de una ruta. Requiere ownerId (dueño de la ruta).",
        OperationId = "GetRoutePassengers"
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Lista de pasajeros", typeof(RoutePassengersResponse))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "El ownerId no es dueño de la ruta")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Ruta no encontrada")]
    public async Task<IActionResult> GetRoutePassengers([FromRoute] int routeId, [FromQuery] int ownerId)
    {
        try
        {
            var (route, passengers) = await routePassengerQueryService.Handle(
                new GetRoutePassengersQuery(routeId, ownerId));
            var response = new RoutePassengersResponse(
                route.Id,
                route.SeatsTotal,
                route.SeatsAvailable,
                passengers.Select(RoutePassengerResourceFromViewAssembler.ToResourceFromView).ToList());
            return Ok(response);
        }
        catch (CarpoolException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Code, message = ex.Message });
        }
    }

    [HttpPost("{routeId:int}/passengers/{passengerId:int}/accept")]
    [SwaggerOperation(
        Summary = "Accept a passenger request (US16)",
        Description = "Pasa la solicitud a CONFIRMED y descuenta el cupo. Requiere ownerId.",
        OperationId = "AcceptRoutePassenger"
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Solicitud aceptada", typeof(RoutePassengerResource))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "El ownerId no es dueño de la ruta")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Ruta o solicitud no encontrada")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Sin cupo disponible")]
    public async Task<IActionResult> AcceptPassenger([FromRoute] int routeId, [FromRoute] int passengerId, [FromQuery] int ownerId)
    {
        try
        {
            var view = await routePassengerCommandService.Handle(
                new AcceptRoutePassengerCommand(routeId, passengerId, ownerId));
            return Ok(RoutePassengerResourceFromViewAssembler.ToResourceFromView(view));
        }
        catch (CarpoolException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Code, message = ex.Message });
        }
    }

    [HttpPost("{routeId:int}/passengers/{passengerId:int}/reject")]
    [SwaggerOperation(
        Summary = "Reject a passenger request (US16)",
        Description = "Pasa la solicitud a REJECTED y libera el cupo tentativo. Requiere ownerId.",
        OperationId = "RejectRoutePassenger"
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Solicitud rechazada", typeof(RoutePassengerResource))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "El ownerId no es dueño de la ruta")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Ruta o solicitud no encontrada")]
    public async Task<IActionResult> RejectPassenger([FromRoute] int routeId, [FromRoute] int passengerId, [FromQuery] int ownerId)
    {
        try
        {
            var view = await routePassengerCommandService.Handle(
                new RejectRoutePassengerCommand(routeId, passengerId, ownerId));
            return Ok(RoutePassengerResourceFromViewAssembler.ToResourceFromView(view));
        }
        catch (CarpoolException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Code, message = ex.Message });
        }
    }

    [HttpDelete("{routeId:int}/passengers/{passengerId:int}")]
    [SwaggerOperation(
        Summary = "Remove a confirmed passenger (US16)",
        Description = "Pasa de CONFIRMED a CANCELLED y libera el cupo. Requiere ownerId.",
        OperationId = "RemoveRoutePassenger"
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Pasajero quitado", typeof(RoutePassengerResource))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "El ownerId no es dueño de la ruta")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Ruta o solicitud no encontrada")]
    public async Task<IActionResult> RemovePassenger([FromRoute] int routeId, [FromRoute] int passengerId, [FromQuery] int ownerId)
    {
        try
        {
            var view = await routePassengerCommandService.Handle(
                new RemoveRoutePassengerCommand(routeId, passengerId, ownerId));
            return Ok(RoutePassengerResourceFromViewAssembler.ToResourceFromView(view));
        }
        catch (CarpoolException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Code, message = ex.Message });
        }
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Create Adventure Route",
        Description = "Create a new adventure route",
        OperationId = "CreateAdventureRoute"
    )]
    [SwaggerResponse(StatusCodes.Status201Created, "The adventure route was created", typeof(AdventureRouteResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "The adventure route could not be created")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "El owner no tiene correo institucional (rutas de carpool)")]
    public async Task<IActionResult> CreateAdventureRoute([FromBody] CreateAdventureRouteResource resource)
    {
        var command = CreateAdventureRouteCommandFromResourceAssembler.ToCommandFromResource(resource);
        try
        {
            var adventureRoute = await adventureRouteCommandService.Handle(command);
            if (adventureRoute is null) return BadRequest();
            var adventureRouteResource = AdventureRouteResourceFromEntityAssembler.ToResourceFromEntity(adventureRoute);
            return CreatedAtAction(nameof(GetAdventureRouteById), new { routeId = adventureRoute.Id }, adventureRouteResource);
        }
        catch (CarpoolException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Code, message = ex.Message });
        }
    }

    [HttpPut("{routeId:int}")]
    [SwaggerOperation(
        Summary = "Update Adventure Route",
        Description = "Update an existing adventure route",
        OperationId = "UpdateAdventureRoute"
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "The adventure route was updated", typeof(AdventureRouteResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The adventure route was not found")]
    public async Task<IActionResult> UpdateAdventureRoute([FromRoute] int routeId, [FromBody] UpdateAdventureRouteResource resource)
    {
        var command = UpdateAdventureRouteCommandFromResourceAssembler.ToCommandFromResource(routeId, resource);
        var adventureRoute = await adventureRouteCommandService.Handle(command);
        if (adventureRoute is null) return NotFound();
        var adventureRouteResource = AdventureRouteResourceFromEntityAssembler.ToResourceFromEntity(adventureRoute);
        return Ok(adventureRouteResource);
    }

    [HttpPost("{routeId:int}/start")]
    [SwaggerOperation(
        Summary = "Start route",
        Description = "Inicia la ruta (active/full -> in_progress). Requiere ownerId.",
        OperationId = "StartAdventureRoute")]
    [SwaggerResponse(StatusCodes.Status200OK, "Ruta iniciada", typeof(AdventureRouteResource))]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Transición ilegal")]
    public async Task<IActionResult> StartRoute([FromRoute] int routeId, [FromQuery] int ownerId) =>
        await RunTransition(() => adventureRouteCommandService.Handle(new StartAdventureRouteCommand(routeId, ownerId)));

    [HttpPost("{routeId:int}/complete")]
    [SwaggerOperation(
        Summary = "Complete route",
        Description = "Completa la ruta (in_progress -> completed). Requiere ownerId.",
        OperationId = "CompleteAdventureRoute")]
    [SwaggerResponse(StatusCodes.Status200OK, "Ruta completada", typeof(AdventureRouteResource))]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Transición ilegal")]
    public async Task<IActionResult> CompleteRoute([FromRoute] int routeId, [FromQuery] int ownerId) =>
        await RunTransition(() => adventureRouteCommandService.Handle(new CompleteAdventureRouteCommand(routeId, ownerId)));

    [HttpPost("{routeId:int}/cancel")]
    [SwaggerOperation(
        Summary = "Cancel route",
        Description = "Cancela la ruta, libera cupos y notifica a los pasajeros. Requiere ownerId.",
        OperationId = "CancelAdventureRoute")]
    [SwaggerResponse(StatusCodes.Status200OK, "Ruta cancelada", typeof(AdventureRouteResource))]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Transición ilegal")]
    public async Task<IActionResult> CancelRoute([FromRoute] int routeId, [FromQuery] int ownerId) =>
        await RunTransition(() => adventureRouteCommandService.Handle(new CancelAdventureRouteCommand(routeId, ownerId)));

    // Ejecuta una transición de estado y traduce las excepciones a respuestas HTTP.
    private async Task<IActionResult> RunTransition(Func<Task<Domain.Model.Aggregate.AdventureRoute>> action)
    {
        try
        {
            var route = await action();
            return Ok(AdventureRouteResourceFromEntityAssembler.ToResourceFromEntity(route));
        }
        catch (CarpoolException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Code, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = "illegal_transition", message = ex.Message });
        }
    }

    [HttpDelete("{routeId:int}")]
    [SwaggerOperation(
        Summary = "Delete Adventure Route",
        Description = "Delete an adventure route by its Id",
        OperationId = "DeleteAdventureRoute"
    )]
    [SwaggerResponse(StatusCodes.Status204NoContent, "The adventure route was deleted")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The adventure route was not found")]
    public async Task<IActionResult> DeleteAdventureRoute([FromRoute] int routeId)
    {
        var command = new DeleteAdventureRouteCommand(routeId);
        var deleted = await adventureRouteCommandService.Handle(command);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
