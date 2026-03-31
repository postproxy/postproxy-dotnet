using System.Text.Json.Serialization;

namespace PostProxy.Models;

public record Comment
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("external_id")]
    public string? ExternalId { get; init; }

    [JsonPropertyName("body")]
    public string? Body { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("author_username")]
    public string? AuthorUsername { get; init; }

    [JsonPropertyName("author_avatar_url")]
    public string? AuthorAvatarUrl { get; init; }

    [JsonPropertyName("author_external_id")]
    public string? AuthorExternalId { get; init; }

    [JsonPropertyName("parent_external_id")]
    public string? ParentExternalId { get; init; }

    [JsonPropertyName("like_count")]
    public int LikeCount { get; init; }

    [JsonPropertyName("is_hidden")]
    public bool IsHidden { get; init; }

    [JsonPropertyName("permalink")]
    public string? Permalink { get; init; }

    [JsonPropertyName("platform_data")]
    public object? PlatformData { get; init; }

    [JsonPropertyName("posted_at")]
    public DateTimeOffset? PostedAt { get; init; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }

    [JsonPropertyName("replies")]
    public IReadOnlyList<Comment>? Replies { get; init; }
}

public record AcceptedResponse
{
    [JsonPropertyName("accepted")]
    public bool Accepted { get; init; }
}
