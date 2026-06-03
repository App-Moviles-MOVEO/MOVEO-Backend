using Moveo_backend.Shared.Domain.Repositories;
using PaymentEntity = Moveo_backend.Payment.Domain.Model.Aggregate.Payment;

namespace Moveo_backend.Payment.Domain.Repositories;

public interface IPaymentRepository : IBaseRepository<PaymentEntity>
{
    Task<IEnumerable<PaymentEntity>> FindByPayerIdAsync(int payerId);
    Task<IEnumerable<PaymentEntity>> FindByRecipientIdAsync(int recipientId);
    Task<IEnumerable<PaymentEntity>> FindByRentalIdAsync(int rentalId);
    Task<IEnumerable<PaymentEntity>> FindByStatusAsync(string status);
    Task<IEnumerable<PaymentEntity>> FindFilteredAsync(int? payerId, int? recipientId, int? rentalId, string? status, string? type);
}
