using Moveo_backend.Rental.Domain.Model.Aggregates;
using Moveo_backend.Rental.Domain.Model.Commands;
namespace Moveo_backend.Rental.Domain.Services;

public interface IRentalCommandService
{
    Task<Model.Aggregates.Rental> CreateRentalAsync(CreateRentalCommand command);
    Task<Model.Aggregates.Rental?> UpdateRentalAsync(UpdateRentalCommand command);
    Task<bool> CancelRentalAsync(CancelRentalCommand command);
    Task<bool> FinishRentalAsync(FinishRentalCommand command);
}