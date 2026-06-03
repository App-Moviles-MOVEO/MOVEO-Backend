using System.Text.Json.Serialization;

namespace Moveo_backend.Payment.Interfaces.REST.Resources;

public class UpdatePaymentResource
{
    public int PayerId { get; set; }
    public int RecipientId { get; set; }
    public int RentalId { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; } = "PEN";
    
    public string? PaymentMethod { get; set; }
    
    [JsonPropertyName("method")]
    public string? Method
    {
        get => PaymentMethod;
        set
        {
            if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(PaymentMethod))
                PaymentMethod = value;
        }
    }
    
    public string? Status { get; set; }
    public string? TransactionId { get; set; }
    public string? Type { get; set; } = "rental_payment";
    public string? Description { get; set; }
    public string? Reason { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public record PatchPaymentResource(
    string? Status,
    string? TransactionId,
    string? Reason,
    DateTime? CompletedAt
);
