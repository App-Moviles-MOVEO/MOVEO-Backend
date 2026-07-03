using Moveo_backend.Adventure.Application.Internal;
using Moveo_backend.Adventure.Domain.Model.Commands;

namespace Moveo_backend.Adventure.Domain.Services;

public interface IRoutePassengerCommandService
{
    Task<RoutePassengerView> Handle(RequestRouteSeatCommand command);
    Task<RoutePassengerView> Handle(AcceptRoutePassengerCommand command);
    Task<RoutePassengerView> Handle(RejectRoutePassengerCommand command);
    Task<RoutePassengerView> Handle(RemoveRoutePassengerCommand command);
}
