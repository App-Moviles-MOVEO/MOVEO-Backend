using Moveo_backend.Payment.Domain.Model.Queries;
using PaymentEntity = Moveo_backend.Payment.Domain.Model.Aggregate.Payment;

namespace Moveo_backend.Payment.Domain.Services;

public interface IPaymentQueryService
{
    Task<PaymentEntity?> Handle(GetPaymentByIdQuery query);
    Task<IEnumerable<PaymentEntity>> Handle(GetAllPaymentsQuery query);
    Task<IEnumerable<PaymentEntity>> Handle(GetFilteredPaymentsQuery query);
    Task<IEnumerable<PaymentEntity>> Handle(GetPaymentsByPayerIdQuery query);
    Task<IEnumerable<PaymentEntity>> Handle(GetPaymentsByRecipientIdQuery query);
    Task<IEnumerable<PaymentEntity>> Handle(GetPaymentsByRentalIdQuery query);
}
