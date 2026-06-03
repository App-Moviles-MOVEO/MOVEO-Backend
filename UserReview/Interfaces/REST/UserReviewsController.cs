using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moveo_backend.UserReview.Domain.Model.Commands;
using Moveo_backend.UserReview.Domain.Model.Queries;
using Moveo_backend.UserReview.Domain.Services;
using Moveo_backend.UserReview.Interfaces.REST.Resources;
using Moveo_backend.UserReview.Interfaces.REST.Transform;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Swashbuckle.AspNetCore.Annotations;
using UserReviewEntity = Moveo_backend.UserReview.Domain.Model.Aggregate.UserReview;

namespace Moveo_backend.UserReview.Interfaces.REST;

[ApiController]
[Route("api/v1/user-reviews")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("User Reviews - Ratings between users (owner ↔ renter)")]
public class UserReviewsController : ControllerBase
{
    private readonly IUserReviewCommandService _userReviewCommandService;
    private readonly IUserReviewQueryService _userReviewQueryService;
    private readonly AppDbContext _context;

    public UserReviewsController(
        IUserReviewCommandService userReviewCommandService,
        IUserReviewQueryService userReviewQueryService,
        AppDbContext context)
    {
        _userReviewCommandService = userReviewCommandService;
        _userReviewQueryService = userReviewQueryService;
        _context = context;
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all user reviews",
        Description = "Get all user reviews with optional filters",
        OperationId = "GetAllUserReviews"
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "List of user reviews", typeof(IEnumerable<UserReviewResource>))]
    public async Task<IActionResult> GetAllUserReviews(
        [FromQuery] int? reviewedUserId = null,
        [FromQuery] int? reviewerId = null,
        [FromQuery] int? rentalId = null,
        [FromQuery] string? type = null)
    {
        IEnumerable<UserReviewEntity> reviews;

        if (reviewedUserId.HasValue)
        {
            var query = new GetUserReviewsByReviewedUserIdQuery(reviewedUserId.Value, type);
            reviews = await _userReviewQueryService.Handle(query);
        }
        else if (reviewerId.HasValue)
        {
            var query = new GetUserReviewsByReviewerIdQuery(reviewerId.Value);
            reviews = await _userReviewQueryService.Handle(query);
        }
        else if (rentalId.HasValue)
        {
            var query = new GetUserReviewsByRentalIdQuery(rentalId.Value);
            reviews = await _userReviewQueryService.Handle(query);
        }
        else
        {
            var query = new GetAllUserReviewsQuery();
            reviews = await _userReviewQueryService.Handle(query);
        }

        return Ok(await MapManyAsync(reviews.ToList()));
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(
        Summary = "Get user review by ID",
        Description = "Get a specific user review by its ID",
        OperationId = "GetUserReviewById"
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "The user review", typeof(UserReviewResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User review not found")]
    public async Task<IActionResult> GetUserReviewById([FromRoute] int id)
    {
        var query = new GetUserReviewByIdQuery(id);
        var review = await _userReviewQueryService.Handle(query);
        if (review is null) return NotFound();
        return Ok(await MapOneAsync(review));
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Create user review",
        Description = "Create a new user review (owner_to_renter or renter_to_owner)",
        OperationId = "CreateUserReview"
    )]
    [SwaggerResponse(StatusCodes.Status201Created, "The user review was created", typeof(UserReviewResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
    public async Task<IActionResult> CreateUserReview([FromBody] CreateUserReviewResource resource)
    {
        var command = CreateUserReviewCommandFromResourceAssembler.ToCommandFromResource(resource);
        var review = await _userReviewCommandService.Handle(command);
        if (review is null) return BadRequest();
        var reviewResource = await MapOneAsync(review);
        return CreatedAtAction(nameof(GetUserReviewById), new { id = review.Id }, reviewResource);
    }

    [HttpPut("{id:int}")]
    [SwaggerOperation(
        Summary = "Update user review",
        Description = "Update an existing user review",
        OperationId = "UpdateUserReview"
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "The user review was updated", typeof(UserReviewResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User review not found")]
    public async Task<IActionResult> UpdateUserReview([FromRoute] int id, [FromBody] UpdateUserReviewResource resource)
    {
        var command = UpdateUserReviewCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var review = await _userReviewCommandService.Handle(command);
        if (review is null) return NotFound();
        return Ok(await MapOneAsync(review));
    }

    [HttpDelete("{id:int}")]
    [SwaggerOperation(
        Summary = "Delete user review",
        Description = "Delete a user review by ID",
        OperationId = "DeleteUserReview"
    )]
    [SwaggerResponse(StatusCodes.Status204NoContent, "The user review was deleted")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User review not found")]
    public async Task<IActionResult> DeleteUserReview([FromRoute] int id)
    {
        var command = new DeleteUserReviewCommand(id);
        var deleted = await _userReviewCommandService.Handle(command);
        if (!deleted) return NotFound();
        return NoContent();
    }

    // -------------------- Enriquecimiento (reviewerName) --------------------

    private async Task<UserReviewResource> MapOneAsync(UserReviewEntity review)
    {
        var name = await _context.Users
            .Where(u => u.Id == review.ReviewerId)
            .Select(u => u.FirstName + " " + u.LastName)
            .FirstOrDefaultAsync();
        return UserReviewResourceFromEntityAssembler.ToResourceFromEntity(review, name);
    }

    private async Task<List<UserReviewResource>> MapManyAsync(List<UserReviewEntity> reviews)
    {
        if (reviews.Count == 0) return new List<UserReviewResource>();

        var reviewerIds = reviews.Select(r => r.ReviewerId).Distinct().ToList();
        var names = await _context.Users
            .Where(u => reviewerIds.Contains(u.Id))
            .Select(u => new { u.Id, Name = u.FirstName + " " + u.LastName })
            .ToDictionaryAsync(x => x.Id, x => x.Name);

        return reviews.Select(r =>
        {
            names.TryGetValue(r.ReviewerId, out var name);
            return UserReviewResourceFromEntityAssembler.ToResourceFromEntity(r, name);
        }).ToList();
    }
}
