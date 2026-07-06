namespace Moveo_backend.Payment.Domain.Model.Aggregate;

/// <summary>
/// Método de pago (renter) o de cobro/payout (owner) vinculado a un usuario (US21).
/// Category distingue el uso: "payment" (con qué paga) o "payout" (a dónde recibe).
/// Nunca se guardan datos sensibles completos: solo un enmascarado para mostrar en la UI.
/// </summary>
public class PaymentMethod
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string Category { get; private set; } = "payment"; // "payment" | "payout"
    public string Type { get; private set; } = "card";        // "card" | "yape" | "plin" | "bank"
    public string Label { get; private set; } = string.Empty;  // alias visible ("Visa •••• 4242")
    public string? MaskedNumber { get; private set; }          // últimos dígitos / número enmascarado
    public string? Holder { get; private set; }                // titular
    public bool IsDefault { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    protected PaymentMethod() { }

    public PaymentMethod(int userId, string category, string type, string label, string? maskedNumber, string? holder, bool isDefault)
    {
        UserId = userId;
        Category = category == "payout" ? "payout" : "payment";
        Type = string.IsNullOrWhiteSpace(type) ? "card" : type.ToLowerInvariant();
        Label = label;
        MaskedNumber = maskedNumber;
        Holder = holder;
        IsDefault = isDefault;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetDefault(bool value) => IsDefault = value;
}
