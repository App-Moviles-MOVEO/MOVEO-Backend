using System.Text.Json.Serialization;

namespace Moveo_backend.Notification.Interfaces.REST.Resources;

public class CreateNotificationResource
{
    [JsonPropertyName("userId")]
    public int UserId { get; set; }
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("body")]
    public string? Body { get; set; }
    
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("relatedEntityId")]
    public int? RelatedEntityId { get; set; }
    
    [JsonPropertyName("relatedId")]
    public int? RelatedId { get; set; }
    
    [JsonPropertyName("relatedEntityType")]
    public string? RelatedEntityType { get; set; }
    
    [JsonPropertyName("relatedType")]
    public string? RelatedType { get; set; }
    
    [JsonPropertyName("actionUrl")]
    public string? ActionUrl { get; set; }
    
    [JsonPropertyName("actionLabel")]
    public string? ActionLabel { get; set; }
    
    [JsonPropertyName("metadata")]
    public object? Metadata { get; set; }
    
    // Helper property to get body from either field
    public string GetBody() => Body ?? Message ?? string.Empty;
    
    // Helper to get related entity id from either field
    public int? GetRelatedEntityId() => RelatedEntityId ?? RelatedId;
    
    // Helper to get related entity type from either field
    public string? GetRelatedEntityType() => RelatedEntityType ?? RelatedType;
    
    // Helper to serialize metadata to JSON
    public string? GetMetadataJson() => Metadata != null 
        ? System.Text.Json.JsonSerializer.Serialize(Metadata) 
        : null;
}
