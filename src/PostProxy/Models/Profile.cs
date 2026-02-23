using System.Text.Json.Serialization;

namespace PostProxy.Models;

public record Profile
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("status")]
    public ProfileStatus? Status { get; init; }

    [JsonPropertyName("platform")]
    public Platform? Platform { get; init; }

    [JsonPropertyName("profile_group_id")]
    public string? ProfileGroupId { get; init; }

    [JsonPropertyName("expires_at")]
    public DateTimeOffset? ExpiresAt { get; init; }

    [JsonPropertyName("post_count")]
    public int? PostCount { get; init; }
}
