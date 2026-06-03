using System.Text.Json.Serialization;

namespace Moveo_backend.Support.Interfaces.REST.Resources;

public class UpdateSupportTicketResource
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    
    [JsonPropertyName("priority")]
    public string? Priority { get; set; }
    
    [JsonPropertyName("assignedToId")]
    public int? AssignedToId { get; set; }
    
    [JsonPropertyName("resolutionNotes")]
    public string? ResolutionNotes { get; set; }
    
    [JsonPropertyName("resolvedAt")]
    public DateTime? ResolvedAt { get; set; }
    
    // Damage cost update
    [JsonPropertyName("estimatedCost")]
    public decimal? EstimatedCost { get; set; }
    
    // Dispute fields
    [JsonPropertyName("disputeStatus")]
    public string? DisputeStatus { get; set; }
    
    [JsonPropertyName("disputeReason")]
    public string? DisputeReason { get; set; }
    
    [JsonPropertyName("disputeDescription")]
    public string? DisputeDescription { get; set; }
    
    [JsonPropertyName("disputedBy")]
    public int? DisputedBy { get; set; }
    
    // Additional proof fields (owner response to dispute)
    [JsonPropertyName("additionalProof")]
    public List<string>? AdditionalProof { get; set; }
    
    [JsonPropertyName("additionalProofMessage")]
    public string? AdditionalProofMessage { get; set; }
    
    // Payment fields
    [JsonPropertyName("paymentStatus")]
    public string? PaymentStatus { get; set; }
    
    // Helper to convert additional proof to JSON
    public string? GetAdditionalProofJson() => AdditionalProof != null && AdditionalProof.Count > 0 
        ? System.Text.Json.JsonSerializer.Serialize(AdditionalProof) 
        : null;
}
