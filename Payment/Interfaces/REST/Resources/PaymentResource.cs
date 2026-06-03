using System.Text.Json.Serialization;

namespace Moveo_backend.Payment.Interfaces.REST.Resources;

public class PaymentResource
{
    public int Id { get; set; }
    public int PayerId { get; set; }
    public int RecipientId { get; set; }
    public int RentalId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "PEN";
    
    /// <summary>
    /// Payment method - returned as both 'paymentMethod' and 'method' for compatibility
    /// </summary>
    public string PaymentMethod { get; set; } = "card";
    
    /// <summary>
    /// Alias for PaymentMethod (for backward compatibility)
    /// </summary>
    [JsonPropertyName("method")]
    public string Method => PaymentMethod;
    
    public string Status { get; set; } = "pending";
    public string? TransactionId { get; set; }
    public string Type { get; set; } = "rental_payment";
    public string? Description { get; set; }
    public string? Reason { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
