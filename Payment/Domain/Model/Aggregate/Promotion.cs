namespace Moveo_backend.Payment.Domain.Model.Aggregate;

/// <summary>
/// Cupón / oferta promocional (US27, US29, US34). Puede ser global (OwnerId null) o de un owner.
/// El descuento es porcentual o de monto fijo, con vigencia y reputación mínima opcional (US29).
/// </summary>
public class Promotion
{
    public int Id { get; private set; }
    public int? OwnerId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string DiscountType { get; private set; } = "percent"; // "percent" | "fixed"
    public decimal DiscountValue { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }
    public double MinReputation { get; private set; }
    public bool Active { get; private set; } = true;
    public int? MaxUses { get; private set; }
    public int UsedCount { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    protected Promotion() { }

    public Promotion(int? ownerId, string code, string discountType, decimal discountValue,
        DateTime startsAt, DateTime endsAt, double minReputation, int? maxUses)
    {
        OwnerId = ownerId;
        Code = code.Trim().ToUpperInvariant();
        DiscountType = discountType == "fixed" ? "fixed" : "percent";
        DiscountValue = discountValue;
        StartsAt = startsAt;
        EndsAt = endsAt;
        MinReputation = minReputation;
        MaxUses = maxUses;
        Active = true;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>Estado calculado: scheduled | active | expired | inactive.</summary>
    public string Status(DateTime now)
    {
        if (!Active) return "inactive";
        if (now < StartsAt) return "scheduled";
        if (now > EndsAt) return "expired";
        if (MaxUses.HasValue && UsedCount >= MaxUses.Value) return "expired";
        return "active";
    }

    public bool IsRedeemable(DateTime now, double userReputation) =>
        Status(now) == "active" && userReputation >= MinReputation;

    /// <summary>Calcula el descuento sobre un monto (nunca mayor que el monto).</summary>
    public decimal ComputeDiscount(decimal amount)
    {
        var discount = DiscountType == "percent"
            ? amount * (DiscountValue / 100m)
            : DiscountValue;
        return Math.Round(Math.Min(Math.Max(discount, 0m), amount), 2);
    }

    public void SetActive(bool value) => Active = value;
    public void RegisterUse() => UsedCount++;

    public void Update(string? discountType, decimal? discountValue, DateTime? startsAt, DateTime? endsAt,
        double? minReputation, int? maxUses, bool? active)
    {
        if (discountType != null) DiscountType = discountType == "fixed" ? "fixed" : "percent";
        if (discountValue.HasValue) DiscountValue = discountValue.Value;
        if (startsAt.HasValue) StartsAt = startsAt.Value;
        if (endsAt.HasValue) EndsAt = endsAt.Value;
        if (minReputation.HasValue) MinReputation = minReputation.Value;
        if (maxUses.HasValue) MaxUses = maxUses.Value;
        if (active.HasValue) Active = active.Value;
    }
}
