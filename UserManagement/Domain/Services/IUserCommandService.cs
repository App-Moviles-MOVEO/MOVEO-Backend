using Moveo_backend.UserManagement.Domain.Model.Aggregates;
using Moveo_backend.UserManagement.Domain.Model.Commands;

namespace Moveo_backend.UserManagement.Domain.Services;

public interface IUserCommandService
{
    Task<User> HandleCreate(CreateUserCommand command);
    Task<User> HandleUpdate(UpdateUserCommand command);
    Task HandleDelete(DeleteUserCommand command);
}