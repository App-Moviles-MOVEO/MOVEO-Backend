using System.Text.Json;
using Moveo_backend.Adventure.Domain.Model.Commands;

namespace Moveo_backend.Adventure.Domain.Model.Aggregate;

/// <summary>
///     Adventure Route Aggregate Root
/// </summary>
public class AdventureRoute
{
    public int Id { get; }
    public int OwnerId { get; private set; }
    public string Name { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string StartLocation { get; private set; }
    public string EndLocation { get; private set; }
    public string Type { get; private set; } // "beach" | "mountain" | "city" | "desert" | "jungle"
    public int Duration { get; private set; } // En horas
    public string Difficulty { get; private set; } // "easy" | "moderate" | "hard"
    public decimal EstimatedCost { get; private set; }
    public string? VehicleName { get; private set; }
    public string? ImageUrl { get; private set; }
    public string TagsJson { get; private set; } = "[]";
    public bool Featured { get; private set; }
    public int? MaxCapacity { get; private set; }
    public double Rating { get; private set; }
    public int ReviewsCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public List<string> Tags
    {
        get => JsonSerializer.Deserialize<List<string>>(TagsJson) ?? new List<string>();
        private set => TagsJson = JsonSerializer.Serialize(value);
    }

    // Constructor vacío para EF Core
    public AdventureRoute()
    {
        Name = string.Empty;
        Title = string.Empty;
        Description = string.Empty;
        StartLocation = string.Empty;
        EndLocation = string.Empty;
        Type = string.Empty;
        Difficulty = string.Empty;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public AdventureRoute(
        int ownerId,
        string name,
        string title,
        string description,
        string startLocation,
        string endLocation,
        string type,
        int duration,
        string difficulty,
        decimal estimatedCost,
        string? vehicleName,
        string? imageUrl,
        List<string>? tags,
        bool featured,
        int? maxCapacity) : this()
    {
        OwnerId = ownerId;
        Name = name;
        Title = title;
        Description = description;
        StartLocation = startLocation;
        EndLocation = endLocation;
        Type = type;
        Duration = duration;
        Difficulty = difficulty;
        EstimatedCost = estimatedCost;
        VehicleName = vehicleName;
        ImageUrl = imageUrl;
        Tags = tags ?? new List<string>();
        Featured = featured;
        MaxCapacity = maxCapacity;
        Rating = 0;
        ReviewsCount = 0;
    }

    public AdventureRoute(CreateAdventureRouteCommand command) : this()
    {
        OwnerId = command.OwnerId;
        Name = command.Name;
        Title = command.Title;
        Description = command.Description;
        StartLocation = command.StartLocation;
        EndLocation = command.EndLocation;
        Type = command.Type;
        Duration = command.Duration;
        Difficulty = command.Difficulty;
        EstimatedCost = command.EstimatedCost;
        VehicleName = command.VehicleName;
        ImageUrl = command.ImageUrl;
        Tags = command.Tags ?? new List<string>();
        Featured = command.Featured;
        MaxCapacity = command.MaxCapacity;
        Rating = 0;
        ReviewsCount = 0;
    }

    public void Update(UpdateAdventureRouteCommand command)
    {
        Name = command.Name;
        Title = command.Title;
        Description = command.Description;
        StartLocation = command.StartLocation;
        EndLocation = command.EndLocation;
        Type = command.Type;
        Duration = command.Duration;
        Difficulty = command.Difficulty;
        EstimatedCost = command.EstimatedCost;
        VehicleName = command.VehicleName;
        ImageUrl = command.ImageUrl;
        Tags = command.Tags ?? new List<string>();
        Featured = command.Featured;
        MaxCapacity = command.MaxCapacity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRating(double newRating, int newReviewsCount)
    {
        Rating = newRating;
        ReviewsCount = newReviewsCount;
        UpdatedAt = DateTime.UtcNow;
    }
}
