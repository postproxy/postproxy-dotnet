using System.Text.Json.Serialization;

namespace PostProxy.Models;

/// <summary>
/// Which Meta app controls a Facebook / Instagram conversation. Both fields are
/// <c>null</c> when the conversation is idle.
/// </summary>
public record ThreadOwner
{
    [JsonPropertyName("app_id")]
    public string? AppId { get; init; }

    [JsonPropertyName("expires_at")]
    public DateTimeOffset? ExpiresAt { get; init; }
}
