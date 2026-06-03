using Microsoft.EntityFrameworkCore;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Moveo_backend.Support.Domain.Model.Aggregate;
using Moveo_backend.Support.Domain.Repositories;

namespace Moveo_backend.Support.Infrastructure.Persistence.EFC.Repositories;

public class TicketMessageRepository(AppDbContext context) 
    : BaseRepository<TicketMessage>(context), ITicketMessageRepository
{
    public async Task<IEnumerable<TicketMessage>> FindByTicketIdAsync(int ticketId)
    {
        return await Context.Set<TicketMessage>()
            .Where(m => m.TicketId == ticketId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }
}
