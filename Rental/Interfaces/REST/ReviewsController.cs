using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moveo_backend.Rental.Domain.Model.Aggregates;
using Moveo_backend.Rental.Domain.Model.Commands;
using Moveo_backend.Rental.Domain.Model.Queries;
using Moveo_backend.Rental.Domain.Services;
using Moveo_backend.Rental.Interfaces.REST.Resources;
using Moveo_backend.Rental.Interfaces.REST.Transform;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace Moveo_backend.Rental.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewCommandService _reviewCommandService;
    private readonly IReviewQueryService _reviewQueryService;
    private readonly AppDbContext _context;

    public ReviewsController(
        IReviewCommandService reviewCommandService,
        IReviewQueryService reviewQueryService,
        AppDbContext context)
    {
        _reviewCommandService = reviewCommandService;
        _reviewQueryService = reviewQueryService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllReviews(
        [FromQuery] int? vehicleId = null,
        [FromQuery] int? rentalId = null,
        [FromQuery] int? reviewerId = null,
        [FromQuery] int? revieweeId = null)
    {
        IEnumerable<Review> reviews;

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

        return Ok(await MapManyAsync(reviews.ToList()));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetReviewById(int id)
    {
        var query = new GetReviewByIdQuery(id);
        var review = await _reviewQueryService.Handle(query);
        if (review == null) return NotFound();
        return Ok(await MapOneAsync(review));
    }

    [HttpGet("rental/{rentalId:int}")]
    public async Task<IActionResult> GetReviewsByRentalId(int rentalId)
    {
        var query = new GetReviewsByRentalIdQuery(rentalId);
        var reviews = await _reviewQueryService.Handle(query);
        return Ok(await MapManyAsync(reviews.ToList()));
    }

    [HttpGet("reviewer/{reviewerId:int}")]
    public async Task<IActionResult> GetReviewsByReviewerId(int reviewerId)
    {
        var query = new GetReviewsByReviewerIdQuery(reviewerId);
        var reviews = await _reviewQueryService.Handle(query);
        return Ok(await MapManyAsync(reviews.ToList()));
    }

    [HttpGet("reviewee/{revieweeId:int}")]
    public async Task<IActionResult> GetReviewsByRevieweeId(int revieweeId)
    {
        var query = new GetReviewsByRevieweeIdQuery(revieweeId);
        var reviews = await _reviewQueryService.Handle(query);
        return Ok(await MapManyAsync(reviews.ToList()));
    }

    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewResource resource)
    {
        var command = CreateReviewCommandFromResourceAssembler.ToCommandFromResource(resource);
        var review = await _reviewCommandService.Handle(command);
        if (review == null) return BadRequest();
        var reviewResource = await MapOneAsync(review);
        return CreatedAtAction(nameof(GetReviewById), new { id = review.Id }, reviewResource);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewResource resource)
    {
        var command = UpdateReviewCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var review = await _reviewCommandService.Handle(command);
        if (review == null) return NotFound();
        return Ok(await MapOneAsync(review));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var command = new DeleteReviewCommand(id);
        var result = await _reviewCommandService.Handle(command);
        if (!result) return NotFound();
        return NoContent();
    }

    // -------------------- Enriquecimiento (reviewerName) --------------------

    private async Task<ReviewResource> MapOneAsync(Review review)
    {
        var name = await _context.Users
            .Where(u => u.Id == review.ReviewerId)
            .Select(u => u.FirstName + " " + u.LastName)
            .FirstOrDefaultAsync();
        return ReviewResourceFromEntityAssembler.ToResourceFromEntity(review, name);
    }

    private async Task<List<ReviewResource>> MapManyAsync(List<Review> reviews)
    {
        if (reviews.Count == 0) return new List<ReviewResource>();

        var reviewerIds = reviews.Select(r => r.ReviewerId).Distinct().ToList();
        var names = await _context.Users
            .Where(u => reviewerIds.Contains(u.Id))
            .Select(u => new { u.Id, Name = u.FirstName + " " + u.LastName })
            .ToDictionaryAsync(x => x.Id, x => x.Name);

        return reviews.Select(r =>
        {
            names.TryGetValue(r.ReviewerId, out var name);
            return ReviewResourceFromEntityAssembler.ToResourceFromEntity(r, name);
        }).ToList();
    }
}
