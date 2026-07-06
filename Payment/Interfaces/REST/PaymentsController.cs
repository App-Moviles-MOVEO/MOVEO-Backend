using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moveo_backend.Payment.Domain.Model.Commands;
using Moveo_backend.Payment.Domain.Model.Queries;
using Moveo_backend.Payment.Domain.Services;
using Moveo_backend.Payment.Interfaces.REST.Resources;
using Moveo_backend.Payment.Interfaces.REST.Transform;
using Moveo_backend.Notification.Domain.Model.Commands;
using Moveo_backend.Notification.Domain.Services;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace Moveo_backend.Payment.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
public class PaymentsController(
    IPaymentCommandService paymentCommandService,
    IPaymentQueryService paymentQueryService,
    INotificationCommandService notificationCommandService,
    AppDbContext context) : ControllerBase
{
    // GET /api/v1/payments with optional filters
    [HttpGet]
    public async Task<IActionResult> GetAllPayments(
        [FromQuery] int? payerId,
        [FromQuery] int? recipientId,
        [FromQuery] int? rentalId,
        [FromQuery] string? status,
        [FromQuery] string? type)
    {
        var query = new GetFilteredPaymentsQuery(payerId, recipientId, rentalId, status, type);
        var payments = await paymentQueryService.Handle(query);
        var resources = payments.Select(PaymentResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    /// <summary>
    /// Monitoreo automático de anomalías financieras (US40): montos atípicos (media+3σ),
    /// cobros duplicados (mismo pagador/monto/día) y exceso de reembolsos. Si se pasa userId,
    /// analiza los pagos donde participa; si no, barre todos.
    /// </summary>
    [HttpGet("anomalies")]
    public async Task<IActionResult> GetAnomalies([FromQuery] int? userId)
    {
        var query = context.Payments.AsQueryable();
        if (userId.HasValue)
            query = query.Where(p => p.PayerId == userId.Value || p.RecipientId == userId.Value);

        var payments = await query
            .Select(p => new { p.Id, p.PayerId, p.RecipientId, p.Amount, p.Status, p.Type, p.CreatedAt })
            .ToListAsync();

        var anomalies = new List<object>();

        // 1) Montos atípicos: por encima de media + 3σ de los pagos completados.
        var completed = payments.Where(p => p.Status == "completed").Select(p => (double)p.Amount).ToList();
        if (completed.Count >= 3)
        {
            var mean = completed.Average();
            var std = Math.Sqrt(completed.Average(a => Math.Pow(a - mean, 2)));
            var threshold = mean + 3 * std;
            foreach (var p in payments.Where(p => p.Status == "completed" && (double)p.Amount > threshold))
                anomalies.Add(new { paymentId = p.Id, type = "amount_outlier",
                    detail = $"Monto {p.Amount} supera el umbral {Math.Round(threshold, 2)}", severity = "high" });
        }

        // 2) Cobros duplicados: mismo pagador + monto + día.
        var duplicates = payments
            .GroupBy(p => new { p.PayerId, p.Amount, Day = p.CreatedAt.Date })
            .Where(g => g.Count() > 1);
        foreach (var g in duplicates)
            anomalies.Add(new { paymentIds = g.Select(x => x.Id).ToList(), type = "duplicate_charge",
                detail = $"{g.Count()} cobros del pagador {g.Key.PayerId} por {g.Key.Amount} el mismo día", severity = "medium" });

        // 3) Exceso de reembolsos: más del 30% de los pagos del usuario son refunds.
        var refunds = payments.Count(p => p.Type == "refund" || p.Status == "refunded");
        if (payments.Count >= 5 && refunds > payments.Count * 0.3)
            anomalies.Add(new { type = "excess_refunds",
                detail = $"{refunds} de {payments.Count} movimientos son reembolsos", severity = "medium" });

        return Ok(new { userId, scanned = payments.Count, flagged = anomalies.Count, anomalies });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPaymentById(int id)
    {
        var query = new GetPaymentByIdQuery(id);
        var payment = await paymentQueryService.Handle(query);
        if (payment is null) return NotFound();
        var resource = PaymentResourceFromEntityAssembler.ToResourceFromEntity(payment);
        return Ok(resource);
    }

    [HttpGet("payer/{payerId:int}")]
    public async Task<IActionResult> GetPaymentsByPayerId(int payerId)
    {
        var query = new GetPaymentsByPayerIdQuery(payerId);
        var payments = await paymentQueryService.Handle(query);
        var resources = payments.Select(PaymentResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("recipient/{recipientId:int}")]
    public async Task<IActionResult> GetPaymentsByRecipientId(int recipientId)
    {
        var query = new GetPaymentsByRecipientIdQuery(recipientId);
        var payments = await paymentQueryService.Handle(query);
        var resources = payments.Select(PaymentResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("rental/{rentalId:int}")]
    public async Task<IActionResult> GetPaymentsByRentalId(int rentalId)
    {
        var query = new GetPaymentsByRentalIdQuery(rentalId);
        var payments = await paymentQueryService.Handle(query);
        var resources = payments.Select(PaymentResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentResource resource)
    {
        var command = CreatePaymentCommandFromResourceAssembler.ToCommandFromResource(resource);
        var payment = await paymentCommandService.Handle(command);
        if (payment is null) return BadRequest();
        var paymentResource = PaymentResourceFromEntityAssembler.ToResourceFromEntity(payment);
        return CreatedAtAction(nameof(GetPaymentById), new { id = payment.Id }, paymentResource);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdatePayment(int id, [FromBody] UpdatePaymentResource resource)
    {
        var command = UpdatePaymentCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var payment = await paymentCommandService.Handle(command);
        if (payment is null) return NotFound();
        var paymentResource = PaymentResourceFromEntityAssembler.ToResourceFromEntity(payment);
        return Ok(paymentResource);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> PatchPayment(int id, [FromBody] PatchPaymentResource resource)
    {
        var command = UpdatePaymentCommandFromResourceAssembler.ToPatchCommandFromResource(id, resource);
        var payment = await paymentCommandService.Handle(command);
        if (payment is null) return NotFound();
        var paymentResource = PaymentResourceFromEntityAssembler.ToResourceFromEntity(payment);
        return Ok(paymentResource);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePayment(int id)
    {
        var command = new DeletePaymentCommand(id);
        var result = await paymentCommandService.Handle(command);
        if (!result) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Ejecuta el reembolso de un pago aplicando la política por antelación de cancelación
    /// (>48h = 100%, 24-48h = 50%, &lt;24h = 0%). Marca el pago como refunded, crea el registro
    /// de reembolso y notifica a ambas partes. 422 si la política no permite reembolso.
    /// </summary>
    [HttpPost("{id:int}/refund")]
    public async Task<IActionResult> Refund(int id, [FromBody] RefundRequestResource? body)
    {
        var payment = await paymentQueryService.Handle(new GetPaymentByIdQuery(id));
        if (payment is null) return NotFound(new { error = "not_found", message = "Pago no encontrado" });

        if (payment.Status == "refunded")
            return UnprocessableEntity(new { error = "already_refunded", message = "El pago ya fue reembolsado" });

        // La antelación se mide contra la fecha de inicio del alquiler asociado.
        var rental = await context.Rentals.FirstOrDefaultAsync(r => r.Id == payment.RentalId);
        if (rental is null)
            return UnprocessableEntity(new { error = "rental_not_found", message = "No se encontró el alquiler del pago" });

        var hoursBeforeStart = (rental.StartDate - DateTime.UtcNow).TotalHours;
        var (pct, policy) = hoursBeforeStart switch
        {
            >= 48 => (1.0m, "100%"),
            >= 24 => (0.5m, "50%"),
            _ => (0.0m, "0%")
        };

        if (pct == 0m)
            return UnprocessableEntity(new
            {
                error = "refund_not_allowed",
                message = "La política no permite reembolso con menos de 24h de antelación",
                policy
            });

        var refundedAmount = Math.Round(payment.Amount * pct, 2);

        // 1) Marca el pago original como reembolsado.
        await paymentCommandService.Handle(new PatchPaymentCommand(
            payment.Id,
            Status: "refunded",
            Reason: $"Reembolso {policy} por cancelación ({body?.Reason ?? "sin motivo"})",
            CompletedAt: DateTime.UtcNow));

        // 2) Crea el movimiento de reembolso (dinero del owner de vuelta al renter).
        var refundPayment = await paymentCommandService.Handle(new CreatePaymentCommand(
            PayerId: payment.RecipientId,
            RecipientId: payment.PayerId,
            RentalId: payment.RentalId,
            Amount: refundedAmount,
            Currency: payment.Currency,
            Method: payment.Method,
            Type: "refund",
            Description: $"Reembolso {policy} del pago #{payment.Id}"));
        if (refundPayment != null)
            await paymentCommandService.Handle(new PatchPaymentCommand(
                refundPayment.Id, Status: "completed", CompletedAt: DateTime.UtcNow));

        // 3) Notifica a ambas partes.
        await notificationCommandService.Handle(new CreateNotificationCommand(
            payment.PayerId, "Reembolso procesado",
            $"Se te reembolsó {refundedAmount} ({policy}) por la cancelación.", "payment",
            payment.RentalId, "rental", null, null, null));
        await notificationCommandService.Handle(new CreateNotificationCommand(
            payment.RecipientId, "Reembolso emitido",
            $"Se emitió un reembolso de {refundedAmount} ({policy}) al arrendatario.", "payment",
            payment.RentalId, "rental", null, null, null));

        return Ok(new { refundedAmount, policy, status = "refunded" });
    }
}
