using System.Text.Json.Serialization;

namespace PostProxy.Models;

public record ApiKey
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("profile_group_id")]
    public string? ProfileGroupId { get; init; }

    [JsonPropertyName("sandbox")]
    public bool Sandbox { get; init; }

    /// <summary>Only returned by <c>ApiKeys.CreateAsync</c> — store it, it can't be read again.</summary>
    [JsonPropertyName("token")]
    public string? Token { get; init; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }
}
