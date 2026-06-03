using Moveo_backend.Shared.Domain.Repositories;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace Moveo_backend.Shared.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
///     Unit of work for the application.
/// </summary>
public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task CompleteAsync()
    {
        await context.SaveChangesAsync();
    }
}
