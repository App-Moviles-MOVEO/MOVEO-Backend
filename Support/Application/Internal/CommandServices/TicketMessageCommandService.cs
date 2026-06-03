using Moveo_backend.Shared.Domain.Repositories;
using Moveo_backend.Support.Domain.Model.Aggregate;
using Moveo_backend.Support.Domain.Model.Commands;
using Moveo_backend.Support.Domain.Repositories;
using Moveo_backend.Support.Domain.Services;

namespace Moveo_backend.Support.Application.Internal.CommandServices;

public class TicketMessageCommandService(
    ITicketMessageRepository ticketMessageRepository,
    ISupportTicketRepository supportTicketRepository,
    IUnitOfWork unitOfWork) : ITicketMessageCommandService
{
    public async Task<TicketMessage?> Handle(CreateTicketMessageCommand command)
    {
        var ticket = await supportTicketRepository.FindByIdAsync(command.TicketId);
        if (ticket is null) return null;

        var message = new TicketMessage(command);
        await ticketMessageRepository.AddAsync(message);
        ticket.AddMessage(message);
        await unitOfWork.CompleteAsync();
        return message;
    }
}
