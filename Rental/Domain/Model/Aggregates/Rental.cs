namespace Moveo_backend.Rental.Domain.Model.Aggregates;

/// <summary>
///     Rental Aggregate Root
/// </summary>
public class Rental
{
    public int Id { get; private set; }
    public int VehicleId { get; private set; }
    public int RenterId { get; private set; }
    public int OwnerId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public decimal TotalPrice { get; private set; }
    public string Status { get; private set; } = "pending";
    public string? PickupLocation { get; private set; }
    public string? ReturnLocation { get; private set; }
    public string? Notes { get; private set; }
    public int? AdventureRouteId { get; private set; }
    public bool? VehicleRated { get; private set; }
    public int? VehicleRating { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    // Parameterless constructor for EF Core
    private Rental()
    {
        Status = "pending";
        CreatedAt = DateTime.UtcNow;
    }

    public Rental(
        int vehicleId,
        int renterId,
        int ownerId,
        DateTime startDate,
        DateTime endDate,
        decimal totalPrice,
        string? pickupLocation,
        string? returnLocation,
        string? notes,
        int? adventureRouteId = null)
    {
        VehicleId = vehicleId;
        RenterId = renterId;
        OwnerId = ownerId;
        StartDate = startDate;
        EndDate = endDate;
        TotalPrice = totalPrice;
        PickupLocation = pickupLocation;
        ReturnLocation = returnLocation;
        Notes = notes;
        AdventureRouteId = adventureRouteId;
        Status = "pending";
        VehicleRated = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(
        int vehicleId,
        int renterId,
        int ownerId,
        DateTime startDate,
        DateTime endDate,
        decimal totalPrice,
        string status,
        string? pickupLocation,
        string? returnLocation,
        string? notes,
        int? adventureRouteId,
        bool? vehicleRated,
        int? vehicleRating,
        DateTime? acceptedAt,
        DateTime? completedAt)
    {
        VehicleId = vehicleId;
        RenterId = renterId;
        OwnerId = ownerId;
        StartDate = startDate;
        EndDate = endDate;
        TotalPrice = totalPrice;
        Status = status;
        PickupLocation = pickupLocation;
        ReturnLocation = returnLocation;
        Notes = notes;
        AdventureRouteId = adventureRouteId;
        VehicleRated = vehicleRated;
        VehicleRating = vehicleRating;
        AcceptedAt = acceptedAt;
        CompletedAt = completedAt;
    }

    public void PartialUpdate(
        string? status = null,
        bool? vehicleRated = null,
        int? vehicleRating = null,
        DateTime? acceptedAt = null,
        DateTime? completedAt = null)
    {
        if (status != null) Status = status;
        if (vehicleRated.HasValue) VehicleRated = vehicleRated;
        if (vehicleRating.HasValue) VehicleRating = vehicleRating;
        if (acceptedAt.HasValue) AcceptedAt = acceptedAt;
        if (completedAt.HasValue) CompletedAt = completedAt;
    }

    public void Accept()
    {
        if (Status != "pending")
            throw new InvalidOperationException("Can only accept pending rentals");
        Status = "accepted";
        AcceptedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (Status != "accepted")
            throw new InvalidOperationException("Can only activate accepted rentals");
        Status = "active";
    }

    public void Complete()
    {
        if (Status != "active")
            throw new InvalidOperationException("Can only complete active rentals");
        Status = "completed";
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == "completed")
            throw new InvalidOperationException("Cannot cancel completed rentals");
        Status = "cancelled";
    }

    public void RateVehicle(int rating)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5");
        VehicleRated = true;
        VehicleRating = rating;
    }
}
