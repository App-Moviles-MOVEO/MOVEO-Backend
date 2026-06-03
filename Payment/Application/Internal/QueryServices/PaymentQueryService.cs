using Moveo_backend.Payment.Domain.Model.Queries;
using Moveo_backend.Payment.Domain.Repositories;
using Moveo_backend.Payment.Domain.Services;
using PaymentEntity = Moveo_backend.Payment.Domain.Model.Aggregate.Payment;

namespace Moveo_backend.Payment.Application.Internal.QueryServices;

public class PaymentQueryService(IPaymentRepository paymentRepository) : IPaymentQueryService
{
    public async Task<PaymentEntity?> Handle(GetPaymentByIdQuery query)
    {
        return await paymentRepository.FindByIdAsync(query.Id);
    }

    public async Task<IEnumerable<PaymentEntity>> Handle(GetAllPaymentsQuery query)
    {
        return await paymentRepository.ListAsync();
    }

    public async Task<IEnumerable<PaymentEntity>> Handle(GetFilteredPaymentsQuery query)
    {
        return await paymentRepository.FindFilteredAsync(
            query.PayerId, query.RecipientId, query.RentalId, query.Status, query.Type);
    }

    public async Task<IEnumerable<PaymentEntity>> Handle(GetPaymentsByPayerIdQuery query)
    {
        return await paymentRepository.FindByPayerIdAsync(query.PayerId);
    }

    public async Task<IEnumerable<PaymentEntity>> Handle(GetPaymentsByRecipientIdQuery query)
    {
        return await paymentRepository.FindByRecipientIdAsync(query.RecipientId);
    }

    public async Task<IEnumerable<PaymentEntity>> Handle(GetPaymentsByRentalIdQuery query)
    {
        return await paymentRepository.FindByRentalIdAsync(query.RentalId);
    }
}
