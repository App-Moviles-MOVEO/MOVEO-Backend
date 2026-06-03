using System.Text.Json.Serialization;

namespace Moveo_backend.Payment.Interfaces.REST.Resources;

/// <summary>
/// Resource for creating a payment.
/// Accepts both 'method' and 'paymentMethod' for frontend compatibility.
/// </summary>
public class CreatePaymentResource
{
    public int PayerId { get; set; }
    public int RecipientId { get; set; }
    public int RentalId { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; } = "PEN";
    
    /// <summary>
    /// Payment method (e.g., "card", "cash", "transfer")
    /// </summary>
    public string? PaymentMethod { get; set; }
    
    /// <summary>
    /// Legacy field - maps to PaymentMethod for backward compatibility
    /// </summary>
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
    
    /// <summary>
    /// Payment type (e.g., "rental_payment", "deposit", "damage_payment")
    /// Defaults to "rental_payment" if not provided
    /// </summary>
    public string? Type { get; set; } = "rental_payment";
    
    public string? Status { get; set; } = "pending";
    public string? TransactionId { get; set; }
    public string? Description { get; set; }
    public string? Reason { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CreatedAt { get; set; }
}
