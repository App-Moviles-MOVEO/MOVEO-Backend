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

    /// <summary>
    /// Disputa una reseña (US41). Mediación automática: si es un voto bajo (≤2), atípico respecto
    /// al promedio del usuario (diferencia ≥1.5) y sin justificación, se excluye de la reputación;
    /// en caso contrario queda marcada como "disputed" (sigue contando).
    /// </summary>
    [HttpPost("{id:int}/dispute")]
    [SwaggerOperation(Summary = "Dispute a user review", OperationId = "DisputeUserReview")]
    [SwaggerResponse(StatusCodes.Status200OK, "Disputa resuelta automáticamente")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Reseña no encontrada")]
    public async Task<IActionResult> DisputeUserReview([FromRoute] int id, [FromBody] DisputeReviewResource? resource)
    {
        var review = await _context.UserReviews.FirstOrDefaultAsync(r => r.Id == id);
        if (review is null) return NotFound();

        // Promedio de las demás reseñas del mismo usuario (excluyendo esta y las ya excluidas).
        var others = await _context.UserReviews
            .Where(r => r.ReviewedUserId == review.ReviewedUserId && r.Id != id && r.Status != "excluded")
            .Select(r => r.Rating)
            .ToListAsync();
        var avgOthers = others.Count > 0 ? others.Average() : review.Rating;

        var isLow = review.Rating <= 2;
        var isOutlier = (avgOthers - review.Rating) >= 1.5;
        var hasNoJustification = string.IsNullOrWhiteSpace(review.Comment) || review.Comment.Trim().Length < 10;

        var excluded = isLow && isOutlier && hasNoJustification;
        if (excluded) review.MarkExcluded(resource?.Reason);
        else review.MarkDisputed(resource?.Reason);

        await _context.SaveChangesAsync();

        // Reputación ajustada tras la resolución.
        var adjusted = await _context.UserReviews
            .Where(r => r.ReviewedUserId == review.ReviewedUserId && r.Status != "excluded")
            .Select(r => r.Rating)
            .ToListAsync();
        var adjustedReputation = adjusted.Count > 0 ? Math.Round(adjusted.Average(x => (double)x), 2) : 0;

        return Ok(new
        {
            id = review.Id,
            status = review.Status,
            outcome = excluded ? "excluded" : "kept",
            adjustedReputation
        });
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

public record DisputeReviewResource(string? Reason = null);
