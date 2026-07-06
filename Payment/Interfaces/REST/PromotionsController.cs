using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Moveo_backend.Payment.Domain.Model.Aggregate;
using Moveo_backend.Payment.Interfaces.REST.Resources;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace Moveo_backend.Payment.Interfaces.REST;

[ApiController]
[Route("api/v1/promotions")]
[Produces("application/json")]
[SwaggerTag("Cupones y ofertas promocionales (US27/US29/US34)")]
public class PromotionsController(AppDbContext context) : ControllerBase
{
    /// <summary>Lista promociones (opcional por owner y/o solo activas vigentes).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? ownerId, [FromQuery] bool? onlyActive)
    {
        var query = context.Promotions.AsQueryable();
        if (ownerId.HasValue) query = query.Where(p => p.OwnerId == ownerId.Value);
        var promos = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

        var now = DateTime.UtcNow;
        var result = promos.Where(p => onlyActive != true || p.Status(now) == "active");
        return Ok(result.Select(p => ToResource(p, now)));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var promo = await context.Promotions.FirstOrDefaultAsync(p => p.Id == id);
        if (promo == null) return NotFound();
        return Ok(ToResource(promo, DateTime.UtcNow));
    }

    /// <summary>Crea un cupón. El código es único (case-insensitive).</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePromotionResource resource)
    {
        if (string.IsNullOrWhiteSpace(resource.Code))
            return BadRequest(new { error = "invalid_request", message = "code es obligatorio" });
        if (resource.DiscountValue <= 0)
            return BadRequest(new { error = "invalid_request", message = "discountValue debe ser > 0" });

        var code = resource.Code.Trim().ToUpperInvariant();
        if (await context.Promotions.AnyAsync(p => p.Code == code))
            return Conflict(new { error = "duplicate_code", message = "Ya existe un cupón con ese código" });

        var starts = resource.StartsAt ?? DateTime.UtcNow;
        var ends = resource.EndsAt ?? starts.AddMonths(1);
        if (ends <= starts)
            return BadRequest(new { error = "invalid_request", message = "endsAt debe ser posterior a startsAt" });

        var promo = new Promotion(resource.OwnerId, code, resource.DiscountType ?? "percent",
            resource.DiscountValue, starts, ends, resource.MinReputation ?? 0, resource.MaxUses);
        context.Promotions.Add(promo);
        await context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, ToResource(promo, DateTime.UtcNow));
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Patch(int id, [FromBody] UpdatePromotionResource resource)
    {
        var promo = await context.Promotions.FirstOrDefaultAsync(p => p.Id == id);
        if (promo == null) return NotFound();

        promo.Update(resource.DiscountType, resource.DiscountValue, resource.StartsAt, resource.EndsAt,
            resource.MinReputation, resource.MaxUses, resource.Active);
        await context.SaveChangesAsync();
        return Ok(ToResource(promo, DateTime.UtcNow));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var promo = await context.Promotions.FirstOrDefaultAsync(p => p.Id == id);
        if (promo == null) return NotFound();
        context.Promotions.Remove(promo);
        await context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Valida un cupón contra un monto y la reputación del usuario, y devuelve el descuento aplicable.
    /// No consume el cupón (para eso usar /redeem al confirmar el pago).
    /// </summary>
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidatePromotionResource resource)
    {
        var code = (resource.Code ?? "").Trim().ToUpperInvariant();
        var promo = await context.Promotions.FirstOrDefaultAsync(p => p.Code == code);
        var now = DateTime.UtcNow;

        if (promo == null)
            return Ok(new { valid = false, reason = "not_found", discount = 0m, finalAmount = resource.Amount });

        var status = promo.Status(now);
        if (status != "active")
            return Ok(new { valid = false, reason = status, discount = 0m, finalAmount = resource.Amount });
        if (resource.UserReputation < promo.MinReputation)
            return Ok(new { valid = false, reason = "reputation_below_minimum", discount = 0m, finalAmount = resource.Amount });

        var discount = promo.ComputeDiscount(resource.Amount);
        return Ok(new
        {
            valid = true,
            reason = (string?)null,
            code = promo.Code,
            discount,
            finalAmount = Math.Round(resource.Amount - discount, 2)
        });
    }

    /// <summary>Consume un uso del cupón (llamar al confirmar el pago con el cupón aplicado).</summary>
    [HttpPost("{id:int}/redeem")]
    public async Task<IActionResult> Redeem(int id)
    {
        var promo = await context.Promotions.FirstOrDefaultAsync(p => p.Id == id);
        if (promo == null) return NotFound();
        if (promo.Status(DateTime.UtcNow) != "active")
            return UnprocessableEntity(new { error = "not_redeemable", message = "El cupón no está vigente" });

        promo.RegisterUse();
        await context.SaveChangesAsync();
        return Ok(ToResource(promo, DateTime.UtcNow));
    }

    private static object ToResource(Promotion p, DateTime now) => new
    {
        id = p.Id,
        ownerId = p.OwnerId,
        code = p.Code,
        discountType = p.DiscountType,
        discountValue = p.DiscountValue,
        startsAt = p.StartsAt,
        endsAt = p.EndsAt,
        minReputation = p.MinReputation,
        active = p.Active,
        maxUses = p.MaxUses,
        usedCount = p.UsedCount,
        status = p.Status(now),
        createdAt = p.CreatedAt
    };
}
