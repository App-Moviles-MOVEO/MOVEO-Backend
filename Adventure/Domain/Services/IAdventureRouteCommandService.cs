using Moveo_backend.Adventure.Domain.Model.Aggregate;
using Moveo_backend.Adventure.Domain.Model.Commands;

namespace Moveo_backend.Adventure.Domain.Services;

public interface IAdventureRouteCommandService
{
    Task<AdventureRoute?> Handle(CreateAdventureRouteCommand command);
    Task<AdventureRoute?> Handle(UpdateAdventureRouteCommand command);
    Task<bool> Handle(DeleteAdventureRouteCommand command);
}
