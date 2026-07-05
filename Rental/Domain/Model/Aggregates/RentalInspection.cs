using System.Text.Json;

namespace Moveo_backend.Rental.Domain.Model.Aggregates;

/// <summary>
/// Checklist fotográfico de inspección de un alquiler (US12). Guarda el conjunto de fotos
/// (punto -> URL) tomadas antes (PRE) o después (POST) del alquiler, como evidencia en disputas.
/// </summary>
public class RentalInspection
{
    public int Id { get; private set; }
    public int RentalId { get; private set; }
    public string Type { get; private set; } = "PRE"; // "PRE" | "POST"
    public int? CreatedById { get; private set; }     // quién la registró (owner normalmente)
    public string? Notes { get; private set; }
    public string PhotosJson { get; private set; } = "{}";
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public Dictionary<string, string> Photos
    {
        get => JsonSerializer.Deserialize<Dictionary<string, string>>(PhotosJson) ?? new();
        private set => PhotosJson = JsonSerializer.Serialize(value);
    }

    protected RentalInspection() { }

    public RentalInspection(int rentalId, string type, Dictionary<string, string> photos, int? createdById, string? notes)
    {
        RentalId = rentalId;
        Type = type.ToUpperInvariant() == "POST" ? "POST" : "PRE";
        Photos = photos;
        CreatedById = createdById;
        Notes = notes;
        CreatedAt = DateTime.UtcNow;
    }
}
