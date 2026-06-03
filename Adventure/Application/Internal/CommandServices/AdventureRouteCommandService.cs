using Moveo_backend.Adventure.Domain.Model.Aggregate;
using Moveo_backend.Adventure.Domain.Model.Commands;
using Moveo_backend.Adventure.Domain.Repositories;
using Moveo_backend.Adventure.Domain.Services;
using Moveo_backend.Shared.Domain.Repositories;

namespace Moveo_backend.Adventure.Application.Internal.CommandServices;

public class AdventureRouteCommandService(
    IAdventureRouteRepository adventureRouteRepository,
    IUnitOfWork unitOfWork) : IAdventureRouteCommandService
{
    public async Task<AdventureRoute?> Handle(CreateAdventureRouteCommand command)
    {
        if (await adventureRouteRepository.ExistsByNameAsync(command.Name))
            throw new Exception("Adventure route with this name already exists");

        var adventureRoute = new AdventureRoute(command);
        await adventureRouteRepository.AddAsync(adventureRoute);
        await unitOfWork.CompleteAsync();
        return adventureRoute;
    }

    public async Task<AdventureRoute?> Handle(UpdateAdventureRouteCommand command)
    {
        var adventureRoute = await adventureRouteRepository.FindByIdAsync(command.Id);
        if (adventureRoute is null) return null;

        adventureRoute.Update(command);
        adventureRouteRepository.Update(adventureRoute);
        await unitOfWork.CompleteAsync();
        return adventureRoute;
    }

    public async Task<bool> Handle(DeleteAdventureRouteCommand command)
    {
        var adventureRoute = await adventureRouteRepository.FindByIdAsync(command.Id);
        if (adventureRoute is null) return false;

        adventureRouteRepository.Remove(adventureRoute);
        await unitOfWork.CompleteAsync();
        return true;
    }
}
