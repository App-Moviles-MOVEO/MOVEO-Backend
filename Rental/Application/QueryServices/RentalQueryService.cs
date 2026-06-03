using Moveo_backend.Rental.Domain.Model.Aggregates;
using Moveo_backend.Rental.Domain.Services;

namespace Moveo_backend.Rental.Application.QueryServices;

public class RentalQueryService
{
    private readonly IRentalService _rentalService;

    public RentalQueryService(IRentalService rentalService)
    {
        _rentalService = rentalService;
    }

    public Task<IEnumerable<Domain.Model.Aggregates.Rental>> GetAllAsync() => _rentalService.GetAllAsync();
    public Task<Domain.Model.Aggregates.Rental?> GetByIdAsync(int id) => _rentalService.GetByIdAsync(id);
    public Task<IEnumerable<Domain.Model.Aggregates.Rental>> GetByUserIdAsync(int userId) => _rentalService.GetByUserIdAsync(userId);
    public Task<IEnumerable<Domain.Model.Aggregates.Rental>> GetActiveAsync() => _rentalService.GetActiveAsync();
}