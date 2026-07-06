using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Moveo_backend.Payment.Domain.Model.Aggregate;
using Moveo_backend.Payment.Interfaces.REST.Resources;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace Moveo_backend.Payment.Interfaces.REST;

/// <summary>
/// Métodos de pago (renter) y de cobro/payout (owner). US21. Dos rutas sobre la misma entidad:
/// /api/v1/users/{userId}/payment-methods y /api/v1/users/{userId}/payout-methods.
/// </summary>
[ApiController]
[Produces("application/json")]
[SwaggerTag("Métodos de pago y de cobro (US21)")]
public class PaymentMethodsController(AppDbContext context) : ControllerBase
{
    // -------------------- payment-methods (renter) --------------------
    [HttpGet("api/v1/users/{userId:int}/payment-methods")]
    public Task<IActionResult> ListPayment(int userId) => List(userId, "payment");

    [HttpPost("api/v1/users/{userId:int}/payment-methods")]
    public Task<IActionResult> CreatePayment(int userId, [FromBody] CreatePaymentMethodResource resource) =>
        Create(userId, "payment", resource);

    [HttpDelete("api/v1/users/{userId:int}/payment-methods/{id:int}")]
    public Task<IActionResult> DeletePayment(int userId, int id) => Delete(userId, id, "payment");

    // -------------------- payout-methods (owner) --------------------
    [HttpGet("api/v1/users/{userId:int}/payout-methods")]
    public Task<IActionResult> ListPayout(int userId) => List(userId, "payout");

    [HttpPost("api/v1/users/{userId:int}/payout-methods")]
    public Task<IActionResult> CreatePayout(int userId, [FromBody] CreatePaymentMethodResource resource) =>
        Create(userId, "payout", resource);

    [HttpDelete("api/v1/users/{userId:int}/payout-methods/{id:int}")]
    public Task<IActionResult> DeletePayout(int userId, int id) => Delete(userId, id, "payout");

    // -------------------- lógica compartida --------------------
    private async Task<IActionResult> List(int userId, string category)
    {
        var methods = await context.PaymentMethods
            .Where(m => m.UserId == userId && m.Category == category)
            .OrderByDescending(m => m.IsDefault).ThenByDescending(m => m.CreatedAt)
            .ToListAsync();
        return Ok(methods.Select(ToResource));
    }

    private async Task<IActionResult> Create(int userId, string category, CreatePaymentMethodResource resource)
    {
        if (string.IsNullOrWhiteSpace(resource.Type) || string.IsNullOrWhiteSpace(resource.Label))
            return BadRequest(new { error = "invalid_request", message = "type y label son obligatorios" });

        var isFirst = !await context.PaymentMethods.AnyAsync(m => m.UserId == userId && m.Category == category);
        var makeDefault = resource.IsDefault || isFirst;

        if (makeDefault)
        {
            // Solo un método por categoría puede ser el default.
            var currentDefaults = await context.PaymentMethods
                .Where(m => m.UserId == userId && m.Category == category && m.IsDefault)
                .ToListAsync();
            currentDefaults.ForEach(m => m.SetDefault(false));
        }

        var method = new PaymentMethod(userId, category, resource.Type, resource.Label,
            resource.MaskedNumber, resource.Holder, makeDefault);
        context.PaymentMethods.Add(method);
        await context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, ToResource(method));
    }

    private async Task<IActionResult> Delete(int userId, int id, string category)
    {
        var method = await context.PaymentMethods
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId && m.Category == category);
        if (method == null) return NotFound();

        context.PaymentMethods.Remove(method);
        await context.SaveChangesAsync();
        return NoContent();
    }

    private static object ToResource(PaymentMethod m) => new
    {
        id = m.Id,
        userId = m.UserId,
        category = m.Category,
        type = m.Type,
        label = m.Label,
        maskedNumber = m.MaskedNumber,
        holder = m.Holder,
        isDefault = m.IsDefault,
        createdAt = m.CreatedAt
    };
}
