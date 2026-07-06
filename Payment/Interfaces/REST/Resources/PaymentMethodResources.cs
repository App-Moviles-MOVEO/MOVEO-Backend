namespace Moveo_backend.Payment.Interfaces.REST.Resources;

public record CreatePaymentMethodResource(
    string Type,          // card | yape | plin | bank
    string Label,         // alias visible
    string? MaskedNumber = null,
    string? Holder = null,
    bool IsDefault = false
);
