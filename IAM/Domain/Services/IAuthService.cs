using Moveo_backend.IAM.Domain.Model.Commands;
using Moveo_backend.IAM.Interfaces.REST.Resources;

namespace Moveo_backend.IAM.Domain.Services;

public interface IAuthService
{
    Task<AuthenticatedUserResource?> LoginAsync(LoginCommand command);
    Task<AuthenticatedUserResource?> RegisterAsync(RegisterCommand command);
    Task<AuthenticatedUserResource?> GetCurrentUserAsync(int userId);
    Task<bool> ChangePasswordAsync(int userId, AuthChangePasswordCommand command);

    /// <summary>
    /// Genera un token de reseteo si el email existe. Devuelve el token SOLO en entornos
    /// de desarrollo (para probar sin servidor de correo); en producción devuelve null.
    /// El endpoint responde siempre 200 para no revelar si el email existe.
    /// </summary>
    Task<string?> ForgotPasswordAsync(ForgotPasswordCommand command);

    /// <summary>
    /// Cambia la contraseña usando un token válido y no expirado. false si es inválido/expirado.
    /// </summary>
    Task<bool> ResetPasswordAsync(ResetPasswordCommand command);

    /// <summary>
    /// Registra la subida de documentos KYC. Devuelve el nuevo estado ("pending") o null si el usuario no existe.
    /// </summary>
    Task<string?> SubmitKycAsync(int userId, string? dniFrontUrl, string? dniBackUrl, string? selfieUrl);

    /// <summary>Lista las solicitudes KYC en revisión (estado "pending") para el panel admin.</summary>
    Task<IEnumerable<KycReviewItemResource>> GetPendingKycAsync();

    /// <summary>Resuelve una solicitud KYC: approve o reject (con motivo). Devuelve el nuevo estado o null.</summary>
    Task<string?> ReviewKycAsync(int userId, bool approve, string? rejectionReason);
}
