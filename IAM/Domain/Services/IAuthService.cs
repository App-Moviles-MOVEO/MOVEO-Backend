using Moveo_backend.IAM.Domain.Model.Commands;
using Moveo_backend.IAM.Interfaces.REST.Resources;

namespace Moveo_backend.IAM.Domain.Services;

public interface IAuthService
{
    Task<AuthenticatedUserResource?> LoginAsync(LoginCommand command);
    Task<AuthenticatedUserResource?> RegisterAsync(RegisterCommand command);
    Task<AuthenticatedUserResource?> GetCurrentUserAsync(int userId);
    Task<bool> ChangePasswordAsync(int userId, AuthChangePasswordCommand command);
}
