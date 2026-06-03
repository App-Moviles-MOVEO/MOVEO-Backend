namespace Moveo_backend.Rental.Domain.Model.Aggregates;

public class Review
{
    public int Id { get; private set; }
    public int RentalId { get; private set; }
    public int? VehicleId { get; private set; }
    public int ReviewerId { get; private set; }
    public int RevieweeId { get; private set; }
    public int Rating { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty; // "owner-to-renter", "renter-to-owner", "vehicle"
    public DateTime CreatedAt { get; private set; }
    
    // Navigation properties
    public Rental? Rental { get; private set; }
    
    protected Review() { }
    
    public Review(int rentalId, int? vehicleId, int reviewerId, int revieweeId, int rating, string comment, string type)
    {
        RentalId = rentalId;
        VehicleId = vehicleId;
        ReviewerId = reviewerId;
        RevieweeId = revieweeId;
        Rating = rating;
        Comment = comment ?? string.Empty;
        Type = type;
        CreatedAt = DateTime.UtcNow;
    }
    
    public void Update(int? rating, string? comment)
    {
        if (rating.HasValue) Rating = rating.Value;
        if (comment != null) Comment = comment;
    }
}
