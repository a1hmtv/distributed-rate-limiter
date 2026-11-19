using System.Text.Json.Serialization;

namespace RateLimiter.Reader.Domain;

public class UserRequestEvent
{
    [JsonPropertyName("user_id")]
    public int UserId { get; init; }
    
    [JsonPropertyName("endpoint")]
    public string Endpoint { get; init; } = String.Empty;
}