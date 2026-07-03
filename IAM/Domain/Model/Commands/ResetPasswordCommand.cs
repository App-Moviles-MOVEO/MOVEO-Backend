namespace Moveo_backend.IAM.Domain.Model.Commands;

public record ResetPasswordCommand(string Token, string NewPassword);
