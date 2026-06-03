using Moveo_backend.UserManagement.Domain.Model.Aggregates;
using Moveo_backend.UserManagement.Domain.Model.Queries;
using Moveo_backend.UserManagement.Domain.Repositories;
using Moveo_backend.UserManagement.Domain.Services;

namespace Moveo_backend.UserManagement.Application.QueryServices;

public class UserQueryService : IUserQueryService
{
    private readonly IUserRepository _userRepository;

    public UserQueryService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<User>> Handle(GetAllUsersQuery query)
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<User?> Handle(GetUserByIdQuery query)
    {
        return await _userRepository.FindByIdAsync(query.Id);
    }
    
    public async Task<User?> Handle(GetUsersByRoleQuery query)
    {
        return await _userRepository.FindByRoleAsync(query.Role);
    }
    
    public async Task<User?> Handle(GetUserByEmailQuery query)
    {
        return await _userRepository.FindByEmailAsync(query.Email);
    }
}