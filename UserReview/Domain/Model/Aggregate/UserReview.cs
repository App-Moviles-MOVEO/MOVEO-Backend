using Moveo_backend.UserReview.Domain.Model.Commands;

namespace Moveo_backend.UserReview.Domain.Model.Aggregate;

/// <summary>
/// UserReview Aggregate Root - Represents a review from one user to another
/// </summary>
public class UserReview
{
    public int Id { get; private set; }
    public int ReviewerId { get; private set; }
    public int ReviewedUserId { get; private set; }
    public int RentalId { get; private set; }
    public int Rating { get; private set; } // 1-5
    public string Comment { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty; // owner_to_renter | renter_to_owner
    public DateTime CreatedAt { get; private set; }

    // US41 — disputa de reseña. "active"/"disputed" cuentan para la reputación; "excluded" no.
    public string Status { get; private set; } = "active"; // active | disputed | excluded
    public string? DisputeReason { get; private set; }
    public DateTime? DisputedAt { get; private set; }

    public bool CountsForReputation => Status != "excluded";

    protected UserReview() { }

    public UserReview(CreateUserReviewCommand command)
    {
        ReviewerId = command.ReviewerId;
        ReviewedUserId = command.ReviewedUserId;
        RentalId = command.RentalId;
        Rating = command.Rating;
        Comment = command.Comment ?? string.Empty;
        Type = command.Type;
        CreatedAt = DateTime.UtcNow;
        Status = "active";
    }

    public void Update(int? rating, string? comment)
    {
        if (rating.HasValue && rating.Value >= 1 && rating.Value <= 5)
            Rating = rating.Value;
        if (comment != null)
            Comment = comment;
    }

    public void MarkDisputed(string? reason)
    {
        Status = "disputed";
        DisputeReason = reason;
        DisputedAt = DateTime.UtcNow;
    }

    public void MarkExcluded(string? reason)
    {
        Status = "excluded";
        DisputeReason = reason;
        DisputedAt = DateTime.UtcNow;
    }
}
