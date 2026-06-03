using Moveo_backend.UserManagement.Domain.Model.Commands;
using Moveo_backend.UserManagement.Interfaces.REST.Resources;

namespace Moveo_backend.UserManagement.Domain.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserResource>> GetAllUsersAsync();
        Task<UserResource> CreateUserAsync(CreateUserCommand command);
        Task<UserResource?> UpdateUserAsync(int id, UpdateUserCommand command);
        Task<bool> ChangePasswordAsync(int id, ChangePasswordCommand command);
        Task<bool> ChangeUserRoleAsync(int id, ChangeUserRoleCommand command);
        Task<bool> DeleteUserAsync(int id);
    }
}