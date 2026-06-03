using Moveo_backend.Payment.Domain.Model.Commands;
using Moveo_backend.Payment.Interfaces.REST.Resources;

namespace Moveo_backend.Payment.Interfaces.REST.Transform;

public static class UpdatePaymentCommandFromResourceAssembler
{
    public static UpdatePaymentCommand ToCommandFromResource(int id, UpdatePaymentResource resource)
    {
        // Normalize: use PaymentMethod or Method, default to "card"
        var method = resource.PaymentMethod ?? resource.Method ?? "card";
        var type = resource.Type ?? "rental_payment";
        var currency = resource.Currency ?? "PEN";
        var status = resource.Status ?? "pending";
        
        return new UpdatePaymentCommand(
            id,
            resource.PayerId,
            resource.RecipientId,
            resource.RentalId,
            resource.Amount,
            currency,
            method,
            status,
            resource.TransactionId,
            type,
            resource.Description,
            resource.Reason,
            resource.DueDate,
            resource.CompletedAt
        );
    }

    public static PatchPaymentCommand ToPatchCommandFromResource(int id, PatchPaymentResource resource)
    {
        return new PatchPaymentCommand(
            id,
            resource.Status,
            resource.TransactionId,
            resource.Reason,
            resource.CompletedAt
        );
    }
}
