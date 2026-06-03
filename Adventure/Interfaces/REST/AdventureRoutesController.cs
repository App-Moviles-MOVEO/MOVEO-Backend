using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
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
    IAdventureRouteQueryService adventureRouteQueryService) : ControllerBase
{
    [HttpGet("{routeId:int}")]
    [SwaggerOperation(
        Summary = "Get Adventure Route by Id",
        Description = "Get an adventure route by its unique identifier",
        OperationId = "GetAdventureRouteById"
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "The adventure route was found", typeof(AdventureRouteResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The adventure route was not found")]
    public async Task<IActionResult> GetAdventureRouteById([FromRoute] int routeId)
    {
        var adventureRoute = await adventureRouteQueryService.Handle(new GetAdventureRouteByIdQuery(routeId));
        if (adventureRoute is null) return NotFound();
        var resource = AdventureRouteResourceFromEntityAssembler.ToResourceFromEntity(adventureRoute);
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
        [FromQuery] bool? featured = null)
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

        var resources = routes.Select(AdventureRouteResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Create Adventure Route",
        Description = "Create a new adventure route",
        OperationId = "CreateAdventureRoute"
    )]
    [SwaggerResponse(StatusCodes.Status201Created, "The adventure route was created", typeof(AdventureRouteResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "The adventure route could not be created")]
    public async Task<IActionResult> CreateAdventureRoute([FromBody] CreateAdventureRouteResource resource)
    {
        var command = CreateAdventureRouteCommandFromResourceAssembler.ToCommandFromResource(resource);
        var adventureRoute = await adventureRouteCommandService.Handle(command);
        if (adventureRoute is null) return BadRequest();
        var adventureRouteResource = AdventureRouteResourceFromEntityAssembler.ToResourceFromEntity(adventureRoute);
        return CreatedAtAction(nameof(GetAdventureRouteById), new { routeId = adventureRoute.Id }, adventureRouteResource);
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
