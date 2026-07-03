using Moveo_backend.Adventure.Application.Internal;
using Moveo_backend.Adventure.Interfaces.REST.Resources;

namespace Moveo_backend.Adventure.Interfaces.REST.Transform;

public static class RoutePassengerResourceFromViewAssembler
{
    public static RoutePassengerResource ToResourceFromView(RoutePassengerView view)
    {
        return new RoutePassengerResource(
            view.Passenger.Id,
            view.Passenger.PassengerId,
            view.FullName,
            view.AvatarUrl,
            view.Reputation,
            view.Verified ? "VERIFIED" : "UNVERIFIED",
            view.Passenger.Status,
            view.Passenger.Seats,
            view.Passenger.RequestedAt
        );
    }
}
