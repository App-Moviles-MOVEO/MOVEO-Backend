using Moveo_backend.Rental.Domain.Model.Aggregates;
using Moveo_backend.Rental.Domain.Model.Commands;
using Moveo_backend.Rental.Domain.Services;

namespace Moveo_backend.Rental.Application.CommandServices;

public class RentalCommandService
{
    private readonly IRentalService _rentalService;

    public RentalCommandService(IRentalService rentalService)
    {
        _rentalService = rentalService;
    }

    public Task<Domain.Model.Aggregates.Rental> Handle(CreateRentalCommand command)
        => _rentalService.CreateAsync(command);

    public Task<Domain.Model.Aggregates.Rental?> Handle(UpdateRentalCommand command)
        => _rentalService.UpdateAsync(command);

    public Task<Domain.Model.Aggregates.Rental?> Handle(PatchRentalCommand command)
        => _rentalService.PatchAsync(command);

    public Task<bool> Handle(DeleteRentalCommand command)
        => _rentalService.DeleteAsync(command.Id);
}