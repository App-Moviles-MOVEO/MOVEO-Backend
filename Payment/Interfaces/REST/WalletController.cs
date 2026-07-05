using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Moveo_backend.Payment.Domain.Model.Aggregate;
using Moveo_backend.Payment.Interfaces.REST.Resources;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace Moveo_backend.Payment.Interfaces.REST;

[ApiController]
[Produces("application/json")]
[SwaggerTag("Wallet y retiros de fondos")]
public class WalletController(AppDbContext context) : ControllerBase
{
    /// <summary>
    /// Balance del wallet: ingresos acreditados menos retiros (pendientes + completados).
    /// </summary>
    [HttpGet("api/v1/wallet/{userId:int}")]
    [SwaggerOperation(Summary = "Get wallet balance", OperationId = "GetWallet")]
    [SwaggerResponse(StatusCodes.Status200OK, "Balance del wallet")]
    public async Task<IActionResult> GetWallet(int userId)
    {
        var totalEarned = await context.Payments
            .Where(p => p.RecipientId == userId && p.Status == "completed")
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        var pendingWithdrawals = await context.Withdrawals
            .Where(w => w.UserId == userId && w.Status == "pending")
            .SumAsync(w => (decimal?)w.Amount) ?? 0m;

        var completedWithdrawals = await context.Withdrawals
            .Where(w => w.UserId == userId && w.Status == "completed")
            .SumAsync(w => (decimal?)w.Amount) ?? 0m;

        var balance = totalEarned - pendingWithdrawals - completedWithdrawals;

        return Ok(new
        {
            userId,
            balance,
            pendingWithdrawals,
            totalEarned
        });
    }

    /// <summary>Solicita un retiro; se valida contra el balance disponible.</summary>
    [HttpPost("api/v1/withdrawals")]
    [SwaggerOperation(Summary = "Request a withdrawal", OperationId = "CreateWithdrawal")]
    [SwaggerResponse(StatusCodes.Status201Created, "Retiro creado")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos inválidos")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Balance insuficiente")]
    public async Task<IActionResult> CreateWithdrawal([FromBody] CreateWithdrawalResource resource)
    {
        if (resource.UserId <= 0 || resource.Amount <= 0)
            return BadRequest(new { error = "invalid_request", message = "userId y amount (>0) son obligatorios" });
        if (string.IsNullOrWhiteSpace(resource.Destination))
            return BadRequest(new { error = "invalid_request", message = "destination es obligatorio" });

        var allowedMethods = new[] { "yape", "plin", "bank" };
        var method = string.IsNullOrWhiteSpace(resource.Method) ? "yape" : resource.Method.ToLowerInvariant();
        if (!allowedMethods.Contains(method))
            return BadRequest(new { error = "invalid_method", message = "method debe ser yape, plin o bank" });

        var totalEarned = await context.Payments
            .Where(p => p.RecipientId == resource.UserId && p.Status == "completed")
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;
        var reserved = await context.Withdrawals
            .Where(w => w.UserId == resource.UserId && (w.Status == "pending" || w.Status == "completed"))
            .SumAsync(w => (decimal?)w.Amount) ?? 0m;
        var available = totalEarned - reserved;

        if (resource.Amount > available)
            return Conflict(new { error = "insufficient_balance", message = $"Balance disponible: {available}", available });

        var withdrawal = new Withdrawal(resource.UserId, resource.Amount, method, resource.Destination);
        context.Withdrawals.Add(withdrawal);
        await context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, ToResource(withdrawal));
    }

    /// <summary>Historial de retiros de un usuario con sus estados.</summary>
    [HttpGet("api/v1/withdrawals/user/{userId:int}")]
    [SwaggerOperation(Summary = "List user withdrawals", OperationId = "GetUserWithdrawals")]
    [SwaggerResponse(StatusCodes.Status200OK, "Historial de retiros")]
    public async Task<IActionResult> GetUserWithdrawals(int userId)
    {
        var withdrawals = await context.Withdrawals
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
        return Ok(withdrawals.Select(ToResource));
    }

    /// <summary>Marca un retiro como completado o rechazado (proceso administrativo).</summary>
    [HttpPatch("api/v1/withdrawals/{id:int}")]
    [SwaggerOperation(Summary = "Update withdrawal status", OperationId = "PatchWithdrawal")]
    [SwaggerResponse(StatusCodes.Status200OK, "Retiro actualizado")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Estado inválido")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Retiro no encontrado")]
    public async Task<IActionResult> PatchWithdrawal(int id, [FromBody] PatchWithdrawalResource resource)
    {
        var withdrawal = await context.Withdrawals.FirstOrDefaultAsync(w => w.Id == id);
        if (withdrawal == null) return NotFound(new { error = "not_found", message = "Retiro no encontrado" });

        switch (resource.Status?.ToLowerInvariant())
        {
            case "completed":
                withdrawal.Complete();
                break;
            case "rejected":
                withdrawal.Reject(resource.RejectionReason);
                break;
            default:
                return BadRequest(new { error = "invalid_status", message = "status debe ser completed o rejected" });
        }

        await context.SaveChangesAsync();
        return Ok(ToResource(withdrawal));
    }

    private static object ToResource(Withdrawal w) => new
    {
        id = w.Id,
        userId = w.UserId,
        amount = w.Amount,
        method = w.Method,
        destination = w.Destination,
        status = w.Status,
        rejectionReason = w.RejectionReason,
        createdAt = w.CreatedAt,
        processedAt = w.ProcessedAt
    };
}
