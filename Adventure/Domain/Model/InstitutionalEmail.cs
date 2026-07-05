namespace Moveo_backend.Adventure.Domain.Model;

/// <summary>
/// Regla de correo institucional para carpooling (US14). El dominio permitido es configurable
/// vía la variable de entorno INSTITUTIONAL_EMAIL_DOMAIN (por defecto "upc.edu.pe").
/// </summary>
public static class InstitutionalEmail
{
    public static string Domain =>
        Environment.GetEnvironmentVariable("INSTITUTIONAL_EMAIL_DOMAIN")?.Trim().TrimStart('@')
            is { Length: > 0 } d
            ? d
            : "upc.edu.pe";

    public static bool IsInstitutional(string? email) =>
        !string.IsNullOrWhiteSpace(email) &&
        email.Trim().EndsWith("@" + Domain, StringComparison.OrdinalIgnoreCase);
}
