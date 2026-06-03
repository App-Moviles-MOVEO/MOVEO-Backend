namespace Moveo_backend.IAM.Domain.Model.Commands;

public record LoginCommand(
    string Email,
    string Password
);
