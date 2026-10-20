using System.Text.Json.Serialization;

namespace PostProxy.Models;

public record Media
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("status")]
    public MediaStatus? Status { get; init; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; init; }

    [JsonPropertyName("content_type")]
    public string? ContentType { get; init; }

    [JsonPropertyName("source_url")]
    public string? SourceUrl { get; init; }

    [JsonPropertyName("url")]
    public string? Url { get; init; }

    [JsonPropertyName("platforms")]
    public IReadOnlyList<MediaPlatformError>? Platforms { get; init; }
}

public record MediaPlatformError
{
    [JsonPropertyName("platform")]
    public Platform? Platform { get; init; }

    [JsonPropertyName("status")]
    public PlatformPostStatus? Status { get; init; }

    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("error_details")]
    public ErrorDetails? ErrorDetails { get; init; }
}

public record ThreadChild
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("body")]
    public string? Body { get; init; }

    [JsonPropertyName("media")]
    public IReadOnlyList<Media>? Media { get; init; }

    /// <summary>Each thread item is delivered separately; this shows whether it went out.</summary>
    [JsonPropertyName("platforms")]
    public IReadOnlyList<PlatformResult>? Platforms { get; init; }
}

public record Post
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("body")]
    public string? Body { get; init; }

    [JsonPropertyName("status")]
    public PostStatus? Status { get; init; }

    [JsonPropertyName("scheduled_at")]
    public DateTimeOffset? ScheduledAt { get; init; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }

    [JsonPropertyName("draft")]
    public bool? Draft { get; init; }

    /// <summary><c>postproxy</c>, or <c>imported</c> for a post pulled in from the platform.</summary>
    [JsonPropertyName("source")]
    public string? Source { get; init; }

    [JsonPropertyName("media")]
    public IReadOnlyList<Media>? Media { get; init; }

    [JsonPropertyName("platforms")]
    public IReadOnlyList<PlatformResult>? Platforms { get; init; }

    [JsonPropertyName("thread")]
    public IReadOnlyList<ThreadChild>? Thread { get; init; }

    [JsonPropertyName("queue_id")]
    public string? QueueId { get; init; }

    [JsonPropertyName("queue_priority")]
    public string? QueuePriority { get; init; }
}

public record PlatformResult
{
    [JsonPropertyName("platform")]
    public Platform? Platform { get; init; }

    [JsonPropertyName("profile_id")]
    public string? ProfileId { get; init; }

    [JsonPropertyName("profile_name")]
    public string? ProfileName { get; init; }

    [JsonPropertyName("status")]
    public PlatformPostStatus? Status { get; init; }

    [JsonPropertyName("permalink")]
    public string? Permalink { get; init; }

    [JsonPropertyName("params")]
    public Dictionary<string, object>? Params { get; init; }

    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("error_details")]
    public ErrorDetails? ErrorDetails { get; init; }

    [JsonPropertyName("attempted_at")]
    public DateTimeOffset? AttemptedAt { get; init; }

    [JsonPropertyName("insights")]
    public Insights? Insights { get; init; }

    [JsonPropertyName("attachments")]
    public IReadOnlyList<PlatformAttachment>? Attachments { get; init; }
}

/// <summary>A file PostProxy generated for one platform, e.g. a TikTok cover.</summary>
public record PlatformAttachment
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("content_type")]
    public string? ContentType { get; init; }

    [JsonPropertyName("url")]
    public string? Url { get; init; }
}

public record ErrorDetails
{
    [JsonPropertyName("platform_error_code")]
    public string? PlatformErrorCode { get; init; }

    [JsonPropertyName("platform_error_subcode")]
    public string? PlatformErrorSubcode { get; init; }

    [JsonPropertyName("platform_error_message")]
    public string? PlatformErrorMessage { get; init; }

    [JsonPropertyName("postproxy_note")]
    public string? PostproxyNote { get; init; }
}

public record Insights
{
    [JsonPropertyName("impressions")]
    public int? Impressions { get; init; }

    [JsonPropertyName("on")]
    public string? On { get; init; }
}
