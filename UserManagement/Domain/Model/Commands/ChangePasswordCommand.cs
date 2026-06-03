namespace Moveo_backend.UserManagement.Domain.Model.Commands;

public record ChangePasswordCommand(
    int UserId,
    string NewPassword
);