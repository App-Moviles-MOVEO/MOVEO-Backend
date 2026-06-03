namespace Moveo_backend.IAM.Domain.Model.Commands;

public record AuthChangePasswordCommand(
    string CurrentPassword,
    string NewPassword
);
