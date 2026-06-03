using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Moveo_backend.UserReview.Domain.Model.Commands;
using Moveo_backend.UserReview.Domain.Model.Queries;
using Moveo_backend.UserReview.Domain.Services;
using Moveo_backend.UserReview.Interfaces.REST.Resources;
using Moveo_backend.UserReview.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;

namespace Moveo_backend.UserReview.Interfaces.REST;

[ApiController]
[Route("api/v1/user-reviews")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("User Reviews - Ratings between users (owner ↔ renter)")]
public class UserReviewsController(
    IUserReviewCommandService userReviewCommandService,
    IUserReviewQueryService userReviewQueryService) : ControllerBase
{
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
        IEnumerable<Domain.Model.Aggregate.UserReview> reviews;

        if (reviewedUserId.HasValue)
        {
            var query = new GetUserReviewsByReviewedUserIdQuery(reviewedUserId.Value, type);
            reviews = await userReviewQueryService.Handle(query);
        }
        else if (reviewerId.HasValue)
        {
            var query = new GetUserReviewsByReviewerIdQuery(reviewerId.Value);
            reviews = await userReviewQueryService.Handle(query);
        }
        else if (rentalId.HasValue)
        {
            var query = new GetUserReviewsByRentalIdQuery(rentalId.Value);
            reviews = await userReviewQueryService.Handle(query);
        }
        else
        {
            var query = new GetAllUserReviewsQuery();
            reviews = await userReviewQueryService.Handle(query);
        }

        var resources = reviews.Select(UserReviewResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
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
        var review = await userReviewQueryService.Handle(query);
        if (review is null) return NotFound();
        var resource = UserReviewResourceFromEntityAssembler.ToResourceFromEntity(review);
        return Ok(resource);
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
        var review = await userReviewCommandService.Handle(command);
        if (review is null) return BadRequest();
        var reviewResource = UserReviewResourceFromEntityAssembler.ToResourceFromEntity(review);
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
        var review = await userReviewCommandService.Handle(command);
        if (review is null) return NotFound();
        var reviewResource = UserReviewResourceFromEntityAssembler.ToResourceFromEntity(review);
        return Ok(reviewResource);
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
        var deleted = await userReviewCommandService.Handle(command);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
