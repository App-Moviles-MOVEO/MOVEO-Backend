using Microsoft.AspNetCore.Mvc;
using Moveo_backend.Payment.Domain.Model.Commands;
using Moveo_backend.Payment.Domain.Model.Queries;
using Moveo_backend.Payment.Domain.Services;
using Moveo_backend.Payment.Interfaces.REST.Resources;
using Moveo_backend.Payment.Interfaces.REST.Transform;

namespace Moveo_backend.Payment.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
public class PaymentsController(
    IPaymentCommandService paymentCommandService,
    IPaymentQueryService paymentQueryService) : ControllerBase
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
}
