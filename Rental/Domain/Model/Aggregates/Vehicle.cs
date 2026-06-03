using Moveo_backend.Rental.Domain.Model.ValueObjects;
using System.Text.Json;

namespace Moveo_backend.Rental.Domain.Model.Aggregates;

public class Vehicle
{
    public int Id { get; private set; }
    public int OwnerId { get; private set; }

    public string Brand { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public int Year { get; private set; }
    public string Color { get; private set; } = string.Empty;

    public string Transmission { get; private set; } = string.Empty;
    public string FuelType { get; private set; } = string.Empty;
    public int Seats { get; private set; }
    public string LicensePlate { get; private set; } = string.Empty;

    // Usar tipos complejos como Owned Types
    public Money DailyPrice { get; private set; } = null!;
    public Money DepositAmount { get; private set; } = null!;

    public Location Location { get; private set; } = null!;

    // Para EF Core mapeamos listas como JSON
    public string FeaturesJson { get; private set; } = "[]";
    public string RestrictionsJson { get; private set; } = "[]";
    public string ImagesJson { get; private set; } = "[]";

    public string Status { get; private set; } = "active";
    public string? Description { get; private set; }
    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    // Propiedades para acceder a las listas como objetos
    public List<string> Features
    {
        get => JsonSerializer.Deserialize<List<string>>(FeaturesJson) ?? new List<string>();
        private set => FeaturesJson = JsonSerializer.Serialize(value);
    }

    public List<string> Restrictions
    {
        get => JsonSerializer.Deserialize<List<string>>(RestrictionsJson) ?? new List<string>();
        private set => RestrictionsJson = JsonSerializer.Serialize(value);
    }

    public List<string> Images
    {
        get => JsonSerializer.Deserialize<List<string>>(ImagesJson) ?? new List<string>();
        private set => ImagesJson = JsonSerializer.Serialize(value);
    }

    // Constructor principal
    public Vehicle(
        int ownerId,
        string brand,
        string model,
        int year,
        string color,
        string transmission,
        string fuelType,
        int seats,
        string licensePlate,
        Money dailyPrice,
        Money depositAmount,
        Location location,
        string? description = null,
        IEnumerable<string>? features = null,
        IEnumerable<string>? restrictions = null,
        IEnumerable<string>? images = null)
    {
        OwnerId = ownerId;
        Brand = brand;
        Model = model;
        Year = year;
        Color = color;
        Transmission = transmission;
        FuelType = fuelType;
        Seats = seats;
        LicensePlate = licensePlate;
        DailyPrice = dailyPrice;
        DepositAmount = depositAmount;
        Location = location;
        Description = description;
        Features = features?.ToList() ?? new List<string>();
        Restrictions = restrictions?.ToList() ?? new List<string>();
        Images = images?.ToList() ?? new List<string>();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // ⚠ Constructor vacío necesario para EF Core
    protected Vehicle() { }

    public void UpdateDetails(
        int ownerId,
        string brand,
        string model,
        int year,
        string color,
        string transmission,
        string fuelType,
        int seats,
        string licensePlate,
        Money dailyPrice,
        Money depositAmount,
        Location location,
        string status,
        string? description,
        IEnumerable<string>? features,
        IEnumerable<string>? restrictions,
        IEnumerable<string>? images)
    {
        OwnerId = ownerId;
        Brand = brand;
        Model = model;
        Year = year;
        Color = color;
        Transmission = transmission;
        FuelType = fuelType;
        Seats = seats;
        LicensePlate = licensePlate;
        DailyPrice = dailyPrice;
        DepositAmount = depositAmount;
        Location = location;
        Status = status;
        Description = description;
        Features = features?.ToList() ?? new List<string>();
        Restrictions = restrictions?.ToList() ?? new List<string>();
        Images = images?.ToList() ?? new List<string>();
        UpdatedAt = DateTime.UtcNow;
    }

    public void PartialUpdate(
        decimal? dailyPrice = null,
        string? status = null,
        string? description = null,
        IEnumerable<string>? features = null,
        IEnumerable<string>? restrictions = null,
        IEnumerable<string>? images = null)
    {
        if (dailyPrice.HasValue) DailyPrice = new Money(dailyPrice.Value);
        if (status != null) Status = status;
        if (description != null) Description = description;
        if (features != null) Features = features.ToList();
        if (restrictions != null) Restrictions = restrictions.ToList();
        if (images != null) Images = images.ToList();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeStatus(string status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
}
