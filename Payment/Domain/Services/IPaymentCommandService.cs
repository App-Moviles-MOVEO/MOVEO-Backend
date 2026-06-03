using Moveo_backend.Payment.Domain.Model.Commands;
using PaymentEntity = Moveo_backend.Payment.Domain.Model.Aggregate.Payment;

namespace Moveo_backend.Payment.Domain.Services;

public interface IPaymentCommandService
{
    Task<PaymentEntity?> Handle(CreatePaymentCommand command);
    Task<PaymentEntity?> Handle(UpdatePaymentCommand command);
    Task<PaymentEntity?> Handle(PatchPaymentCommand command);
    Task<bool> Handle(DeletePaymentCommand command);
}
