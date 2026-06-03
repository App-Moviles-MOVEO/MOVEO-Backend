using Moveo_backend.Payment.Domain.Model.Commands;
using Moveo_backend.Payment.Interfaces.REST.Resources;

namespace Moveo_backend.Payment.Interfaces.REST.Transform;

public static class CreatePaymentCommandFromResourceAssembler
{
    public static CreatePaymentCommand ToCommandFromResource(CreatePaymentResource resource)
    {
        // Normalize: use PaymentMethod or Method, default to "card"
        var method = resource.PaymentMethod ?? resource.Method ?? "card";
        
        // Normalize: default type to "rental_payment"
        var type = resource.Type ?? "rental_payment";
        
        // Normalize: default currency to "PEN"
        var currency = resource.Currency ?? "PEN";
        
        return new CreatePaymentCommand(
            resource.PayerId,
            resource.RecipientId,
            resource.RentalId,
            resource.Amount,
            currency,
            method,
            type,
            resource.Description,
            resource.DueDate
        );
    }
}
