namespace Moveo_backend.Payment.Interfaces.REST.Resources;

public record CreatePromotionResource(
    string Code,
    decimal DiscountValue,
    string? DiscountType = "percent",   // percent | fixed
    int? OwnerId = null,
    DateTime? StartsAt = null,
    DateTime? EndsAt = null,
    double? MinReputation = 0,
    int? MaxUses = null
);

public record UpdatePromotionResource(
    string? DiscountType = null,
    decimal? DiscountValue = null,
    DateTime? StartsAt = null,
    DateTime? EndsAt = null,
    double? MinReputation = null,
    int? MaxUses = null,
    bool? Active = null
);

public record ValidatePromotionResource(
    string Code,
    decimal Amount,
    double UserReputation = 0
);
