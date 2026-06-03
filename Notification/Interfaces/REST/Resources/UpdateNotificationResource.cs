using System.Text.Json.Serialization;

namespace Moveo_backend.Notification.Interfaces.REST.Resources;

public class UpdateNotificationResource
{
    [JsonPropertyName("read")]
    public bool? Read { get; set; }
    
    [JsonPropertyName("readAt")]
    public DateTime? ReadAt { get; set; }
}
