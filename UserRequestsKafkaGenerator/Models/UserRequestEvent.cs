using System.Text.Json.Serialization;

namespace UserRequestsKafkaGenerator.Models;

public class UserRequestEvent
{
    [JsonPropertyName("user_id")]
    public int UserId { get; init; }
    
    [JsonPropertyName("endpoint")]
    public string Endpoint { get; init; } = String.Empty;
}