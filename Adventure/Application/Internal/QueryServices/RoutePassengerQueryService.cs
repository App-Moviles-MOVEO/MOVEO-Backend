using Moveo_backend.Adventure.Domain.Model;
using Moveo_backend.Adventure.Domain.Model.Aggregate;
using Moveo_backend.Adventure.Domain.Model.Queries;
using Moveo_backend.Adventure.Domain.Repositories;
using Moveo_backend.Adventure.Domain.Services;
using Moveo_backend.UserManagement.Domain.Repositories;
using Moveo_backend.UserReview.Domain.Repositories;

namespace Moveo_backend.Adventure.Application.Internal.QueryServices;

public class RoutePassengerQueryService(
    IRoutePassengerRepository passengerRepository,
    IAdventureRouteRepository routeRepository,
    IUserRepository userRepository,
    IUserReviewRepository userReviewRepository) : IRoutePassengerQueryService
{
    public async Task<(AdventureRoute Route, List<RoutePassengerView> Passengers)> Handle(GetRoutePassengersQuery query)
    {
        var route = await routeRepository.FindByIdAsync(query.RouteId)
                    ?? throw CarpoolException.RouteNotFound();
        if (route.OwnerId != query.OwnerId)
            throw CarpoolException.NotRouteOwner();

        var passengers = await GetPassengersForRoute(query.RouteId);
        return (route, passengers);
    }

    public async Task<List<RoutePassengerView>> GetPassengersForRoute(int routeId)
    {
        var requests = (await passengerRepository.FindByRouteIdAsync(routeId))
            .Where(p => p.IsActive)
            .ToList();
        if (requests.Count == 0) return new List<RoutePassengerView>();

        // Reputaciones de todos los pasajeros involucrados (evita N+1 trayendo por usuario único).
        var passengerIds = requests.Select(r => r.PassengerId).Distinct().ToList();
        var reputationByUser = new Dictionary<int, double>();
        var userById = new Dictionary<int, UserManagement.Domain.Model.Aggregates.User>();

        foreach (var id in passengerIds)
        {
            var user = await userRepository.FindByIdAsync(id);
            if (user is not null) userById[id] = user;

            var reviews = (await userReviewRepository.FindByReviewedUserIdAsync(id)).ToList();
            reputationByUser[id] = reviews.Count == 0 ? 0 : Math.Round(reviews.Average(r => r.Rating), 2);
        }

        return requests.Select(p =>
        {
            userById.TryGetValue(p.PassengerId, out var user);
            reputationByUser.TryGetValue(p.PassengerId, out var reputation);
            return new RoutePassengerView(
                p,
                user?.FullName ?? string.Empty,
                user?.Avatar,
                reputation,
                user?.DniVerified ?? false);
        }).ToList();
    }
}
