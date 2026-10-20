using System.Text.Json.Serialization;

namespace PostProxy.Models;

/// <summary>
/// A record of one post pull for a profile — the sync fired when the profile
/// connects, the recurring poll, or a backfill.
/// </summary>
public record PostSync
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("profile_id")]
    public string? ProfileId { get; init; }

    [JsonPropertyName("placement_id")]
    public string? PlacementId { get; init; }

    /// <summary>Always <c>posts</c> today.</summary>
    [JsonPropertyName("kind")]
    public string? Kind { get; init; }

    [JsonPropertyName("trigger")]
    public PostSyncTrigger Trigger { get; init; }

    [JsonPropertyName("status")]
    public PostSyncStatus Status { get; init; }

    [JsonPropertyName("started_at")]
    public DateTimeOffset? StartedAt { get; init; }

    [JsonPropertyName("completed_at")]
    public DateTimeOffset? CompletedAt { get; init; }

    [JsonPropertyName("posts_seen")]
    public int PostsSeen { get; init; }

    /// <summary>
    /// Posts that were new and got created — lower than <see cref="PostsSeen"/>
    /// whenever the run re-read posts you already have.
    /// </summary>
    [JsonPropertyName("posts_imported")]
    public int PostsImported { get; init; }

    /// <summary>The date floor requested. Null for connect and scheduled runs.</summary>
    [JsonPropertyName("backfill_from")]
    public DateTimeOffset? BackfillFrom { get; init; }

    /// <summary>Publish date of the oldest post the run reached.</summary>
    [JsonPropertyName("oldest_posted_at")]
    public DateTimeOffset? OldestPostedAt { get; init; }

    /// <summary>Platform error message when <see cref="Status"/> is Failed.</summary>
    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// A chat (DM) sync run, started by <c>Profiles.BackfillChatsAsync</c> or on connect.
/// </summary>
public record ChatSync
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("profile_id")]
    public string? ProfileId { get; init; }

    [JsonPropertyName("placement_id")]
    public string? PlacementId { get; init; }

    [JsonPropertyName("kind")]
    public string? Kind { get; init; }

    [JsonPropertyName("trigger")]
    public PostSyncTrigger Trigger { get; init; }

    [JsonPropertyName("status")]
    public PostSyncStatus Status { get; init; }

    [JsonPropertyName("started_at")]
    public DateTimeOffset? StartedAt { get; init; }

    [JsonPropertyName("completed_at")]
    public DateTimeOffset? CompletedAt { get; init; }

    [JsonPropertyName("chats_seen")]
    public int ChatsSeen { get; init; }

    [JsonPropertyName("messages_imported")]
    public int MessagesImported { get; init; }

    [JsonPropertyName("backfill_from")]
    public DateTimeOffset? BackfillFrom { get; init; }

    [JsonPropertyName("oldest_updated_at")]
    public DateTimeOffset? OldestUpdatedAt { get; init; }

    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }
}
