using Microsoft.EntityFrameworkCore;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Moveo_backend.Support.Domain.Model.Aggregate;
using Moveo_backend.Support.Domain.Repositories;

namespace Moveo_backend.Support.Infrastructure.Persistence.EFC.Repositories;

public class SupportTicketRepository(AppDbContext context) 
    : BaseRepository<SupportTicket>(context), ISupportTicketRepository
{
    public async Task<IEnumerable<SupportTicket>> FindByUserIdAsync(int userId)
    {
        return await Context.Set<SupportTicket>()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportTicket>> FindByStatusAsync(string status)
    {
        return await Context.Set<SupportTicket>()
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<SupportTicket?> FindByIdWithMessagesAsync(int id)
    {
        return await Context.Set<SupportTicket>()
            .Include(t => t.Messages)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}
