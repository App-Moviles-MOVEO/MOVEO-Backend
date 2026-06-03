using Microsoft.AspNetCore.Mvc;
using Moveo_backend.Support.Domain.Model.Commands;
using Moveo_backend.Support.Domain.Model.Queries;
using Moveo_backend.Support.Domain.Services;
using Moveo_backend.Support.Interfaces.REST.Resources;
using Moveo_backend.Support.Interfaces.REST.Transform;

namespace Moveo_backend.Support.Interfaces.REST;

[ApiController]
[Route("api/v1/support-tickets")]
public class SupportTicketsController(
    ISupportTicketCommandService supportTicketCommandService,
    ISupportTicketQueryService supportTicketQueryService,
    ITicketMessageCommandService ticketMessageCommandService,
    ITicketMessageQueryService ticketMessageQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllSupportTickets(
        [FromQuery] int? userId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? type = null)
    {
        IEnumerable<Domain.Model.Aggregate.SupportTicket> tickets;
        
        if (userId.HasValue)
        {
            var query = new GetSupportTicketsByUserIdQuery(userId.Value);
            tickets = await supportTicketQueryService.Handle(query);
        }
        else if (!string.IsNullOrEmpty(status))
        {
            var query = new GetSupportTicketsByStatusQuery(status);
            tickets = await supportTicketQueryService.Handle(query);
        }
        else
        {
            var query = new GetAllSupportTicketsQuery();
            tickets = await supportTicketQueryService.Handle(query);
        }
        
        var resources = tickets.Select(t => SupportTicketResourceFromEntityAssembler.ToResourceFromEntity(t));
        return Ok(resources);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSupportTicketById(int id)
    {
        var query = new GetSupportTicketByIdQuery(id);
        var ticket = await supportTicketQueryService.Handle(query);
        if (ticket is null) return NotFound();
        var resource = SupportTicketResourceFromEntityAssembler.ToResourceFromEntity(ticket, includeMessages: true);
        return Ok(resource);
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetSupportTicketsByUserId(int userId)
    {
        var query = new GetSupportTicketsByUserIdQuery(userId);
        var tickets = await supportTicketQueryService.Handle(query);
        var resources = tickets.Select(t => SupportTicketResourceFromEntityAssembler.ToResourceFromEntity(t));
        return Ok(resources);
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetSupportTicketsByStatus(string status)
    {
        var query = new GetSupportTicketsByStatusQuery(status);
        var tickets = await supportTicketQueryService.Handle(query);
        var resources = tickets.Select(t => SupportTicketResourceFromEntityAssembler.ToResourceFromEntity(t));
        return Ok(resources);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSupportTicket([FromBody] CreateSupportTicketResource resource)
    {
        var command = CreateSupportTicketCommandFromResourceAssembler.ToCommandFromResource(resource);
        var ticket = await supportTicketCommandService.Handle(command);
        if (ticket is null) return BadRequest();
        var ticketResource = SupportTicketResourceFromEntityAssembler.ToResourceFromEntity(ticket);
        return CreatedAtAction(nameof(GetSupportTicketById), new { id = ticket.Id }, ticketResource);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateSupportTicket(int id, [FromBody] UpdateSupportTicketResource resource)
    {
        var command = UpdateSupportTicketCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var ticket = await supportTicketCommandService.Handle(command);
        if (ticket is null) return NotFound();
        var ticketResource = SupportTicketResourceFromEntityAssembler.ToResourceFromEntity(ticket);
        return Ok(ticketResource);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> PatchSupportTicket(int id, [FromBody] UpdateSupportTicketResource resource)
    {
        var command = UpdateSupportTicketCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var ticket = await supportTicketCommandService.Handle(command);
        if (ticket is null) return NotFound();
        var ticketResource = SupportTicketResourceFromEntityAssembler.ToResourceFromEntity(ticket);
        return Ok(ticketResource);
    }

    [HttpPatch("{id:int}/close")]
    public async Task<IActionResult> CloseSupportTicket(int id)
    {
        var command = new CloseSupportTicketCommand(id);
        var ticket = await supportTicketCommandService.Handle(command);
        if (ticket is null) return NotFound();
        var ticketResource = SupportTicketResourceFromEntityAssembler.ToResourceFromEntity(ticket);
        return Ok(ticketResource);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSupportTicket(int id)
    {
        var command = new DeleteSupportTicketCommand(id);
        var result = await supportTicketCommandService.Handle(command);
        if (!result) return NotFound();
        return NoContent();
    }

    // Ticket Messages
    [HttpGet("{ticketId:int}/messages")]
    public async Task<IActionResult> GetTicketMessages(int ticketId)
    {
        var query = new GetTicketMessagesQuery(ticketId);
        var messages = await ticketMessageQueryService.Handle(query);
        var resources = messages.Select(TicketMessageResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpPost("{ticketId:int}/messages")]
    public async Task<IActionResult> CreateTicketMessage(int ticketId, [FromBody] CreateTicketMessageResource resource)
    {
        var command = CreateTicketMessageCommandFromResourceAssembler.ToCommandFromResource(ticketId, resource);
        var message = await ticketMessageCommandService.Handle(command);
        if (message is null) return BadRequest();
        var messageResource = TicketMessageResourceFromEntityAssembler.ToResourceFromEntity(message);
        return CreatedAtAction(nameof(GetTicketMessages), new { ticketId }, messageResource);
    }
}
