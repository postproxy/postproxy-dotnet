using System.Text.Json;
using System.Text.Json.Serialization;

namespace PostProxy.Models;

/// <summary>
/// A comment from <c>Comments.ListAllAsync</c>. Flat: replies are their own
/// entries linked to their parent by <see cref="ParentExternalId"/> rather than
/// nested under a replies list.
/// </summary>
public record BulkComment
{
    /// <summary>ID of the post the comment belongs to.</summary>
    [JsonPropertyName("post_id")]
    public required string PostId { get; init; }

    /// <summary>ID of the profile the comment was left on.</summary>
    [JsonPropertyName("profile_id")]
    public required string ProfileId { get; init; }

    [JsonPropertyName("platform")]
    public Platform Platform { get; init; }

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

    [JsonPropertyName("metadata")]
    public IReadOnlyDictionary<string, JsonElement>? Metadata { get; init; }

    [JsonPropertyName("attachments")]
    public IReadOnlyList<Attachment>? Attachments { get; init; }

    [JsonPropertyName("external_edited_at")]
    public DateTimeOffset? ExternalEditedAt { get; init; }

    [JsonPropertyName("posted_at")]
    public DateTimeOffset? PostedAt { get; init; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }
}
