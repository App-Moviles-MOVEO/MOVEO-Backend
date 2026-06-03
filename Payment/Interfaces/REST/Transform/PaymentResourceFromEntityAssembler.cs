using Moveo_backend.Payment.Interfaces.REST.Resources;
using PaymentEntity = Moveo_backend.Payment.Domain.Model.Aggregate.Payment;

namespace Moveo_backend.Payment.Interfaces.REST.Transform;

public static class PaymentResourceFromEntityAssembler
{
    public static PaymentResource ToResourceFromEntity(PaymentEntity entity)
    {
        return new PaymentResource
        {
            Id = entity.Id,
            PayerId = entity.PayerId,
            RecipientId = entity.RecipientId,
            RentalId = entity.RentalId,
            Amount = entity.Amount,
            Currency = entity.Currency,
            PaymentMethod = entity.Method,
            Status = entity.Status,
            TransactionId = entity.TransactionId,
            Type = entity.Type,
            Description = entity.Description,
            Reason = entity.Reason,
            DueDate = entity.DueDate,
            CreatedAt = entity.CreatedAt,
            CompletedAt = entity.CompletedAt
        };
    }
}
