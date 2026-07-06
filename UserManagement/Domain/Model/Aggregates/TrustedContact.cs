namespace Moveo_backend.UserManagement.Domain.Model.Aggregates;

/// <summary>
/// Contacto de confianza de un usuario (US10). Se comparte a través de dispositivos y puede
/// notificarse ante una emergencia (US08). Persistido server-side en lugar de solo local.
/// </summary>
public class TrustedContact
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string? Relationship { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    protected TrustedContact() { }

    public TrustedContact(int userId, string name, string phone, string? relationship)
    {
        UserId = userId;
        Name = name;
        Phone = phone;
        Relationship = relationship;
        CreatedAt = DateTime.UtcNow;
    }
}
