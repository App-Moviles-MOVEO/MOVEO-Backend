using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Moveo_backend.UserManagement.Application.CommandServices;
using Moveo_backend.UserManagement.Application.QueryServices;
using Moveo_backend.UserManagement.Domain.Model.Commands;
using Moveo_backend.UserManagement.Domain.Model.Queries;
using Moveo_backend.UserManagement.Domain.Services;
using Moveo_backend.UserManagement.Interfaces.REST.Resources;
using Moveo_backend.UserManagement.Interfaces.REST.Transform;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace Moveo_backend.UserManagement.Interfaces.REST;

[ApiController]
[Route("api/v1/users")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available User Endpoints")]
public class UsersController(
    IUserCommandService userCommandService,
    IUserQueryService userQueryService,
    AppDbContext context) : ControllerBase
{
    [HttpGet("{userId:int}")]
    [SwaggerOperation(
        Summary = "Get User By Id",
        Description = "Retrieve a single user by its unique identifier",
        OperationId = "GetUserById"
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "The user was found", typeof(UserResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
    public async Task<IActionResult> GetUserById(int userId)
    {
        var getUserByIdQuery = new GetUserByIdQuery(userId);
        var user = await userQueryService.Handle(getUserByIdQuery);
        if (user is null) return NotFound();
        var resource = UserResourceFromEntityAssembler.ToResourceFromEntity(user);
        await EnrichStatsAsync(resource);
        return Ok(resource);
    }

    // US36 — calcula reputación, tasa de puntualidad y badges server-side.
    private async Task EnrichStatsAsync(UserResource resource)
    {
        var userId = resource.Id;

        var ratings = await context.UserReviews
            .Where(r => r.ReviewedUserId == userId)
            .Select(r => r.Rating)
            .ToListAsync();
        var reputation = ratings.Count > 0 ? Math.Round(ratings.Average(x => (double)x), 2) : 0;

        // Puntualidad: alquileres completados que se cerraron dentro de la fecha pactada.
        var completed = await context.Rentals
            .Where(r => r.RenterId == userId && r.Status == "completed" && r.CompletedAt != null)
            .Select(r => new { r.CompletedAt, r.EndDate })
            .ToListAsync();
        var onTimeRate = completed.Count > 0
            ? Math.Round((double)completed.Count(r => r.CompletedAt <= r.EndDate.AddHours(2)) / completed.Count, 2)
            : 0;

        resource.Stats.Reputation = reputation;
        resource.Stats.OnTimeRate = onTimeRate;
        resource.Stats.Badges = BuildBadges(resource, reputation, onTimeRate, ratings.Count, completed.Count);
    }

    private static List<string> BuildBadges(
        UserResource resource, double reputation, double onTimeRate, int reviewsCount, int completedCount)
    {
        var badges = new List<string>();
        if (resource.KycStatus == "approved" || resource.Verified.Dni)
            badges.Add("VERIFIED");
        if (completedCount >= 3 && onTimeRate >= 0.9)
            badges.Add("PUNCTUAL");
        if (resource.Stats.CompletedRentals >= 10)
            badges.Add("TOP_RENTER");
        if (reviewsCount >= 3 && reputation >= 4.8)
            badges.Add("FIVE_STARS");
        return badges;
    }
    
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get All Users or Search by Email",
        Description = "Retrieve a list of all registered users or search by email",
        OperationId = "GetAllUsers"
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "The list of users", typeof(IEnumerable<UserResource>))]
    public async Task<IActionResult> GetAllUsers([FromQuery] string? email = null)
    {
        if (!string.IsNullOrEmpty(email))
        {
            var getUserByEmailQuery = new GetUserByEmailQuery(email);
            var user = await userQueryService.Handle(getUserByEmailQuery);
            if (user is null) return Ok(Array.Empty<UserResource>());
            var resource = UserResourceFromEntityAssembler.ToResourceFromEntity(user);
            return Ok(new[] { resource });
        }
        
        var getAllUsersQuery = new GetAllUsersQuery();
        var users = await userQueryService.Handle(getAllUsersQuery);
        var resources = users.Select(UserResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }
    
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new User",
        Description = "Register a new user in the system",
        OperationId = "CreateUser"
    )]
    [SwaggerResponse(StatusCodes.Status201Created, "The user was successfully created", typeof(UserResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "The user could not be created")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserResource resource)
    {
        var createUserCommand = CreateUserCommandFromResourceAssembler.ToCommandFromResource(resource);
        var user = await userCommandService.HandleCreate(createUserCommand);
        if (user is null) return BadRequest();
        var userResource = UserResourceFromEntityAssembler.ToResourceFromEntity(user);
        return CreatedAtAction(nameof(GetUserById), new { userId = user.Id }, userResource);
    }
    
    [HttpPut("{userId:int}")]
    [SwaggerOperation(
        Summary = "Update a User",
        Description = "Update an existing user's information",
        OperationId = "UpdateUser"
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "The user was successfully updated", typeof(UserResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
    public async Task<IActionResult> UpdateUser(int userId, [FromBody] UpdateUserResource resource)
    {
        var updateUserCommand = UpdateUserCommandFromResourceAssembler.ToCommandFromResource(resource, userId);
        var updatedUser = await userCommandService.HandleUpdate(updateUserCommand);
        if (updatedUser is null) return NotFound();
        var userResource = UserResourceFromEntityAssembler.ToResourceFromEntity(updatedUser);
        return Ok(userResource);
    }
    
    [HttpPatch("{userId:int}")]
    [SwaggerOperation(
        Summary = "Partially Update a User",
        Description = "Partially update an existing user's information",
        OperationId = "PatchUser"
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "The user was successfully updated", typeof(UserResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
    public async Task<IActionResult> PatchUser(int userId, [FromBody] UpdateUserResource resource)
    {
        var updateUserCommand = UpdateUserCommandFromResourceAssembler.ToCommandFromResource(resource, userId);
        var updatedUser = await userCommandService.HandleUpdate(updateUserCommand);
        if (updatedUser is null) return NotFound();
        var userResource = UserResourceFromEntityAssembler.ToResourceFromEntity(updatedUser);
        return Ok(userResource);
    }
    
    [HttpDelete("{userId:int}")]
    [SwaggerOperation(
        Summary = "Delete a User",
        Description = "Remove an existing user from the system",
        OperationId = "DeleteUser"
    )]
    [SwaggerResponse(StatusCodes.Status204NoContent, "The user was deleted successfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        var deleteUserCommand = new DeleteUserCommand(userId);
        await userCommandService.HandleDelete(deleteUserCommand);
        return NoContent();
    }
}