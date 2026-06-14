using Microsoft.EntityFrameworkCore;
using Moveo_backend.Rental.Domain;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace Moveo_backend.Rental.Application.BackgroundServices;

/// <summary>
/// P5 (Opción A) — Expira reservas `pending` sin pagar.
///
/// Una reserva `pending` bloquea fechas (ver <see cref="RentalStatuses.Blocking"/>). Si el cliente
/// la crea y nunca paga, esas fechas quedarían bloqueadas para siempre. Este job, cada
/// <see cref="CheckIntervalMinutes"/> minutos, cancela las reservas `pending` con más de
/// <see cref="ExpirationMinutes"/> minutos de antigüedad que NO tengan un pago `completed`.
/// Al pasar a `cancelled`, sus fechas se liberan automáticamente.
/// </summary>
public class PendingRentalCleanupService : BackgroundService
{
    public const int ExpirationMinutes = 30;   // antigüedad máxima de un pending sin pagar
    public const int CheckIntervalMinutes = 5; // cada cuánto corre el chequeo

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PendingRentalCleanupService> _logger;

    public PendingRentalCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<PendingRentalCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al expirar reservas pending sin pagar");
            }

            await Task.Delay(TimeSpan.FromMinutes(CheckIntervalMinutes), stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var threshold = DateTime.UtcNow.AddMinutes(-ExpirationMinutes);

        var stalePending = await db.Rentals
            .Where(r => r.Status == RentalStatuses.Pending && r.CreatedAt < threshold)
            .ToListAsync(ct);

        if (stalePending.Count == 0) return;

        // No expirar las que ya tienen un pago completado (pueden estar a la espera de aceptación).
        var ids = stalePending.Select(r => r.Id).ToList();
        var paidRentalIds = (await db.Payments
            .Where(p => ids.Contains(p.RentalId) && p.Status == "completed")
            .Select(p => p.RentalId)
            .Distinct()
            .ToListAsync(ct)).ToHashSet();

        var toCancel = stalePending.Where(r => !paidRentalIds.Contains(r.Id)).ToList();
        if (toCancel.Count == 0) return;

        foreach (var rental in toCancel)
            rental.Cancel();

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Expiradas {Count} reservas pending sin pagar (>{Minutes} min).",
            toCancel.Count, ExpirationMinutes);
    }
}
