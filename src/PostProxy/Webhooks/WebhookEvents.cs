using System.Text.Json;
using System.Text.Json.Serialization;
using PostProxy.Models;
using PostProxy.Exceptions;

namespace PostProxy.Webhooks;

public class WebhookParseException : Exception
{
    public WebhookParseException(string message) : base(message) { }
    public WebhookParseException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Webhook envelope. <see cref="Data"/> is held as a <see cref="JsonElement"/>
/// so callers can decode it into the right typed payload via the static
/// <see cref="WebhookEvents"/> helpers.
/// </summary>
public record WebhookEvent
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("type")]
    public WebhookEventType Type { get; init; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }

    [JsonPropertyName("data")]
    public JsonElement Data { get; init; }
}

public record PostProcessedData
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("body")] public string? Body { get; init; }
    [JsonPropertyName("status")] public PostStatus Status { get; init; }
    [JsonPropertyName("scheduled_at")] public DateTimeOffset? ScheduledAt { get; init; }
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; init; }
    [JsonPropertyName("platforms")] public IReadOnlyList<PostProcessedPlatform> Platforms { get; init; } = Array.Empty<PostProcessedPlatform>();
}

public record PostProcessedPlatform
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("platform")] public Platform Platform { get; init; }
    [JsonPropertyName("name")] public string? Name { get; init; }
}

public record PostImportedData
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("body")] public string? Body { get; init; }
    [JsonPropertyName("source")] public string? Source { get; init; }
    [JsonPropertyName("posted_at")] public DateTimeOffset? PostedAt { get; init; }
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; init; }
    [JsonPropertyName("platform")] public Platform Platform { get; init; }
    [JsonPropertyName("profile")] public required ImportedProfile Profile { get; init; }
    [JsonPropertyName("platform_post_id")] public required string PlatformPostId { get; init; }
    [JsonPropertyName("public_id")] public string? PublicId { get; init; }
}

public record ImportedProfile
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("name")] public string? Name { get; init; }
    [JsonPropertyName("platform")] public Platform Platform { get; init; }
}

public record PlatformPostData
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("post_id")] public required string PostId { get; init; }
    [JsonPropertyName("platform")] public Platform Platform { get; init; }
    [JsonPropertyName("profile_id")] public required string ProfileId { get; init; }
    [JsonPropertyName("profile_name")] public string? ProfileName { get; init; }
    [JsonPropertyName("status")] public PlatformPostStatus Status { get; init; }
    [JsonPropertyName("error")] public string? Error { get; init; }
    [JsonPropertyName("error_details")] public ErrorDetails? ErrorDetails { get; init; }
    [JsonPropertyName("platform_id")] public string? PlatformId { get; init; }
    [JsonPropertyName("insights")] public IReadOnlyDictionary<string, JsonElement>? Insights { get; init; }
}

public record ProfileEventData
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("name")] public string? Name { get; init; }
    [JsonPropertyName("platform")] public Platform Platform { get; init; }
    [JsonPropertyName("profile_group_id")] public required string ProfileGroupId { get; init; }
    [JsonPropertyName("status")] public string? Status { get; init; }
    [JsonPropertyName("uid")] public required string Uid { get; init; }
    [JsonPropertyName("username")] public string? Username { get; init; }
}

public record ProfileStatsData
{
    [JsonPropertyName("profile_id")] public required string ProfileId { get; init; }
    [JsonPropertyName("platform")] public Platform Platform { get; init; }
    [JsonPropertyName("placement_id")] public string? PlacementId { get; init; }
    [JsonPropertyName("stats")] public IReadOnlyDictionary<string, JsonElement> Stats { get; init; } =
        new Dictionary<string, JsonElement>();
    [JsonPropertyName("recorded_at")] public DateTimeOffset RecordedAt { get; init; }
}

public record MediaFailedData
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("post_id")] public required string PostId { get; init; }
    [JsonPropertyName("content_type")] public string? ContentType { get; init; }
    [JsonPropertyName("status")] public string? Status { get; init; }
    [JsonPropertyName("error_message")] public string? ErrorMessage { get; init; }
}

public record CommentCreatedData
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("post_id")] public required string PostId { get; init; }
    [JsonPropertyName("platform_post_id")] public string? PlatformPostId { get; init; }
    [JsonPropertyName("platform")] public Platform Platform { get; init; }
    [JsonPropertyName("external_id")] public string? ExternalId { get; init; }
    [JsonPropertyName("parent_external_id")] public string? ParentExternalId { get; init; }
    [JsonPropertyName("body")] public string? Body { get; init; }
    [JsonPropertyName("status")] public string? Status { get; init; }
    [JsonPropertyName("author_external_id")] public string? AuthorExternalId { get; init; }
    [JsonPropertyName("author_name")] public string? AuthorName { get; init; }
    [JsonPropertyName("author_username")] public string? AuthorUsername { get; init; }
    [JsonPropertyName("author_avatar_url")] public string? AuthorAvatarUrl { get; init; }
    [JsonPropertyName("like_count")] public int LikeCount { get; init; }
    [JsonPropertyName("reply_count")] public int ReplyCount { get; init; }
    [JsonPropertyName("is_hidden")] public bool IsHidden { get; init; }
    [JsonPropertyName("permalink")] public string? Permalink { get; init; }
    [JsonPropertyName("platform_data")] public IReadOnlyDictionary<string, JsonElement>? PlatformData { get; init; }
    [JsonPropertyName("posted_at")] public DateTimeOffset? PostedAt { get; init; }
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; init; }
}

public static class WebhookEvents
{
    /// <summary>Parse a raw JSON webhook body and validate the envelope.</summary>
    public static WebhookEvent Parse(string body)
    {
        WebhookEvent? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<WebhookEvent>(body, PostProxyHttpClient.JsonOptions);
        }
        catch (Exception e)
        {
            throw new WebhookParseException("Invalid webhook body: " + e.Message, e);
        }

        if (envelope is null)
            throw new WebhookParseException("Webhook body is empty");

        return envelope;
    }

    public static PostProcessedData AsPostProcessed(WebhookEvent ev) => Decode<PostProcessedData>(ev);
    public static PostImportedData AsPostImported(WebhookEvent ev) => Decode<PostImportedData>(ev);
    public static PlatformPostData AsPlatformPost(WebhookEvent ev) => Decode<PlatformPostData>(ev);
    public static ProfileEventData AsProfileEvent(WebhookEvent ev) => Decode<ProfileEventData>(ev);
    public static ProfileStatsData AsProfileStats(WebhookEvent ev) => Decode<ProfileStatsData>(ev);
    public static MediaFailedData AsMediaFailed(WebhookEvent ev) => Decode<MediaFailedData>(ev);
    public static CommentCreatedData AsCommentCreated(WebhookEvent ev) => Decode<CommentCreatedData>(ev);

    private static T Decode<T>(WebhookEvent ev) where T : class
    {
        try
        {
            return ev.Data.Deserialize<T>(PostProxyHttpClient.JsonOptions)
                ?? throw new WebhookParseException("Failed to decode event data");
        }
        catch (Exception e) when (e is not WebhookParseException)
        {
            throw new WebhookParseException("Failed to decode event data: " + e.Message, e);
        }
    }
}
