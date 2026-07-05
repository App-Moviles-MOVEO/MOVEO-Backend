using Moveo_backend.Adventure.Domain.Model;
using Moveo_backend.Adventure.Domain.Model.Aggregate;
using Moveo_backend.Adventure.Domain.Model.Commands;
using Moveo_backend.Adventure.Domain.Repositories;
using Moveo_backend.Adventure.Domain.Services;
using Moveo_backend.Notification.Domain.Model.Commands;
using Moveo_backend.Notification.Domain.Services;
using Moveo_backend.Shared.Domain.Repositories;
using Moveo_backend.UserManagement.Domain.Repositories;

namespace Moveo_backend.Adventure.Application.Internal.CommandServices;

public class AdventureRouteCommandService(
    IAdventureRouteRepository adventureRouteRepository,
    IUserRepository userRepository,
    IRoutePassengerRepository passengerRepository,
    INotificationCommandService notificationCommandService,
    IUnitOfWork unitOfWork) : IAdventureRouteCommandService
{
    public async Task<AdventureRoute?> Handle(CreateAdventureRouteCommand command)
    {
        if (await adventureRouteRepository.ExistsByNameAsync(command.Name))
            throw new Exception("Adventure route with this name already exists");

        // US14 — las rutas de carpool (con asientos) solo pueden publicarlas usuarios con
        // correo institucional. Las rutas de aventura clásicas (sin asientos) no aplican.
        var isCarpool = command.SeatsTotal.HasValue || command.DepartureDate.HasValue;
        if (isCarpool)
        {
            var owner = await userRepository.FindByIdAsync(command.OwnerId);
            if (!InstitutionalEmail.IsInstitutional(owner?.Email))
                throw CarpoolException.NotInstitutionalOwner();
        }

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

    public async Task<AdventureRoute?> Handle(BookAdventureRouteSeatCommand command)
    {
        var adventureRoute = await adventureRouteRepository.FindByIdAsync(command.Id);
        if (adventureRoute is null) return null;

        adventureRoute.BookSeats(command.Seats);
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

    public async Task<AdventureRoute> Handle(StartAdventureRouteCommand command)
    {
        var route = await GetOwnedRouteAsync(command.RouteId, command.OwnerId);
        route.Start();
        adventureRouteRepository.Update(route);
        await unitOfWork.CompleteAsync();
        return route;
    }

    public async Task<AdventureRoute> Handle(CompleteAdventureRouteCommand command)
    {
        var route = await GetOwnedRouteAsync(command.RouteId, command.OwnerId);
        route.CompleteRoute();
        adventureRouteRepository.Update(route);
        await unitOfWork.CompleteAsync();
        return route;
    }

    public async Task<AdventureRoute> Handle(CancelAdventureRouteCommand command)
    {
        var route = await GetOwnedRouteAsync(command.RouteId, command.OwnerId);
        route.CancelRoute();
        adventureRouteRepository.Update(route);

        // Notifica a los pasajeros activos (pendientes/confirmados) que la ruta se canceló.
        var passengers = (await passengerRepository.FindByRouteIdAsync(route.Id))
            .Where(p => p.IsActive)
            .ToList();
        foreach (var passenger in passengers)
        {
            await notificationCommandService.Handle(new CreateNotificationCommand(
                passenger.PassengerId, "Ruta cancelada",
                "El conductor canceló la ruta de carpool en la que tenías un asiento.",
                "carpool", route.Id, "adventure_route", null, null, null));
        }

        await unitOfWork.CompleteAsync();
        return route;
    }

    // La ruta debe existir y pertenecer al ownerId indicado.
    private async Task<AdventureRoute> GetOwnedRouteAsync(int routeId, int ownerId)
    {
        var route = await adventureRouteRepository.FindByIdAsync(routeId)
                    ?? throw CarpoolException.RouteNotFound();
        if (route.OwnerId != ownerId)
            throw CarpoolException.NotRouteOwner();
        return route;
    }
}
