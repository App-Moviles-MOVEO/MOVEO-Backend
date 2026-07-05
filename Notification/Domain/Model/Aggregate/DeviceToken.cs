namespace Moveo_backend.Notification.Domain.Model.Aggregate;

/// <summary>
/// Token de dispositivo (FCM/APNs) registrado por un usuario para recibir push notifications.
/// Un usuario puede tener varios dispositivos. El token es único: si se re-registra, se reactiva.
/// </summary>
public class DeviceToken
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public string Platform { get; private set; } = "android"; // "android" | "ios" | "web"
    public bool Active { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    protected DeviceToken() { }

    public DeviceToken(int userId, string token, string platform)
    {
        UserId = userId;
        Token = token;
        Platform = string.IsNullOrWhiteSpace(platform) ? "android" : platform;
        Active = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reactivate(int userId, string platform)
    {
        UserId = userId;
        Platform = string.IsNullOrWhiteSpace(platform) ? Platform : platform;
        Active = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Active = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
