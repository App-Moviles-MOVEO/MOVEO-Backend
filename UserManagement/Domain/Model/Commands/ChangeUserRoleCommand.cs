namespace Moveo_backend.UserManagement.Domain.Model.Commands;

public record ChangeUserRoleCommand(
    int UserId,
    string NewRole
);