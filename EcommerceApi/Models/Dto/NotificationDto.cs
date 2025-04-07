using System.Text.Json.Serialization;

namespace EcommerceApi.Models.Dto;

public class NotificationDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty; // "email" or "sms"
    
    [JsonPropertyName("recipient")]
    public string Recipient { get; set; } = string.Empty;
    
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;
    
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
    
    [JsonPropertyName("orderId")]
    public int OrderId { get; set; }
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
} 