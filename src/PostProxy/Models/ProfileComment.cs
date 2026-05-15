using System.Text.Json.Serialization;

namespace PostProxy.Models;

public record ProfileComment
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("external_id")]
    public string? ExternalId { get; init; }

    [JsonPropertyName("parent_external_id")]
    public string? ParentExternalId { get; init; }

    [JsonPropertyName("placement_id")]
    public string? PlacementId { get; init; }

    [JsonPropertyName("body")]
    public string? Body { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("author_username")]
    public string? AuthorUsername { get; init; }

    [JsonPropertyName("author_avatar_url")]
    public string? AuthorAvatarUrl { get; init; }

    [JsonPropertyName("platform_data")]
    public Dictionary<string, object>? PlatformData { get; init; }

    [JsonPropertyName("posted_at")]
    public DateTimeOffset? PostedAt { get; init; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }

    [JsonPropertyName("replies")]
    public IReadOnlyList<ProfileComment>? Replies { get; init; }
}
