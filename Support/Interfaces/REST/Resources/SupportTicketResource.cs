using System.Text.Json.Serialization;

namespace Moveo_backend.Support.Interfaces.REST.Resources;

public class SupportTicketResource
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("userId")]
    public int UserId { get; set; }
    
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("priority")]
    public string Priority { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("relatedId")]
    public int? RelatedId { get; set; }
    
    [JsonPropertyName("relatedType")]
    public string? RelatedType { get; set; }
    
    [JsonPropertyName("resolutionNotes")]
    public string? ResolutionNotes { get; set; }
    
    [JsonPropertyName("assignedToId")]
    public int? AssignedToId { get; set; }
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }
    
    [JsonPropertyName("resolvedAt")]
    public DateTime? ResolvedAt { get; set; }
    
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
    
    // Dispute fields
    [JsonPropertyName("disputeStatus")]
    public string? DisputeStatus { get; set; }
    
    [JsonPropertyName("disputeReason")]
    public string? DisputeReason { get; set; }
    
    [JsonPropertyName("disputeDescription")]
    public string? DisputeDescription { get; set; }
    
    [JsonPropertyName("disputedAt")]
    public DateTime? DisputedAt { get; set; }
    
    [JsonPropertyName("disputedBy")]
    public int? DisputedBy { get; set; }
    
    // Additional proof fields
    [JsonPropertyName("additionalProof")]
    public List<string>? AdditionalProof { get; set; }
    
    [JsonPropertyName("additionalProofMessage")]
    public string? AdditionalProofMessage { get; set; }
    
    [JsonPropertyName("additionalProofSentAt")]
    public DateTime? AdditionalProofSentAt { get; set; }
    
    // Payment fields
    [JsonPropertyName("paymentStatus")]
    public string? PaymentStatus { get; set; }
    
    [JsonPropertyName("paidAt")]
    public DateTime? PaidAt { get; set; }
    
    // Messages
    [JsonPropertyName("messages")]
    public IEnumerable<TicketMessageResource>? Messages { get; set; }
}
