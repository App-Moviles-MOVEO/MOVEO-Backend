using System.Text.Json.Serialization;

namespace Moveo_backend.Support.Interfaces.REST.Resources;

public class CreateSupportTicketResource
{
    [JsonPropertyName("userId")]
    public int UserId { get; set; }
    
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("category")]
    public string? Category { get; set; }
    
    [JsonPropertyName("priority")]
    public string? Priority { get; set; }
    
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    
    [JsonPropertyName("relatedId")]
    public int? RelatedId { get; set; }
    
    [JsonPropertyName("relatedType")]
    public string? RelatedType { get; set; }
    
    // Damage ticket fields
    [JsonPropertyName("estimatedCost")]
    public decimal? EstimatedCost { get; set; }
    
    [JsonPropertyName("vehicleId")]
    public int? VehicleId { get; set; }
    
    [JsonPropertyName("vehicleName")]
    public string? VehicleName { get; set; }
    
    [JsonPropertyName("rentalId")]
    public int? RentalId { get; set; }
    
    [JsonPropertyName("renterId")]
    public int? RenterId { get; set; }
    
    [JsonPropertyName("renterName")]
    public string? RenterName { get; set; }
    
    [JsonPropertyName("attachments")]
    public List<string>? Attachments { get; set; }
    
    // Helper to get category with default
    public string GetCategory() => Category ?? "general";
    
    // Helper to get priority with default
    public string GetPriority() => Priority ?? "medium";
    
    // Helper to get type with default
    public new string GetType() => Type ?? "general";
    
    // Helper to convert attachments to JSON
    public string? GetAttachmentsJson() => Attachments != null && Attachments.Count > 0 
        ? System.Text.Json.JsonSerializer.Serialize(Attachments) 
        : null;
}
