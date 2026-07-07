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

    /// <summary>
    /// Grupo de comunidad de carpool según el dominio del correo: los usuarios
    /// institucionales (@upc.edu.pe) forman una comunidad; el resto ("public",
    /// correos .com y demás) forma otra. Solo se ven/reservan rutas del mismo grupo.
    /// </summary>
    public static string GroupOf(string? email) =>
        IsInstitutional(email) ? Domain : "public";

    /// <summary>True si ambos correos pertenecen a la misma comunidad de carpool.</summary>
    public static bool SameGroup(string? a, string? b) =>
        string.Equals(GroupOf(a), GroupOf(b), StringComparison.OrdinalIgnoreCase);
}
