using Moveo_backend.Adventure.Domain.Model;
using Moveo_backend.Adventure.Domain.Model.Aggregate;
using Moveo_backend.Adventure.Domain.Model.Commands;
using Moveo_backend.Adventure.Domain.Repositories;
using Moveo_backend.Adventure.Domain.Services;
using Moveo_backend.Notification.Domain.Model.Commands;
using Moveo_backend.Notification.Domain.Services;
using Moveo_backend.Payment.Domain.Model.Commands;
using Moveo_backend.Payment.Domain.Services;
using Moveo_backend.Shared.Domain.Repositories;
using Moveo_backend.UserManagement.Domain.Repositories;
using Moveo_backend.UserReview.Domain.Repositories;

namespace Moveo_backend.Adventure.Application.Internal.CommandServices;

public class RoutePassengerCommandService(
    IRoutePassengerRepository passengerRepository,
    IAdventureRouteRepository routeRepository,
    IUserRepository userRepository,
    IUserReviewRepository userReviewRepository,
    INotificationCommandService notificationCommandService,
    IPaymentCommandService paymentCommandService,
    IUnitOfWork unitOfWork) : IRoutePassengerCommandService
{
    public async Task<RoutePassengerView> Handle(RequestRouteSeatCommand command)
    {
        var route = await routeRepository.FindByIdAsync(command.RouteId)
                    ?? throw CarpoolException.RouteNotFound();

        if (route.Status != "active")
            throw CarpoolException.RouteNotActive();

        // Cupo nominal: con bloqueo en CONFIRMED, SeatsAvailable refleja lo ya confirmado.
        if (route.SeatsAvailable is null || route.SeatsAvailable < command.Seats)
            throw CarpoolException.NoSeatsAvailable();

        // No permitir solicitud duplicada activa del mismo pasajero.
        var existing = await passengerRepository.FindActiveByRouteAndPassengerAsync(command.RouteId, command.PassengerId);
        if (existing is not null)
            throw CarpoolException.AlreadyRequested();

        // US14/US11 — validaciones de elegibilidad que necesitan datos del usuario.
        if (!string.IsNullOrWhiteSpace(route.Community) || route.OnlyWomen)
        {
            var passengerUser = await userRepository.FindByIdAsync(command.PassengerId);

            // Comunidad institucional: el pasajero también debe tener correo institucional.
            if (!string.IsNullOrWhiteSpace(route.Community)
                && !InstitutionalEmail.IsInstitutional(passengerUser?.Email))
                throw CarpoolException.NotInstitutionalPassenger();

            // US11 — ruta solo para mujeres: el pasajero debe tener gender = female.
            if (route.OnlyWomen
                && !string.Equals(passengerUser?.Gender, "female", StringComparison.OrdinalIgnoreCase))
                throw CarpoolException.WomenOnlyRoute();
        }

        var passenger = new RoutePassenger(command.RouteId, command.PassengerId, command.Seats);
        await passengerRepository.AddAsync(passenger);
        await unitOfWork.CompleteAsync();

        // Notificar al owner que llegó una nueva solicitud.
        await NotifyAsync(route.OwnerId, "Nueva solicitud de asiento",
            "Un pasajero solicitó un asiento en tu ruta de carpool.", route.Id);

        return await BuildViewAsync(passenger);
    }

    public async Task<RoutePassengerView> Handle(AcceptRoutePassengerCommand command)
    {
        var route = await GetOwnedRouteAsync(command.RouteId, command.OwnerId);

        var passenger = await passengerRepository.FindActiveByRouteAndPassengerAsync(command.RouteId, command.PassengerId)
                        ?? throw CarpoolException.PassengerNotFound();

        if (passenger.Status == RoutePassenger.StatusConfirmed)
            return await BuildViewAsync(passenger); // idempotente

        if (route.SeatsAvailable is null || route.SeatsAvailable < passenger.Seats)
            throw CarpoolException.NoSeatsAvailable();

        route.BookSeats(passenger.Seats); // descuenta y marca "full" si llega a 0
        passenger.Confirm();

        routeRepository.Update(route);
        passengerRepository.Update(passenger);
        await unitOfWork.CompleteAsync();

        // US23 — al confirmar, genera el cobro de la cuota del asiento (pendiente de pago).
        // El pasajero luego lo completa con PATCH /payments/{id} (o el flujo de pago del cliente).
        if (route.PricePerSeat is > 0)
        {
            var amount = route.PricePerSeat.Value * passenger.Seats;
            await paymentCommandService.Handle(new CreatePaymentCommand(
                PayerId: passenger.PassengerId,
                RecipientId: route.OwnerId,
                RentalId: 0,
                Amount: amount,
                Currency: "PEN",
                Method: "yape",
                Type: "carpool_seat",
                Description: $"Cuota de asiento en ruta de carpool #{route.Id}"));
        }

        await NotifyAsync(passenger.PassengerId, "Solicitud aceptada",
            "Tu solicitud de asiento fue aceptada por el conductor.", route.Id);

        return await BuildViewAsync(passenger);
    }

    public async Task<RoutePassengerView> Handle(RejectRoutePassengerCommand command)
    {
        var route = await GetOwnedRouteAsync(command.RouteId, command.OwnerId);

        var passenger = await passengerRepository.FindActiveByRouteAndPassengerAsync(command.RouteId, command.PassengerId)
                        ?? throw CarpoolException.PassengerNotFound();

        if (passenger.Status == RoutePassenger.StatusConfirmed)
            route.ReleaseSeats(passenger.Seats); // libera lo que se había confirmado

        passenger.Reject();

        routeRepository.Update(route);
        passengerRepository.Update(passenger);
        await unitOfWork.CompleteAsync();

        await NotifyAsync(passenger.PassengerId, "Solicitud rechazada",
            "Tu solicitud de asiento fue rechazada por el conductor.", route.Id);

        return await BuildViewAsync(passenger);
    }

    public async Task<RoutePassengerView> Handle(RemoveRoutePassengerCommand command)
    {
        var route = await GetOwnedRouteAsync(command.RouteId, command.OwnerId);

        var passenger = await passengerRepository.FindActiveByRouteAndPassengerAsync(command.RouteId, command.PassengerId)
                        ?? throw CarpoolException.PassengerNotFound();

        if (passenger.Status == RoutePassenger.StatusConfirmed)
            route.ReleaseSeats(passenger.Seats);

        passenger.Cancel();

        routeRepository.Update(route);
        passengerRepository.Update(passenger);
        await unitOfWork.CompleteAsync();

        await NotifyAsync(passenger.PassengerId, "Te quitaron de la ruta",
            "El conductor te quitó de la ruta de carpool.", route.Id);

        return await BuildViewAsync(passenger);
    }

    private async Task<AdventureRoute> GetOwnedRouteAsync(int routeId, int ownerId)
    {
        var route = await routeRepository.FindByIdAsync(routeId)
                    ?? throw CarpoolException.RouteNotFound();
        if (route.OwnerId != ownerId)
            throw CarpoolException.NotRouteOwner();
        return route;
    }

    private async Task NotifyAsync(int userId, string title, string body, int routeId)
    {
        await notificationCommandService.Handle(new CreateNotificationCommand(
            userId, title, body, "carpool", routeId, "adventure_route", null, null, null));
    }

    private async Task<RoutePassengerView> BuildViewAsync(RoutePassenger passenger)
    {
        var user = await userRepository.FindByIdAsync(passenger.PassengerId);
        var reputation = await ComputeReputationAsync(passenger.PassengerId);
        var fullName = user?.FullName ?? string.Empty;
        var verified = user?.DniVerified ?? false;
        return new RoutePassengerView(passenger, fullName, user?.Avatar, reputation, verified);
    }

    private async Task<double> ComputeReputationAsync(int userId)
    {
        var reviews = (await userReviewRepository.FindByReviewedUserIdAsync(userId))
            .Where(r => r.CountsForReputation)
            .ToList();
        if (reviews.Count == 0) return 0;
        return Math.Round(reviews.Average(r => r.Rating), 2);
    }
}
