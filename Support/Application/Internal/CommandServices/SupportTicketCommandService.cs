using Moveo_backend.Shared.Domain.Repositories;
using Moveo_backend.Support.Domain.Model.Aggregate;
using Moveo_backend.Support.Domain.Model.Commands;
using Moveo_backend.Support.Domain.Repositories;
using Moveo_backend.Support.Domain.Services;

namespace Moveo_backend.Support.Application.Internal.CommandServices;

public class SupportTicketCommandService(
    ISupportTicketRepository supportTicketRepository,
    IUnitOfWork unitOfWork) : ISupportTicketCommandService
{
    public async Task<SupportTicket?> Handle(CreateSupportTicketCommand command)
    {
        var ticket = new SupportTicket(command);
        await supportTicketRepository.AddAsync(ticket);
        await unitOfWork.CompleteAsync();
        return ticket;
    }

    public async Task<SupportTicket?> Handle(UpdateSupportTicketCommand command)
    {
        var ticket = await supportTicketRepository.FindByIdAsync(command.Id);
        if (ticket is null) return null;

        ticket.Update(command);
        supportTicketRepository.Update(ticket);
        await unitOfWork.CompleteAsync();
        return ticket;
    }

    public async Task<SupportTicket?> Handle(CloseSupportTicketCommand command)
    {
        var ticket = await supportTicketRepository.FindByIdAsync(command.Id);
        if (ticket is null) return null;

        ticket.Close();
        supportTicketRepository.Update(ticket);
        await unitOfWork.CompleteAsync();
        return ticket;
    }

    public async Task<bool> Handle(DeleteSupportTicketCommand command)
    {
        var ticket = await supportTicketRepository.FindByIdAsync(command.Id);
        if (ticket is null) return false;

        supportTicketRepository.Remove(ticket);
        await unitOfWork.CompleteAsync();
        return true;
    }
}
