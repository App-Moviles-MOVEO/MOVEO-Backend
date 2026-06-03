using Microsoft.AspNetCore.Mvc;
using Moveo_backend.Rental.Domain.Model.Commands;
using Moveo_backend.Rental.Domain.Model.Queries;
using Moveo_backend.Rental.Domain.Services;
using Moveo_backend.Rental.Interfaces.REST.Resources;
using Moveo_backend.Rental.Interfaces.REST.Transform;

namespace Moveo_backend.Rental.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewCommandService _reviewCommandService;
    private readonly IReviewQueryService _reviewQueryService;

    public ReviewsController(
        IReviewCommandService reviewCommandService,
        IReviewQueryService reviewQueryService)
    {
        _reviewCommandService = reviewCommandService;
        _reviewQueryService = reviewQueryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllReviews(
        [FromQuery] int? vehicleId = null,
        [FromQuery] int? rentalId = null,
        [FromQuery] int? reviewerId = null,
        [FromQuery] int? revieweeId = null)
    {
        IEnumerable<Domain.Model.Aggregates.Review> reviews;
        
        if (rentalId.HasValue)
        {
            var query = new GetReviewsByRentalIdQuery(rentalId.Value);
            reviews = await _reviewQueryService.Handle(query);
        }
        else if (reviewerId.HasValue)
        {
            var query = new GetReviewsByReviewerIdQuery(reviewerId.Value);
            reviews = await _reviewQueryService.Handle(query);
        }
        else if (revieweeId.HasValue)
        {
            var query = new GetReviewsByRevieweeIdQuery(revieweeId.Value);
            reviews = await _reviewQueryService.Handle(query);
        }
        else
        {
            var query = new GetAllReviewsQuery();
            reviews = await _reviewQueryService.Handle(query);
        }
        
        // Filter by vehicleId if provided
        if (vehicleId.HasValue)
        {
            reviews = reviews.Where(r => r.VehicleId == vehicleId.Value);
        }
        
        var resources = reviews.Select(ReviewResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetReviewById(int id)
    {
        var query = new GetReviewByIdQuery(id);
        var review = await _reviewQueryService.Handle(query);
        if (review == null) return NotFound();
        var resource = ReviewResourceFromEntityAssembler.ToResourceFromEntity(review);
        return Ok(resource);
    }

    [HttpGet("rental/{rentalId:int}")]
    public async Task<IActionResult> GetReviewsByRentalId(int rentalId)
    {
        var query = new GetReviewsByRentalIdQuery(rentalId);
        var reviews = await _reviewQueryService.Handle(query);
        var resources = reviews.Select(ReviewResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("reviewer/{reviewerId:int}")]
    public async Task<IActionResult> GetReviewsByReviewerId(int reviewerId)
    {
        var query = new GetReviewsByReviewerIdQuery(reviewerId);
        var reviews = await _reviewQueryService.Handle(query);
        var resources = reviews.Select(ReviewResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("reviewee/{revieweeId:int}")]
    public async Task<IActionResult> GetReviewsByRevieweeId(int revieweeId)
    {
        var query = new GetReviewsByRevieweeIdQuery(revieweeId);
        var reviews = await _reviewQueryService.Handle(query);
        var resources = reviews.Select(ReviewResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewResource resource)
    {
        var command = CreateReviewCommandFromResourceAssembler.ToCommandFromResource(resource);
        var review = await _reviewCommandService.Handle(command);
        if (review == null) return BadRequest();
        var reviewResource = ReviewResourceFromEntityAssembler.ToResourceFromEntity(review);
        return CreatedAtAction(nameof(GetReviewById), new { id = review.Id }, reviewResource);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewResource resource)
    {
        var command = UpdateReviewCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var review = await _reviewCommandService.Handle(command);
        if (review == null) return NotFound();
        var reviewResource = ReviewResourceFromEntityAssembler.ToResourceFromEntity(review);
        return Ok(reviewResource);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var command = new DeleteReviewCommand(id);
        var result = await _reviewCommandService.Handle(command);
        if (!result) return NotFound();
        return NoContent();
    }
}
