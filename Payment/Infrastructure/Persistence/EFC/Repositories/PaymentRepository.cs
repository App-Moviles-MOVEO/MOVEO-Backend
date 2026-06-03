using Microsoft.EntityFrameworkCore;
using Moveo_backend.Payment.Domain.Repositories;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Repositories;
using PaymentEntity = Moveo_backend.Payment.Domain.Model.Aggregate.Payment;

namespace Moveo_backend.Payment.Infrastructure.Persistence.EFC.Repositories;

public class PaymentRepository(AppDbContext context) 
    : BaseRepository<PaymentEntity>(context), IPaymentRepository
{
    public async Task<IEnumerable<PaymentEntity>> FindByPayerIdAsync(int payerId)
    {
        return await Context.Set<PaymentEntity>()
            .Where(p => p.PayerId == payerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<PaymentEntity>> FindByRecipientIdAsync(int recipientId)
    {
        return await Context.Set<PaymentEntity>()
            .Where(p => p.RecipientId == recipientId)
            .ToListAsync();
    }

    public async Task<IEnumerable<PaymentEntity>> FindByRentalIdAsync(int rentalId)
    {
        return await Context.Set<PaymentEntity>()
            .Where(p => p.RentalId == rentalId)
            .ToListAsync();
    }

    public async Task<IEnumerable<PaymentEntity>> FindByStatusAsync(string status)
    {
        return await Context.Set<PaymentEntity>()
            .Where(p => p.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<PaymentEntity>> FindFilteredAsync(
        int? payerId, int? recipientId, int? rentalId, string? status, string? type)
    {
        var query = Context.Set<PaymentEntity>().AsQueryable();
        
        if (payerId.HasValue)
            query = query.Where(p => p.PayerId == payerId.Value);
        if (recipientId.HasValue)
            query = query.Where(p => p.RecipientId == recipientId.Value);
        if (rentalId.HasValue)
            query = query.Where(p => p.RentalId == rentalId.Value);
        if (!string.IsNullOrEmpty(status))
            query = query.Where(p => p.Status == status);
        if (!string.IsNullOrEmpty(type))
            query = query.Where(p => p.Type == type);
            
        return await query.ToListAsync();
    }
}
