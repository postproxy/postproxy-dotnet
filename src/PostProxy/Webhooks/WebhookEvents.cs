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
    [JsonPropertyName("platform_id")] public string? PlatformId { get; init; }
    [JsonPropertyName("username")] public string? Username { get; init; }
    /// <summary>On <c>profile.disconnected</c>: <c>user</c> or <c>automated</c>.</summary>
    [JsonPropertyName("origin")] public string? Origin { get; init; }
    /// <summary>
    /// On <c>profile.disconnected</c> with origin <c>automated</c>: why PostProxy
    /// deactivated it, e.g. "Twitter access revoked".
    /// </summary>
    [JsonPropertyName("error")] public string? Error { get; init; }
    /// <summary>Raw platform error behind the disconnect, when available.</summary>
    [JsonPropertyName("error_details")] public ErrorDetails? ErrorDetails { get; init; }
}

/// <summary>
/// Payload of the <c>placement.*</c> events. <c>placement.name_status_updated</c> adds
/// <c>decision</c>, <c>requested_name</c>, <c>rejection_reason</c>;
/// <c>placement.quality_updated</c> adds <c>event</c>, <c>current_limit</c>,
/// <c>old_limit</c> — read those from <see cref="Extra"/>.
/// </summary>
public record PlacementEventData
{
    /// <summary>The placement's id on the platform (page id, phone number id, ...).</summary>
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("name")] public string? Name { get; init; }
    [JsonPropertyName("platform")] public Platform Platform { get; init; }
    [JsonPropertyName("profile_group_id")] public string? ProfileGroupId { get; init; }
    [JsonPropertyName("profile_id")] public string? ProfileId { get; init; }
    [JsonPropertyName("profile_name")] public string? ProfileName { get; init; }
    [JsonPropertyName("metadata")] public IReadOnlyDictionary<string, JsonElement>? Metadata { get; init; }
    [JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; init; }
}

/// <summary>Payload of <c>profile.placements_synced</c>.</summary>
public record PlacementsSyncedData : ProfileEventData
{
    [JsonPropertyName("placements_sync_status")] public string? PlacementsSyncStatus { get; init; }
    [JsonPropertyName("placements")] public IReadOnlyList<PlacementEventData> Placements { get; init; } = Array.Empty<PlacementEventData>();
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
    [JsonPropertyName("profile_id")] public string? ProfileId { get; init; }
    [JsonPropertyName("error")] public string? Error { get; init; }
    [JsonPropertyName("error_details")] public ErrorDetails? ErrorDetails { get; init; }
    [JsonPropertyName("external_edited_at")] public DateTimeOffset? ExternalEditedAt { get; init; }
    [JsonPropertyName("posted_at")] public DateTimeOffset? PostedAt { get; init; }
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; init; }
}

public record MessageEventData
{
    [JsonPropertyName("message")] public required Message Message { get; init; }
}

public record ReactionEventData
{
    [JsonPropertyName("message")] public required Message Message { get; init; }
    [JsonPropertyName("sender_external_id")] public required string SenderExternalId { get; init; }
    [JsonPropertyName("action")] public string? Action { get; init; }
    [JsonPropertyName("reaction")] public string? Reaction { get; init; }
    [JsonPropertyName("emoji")] public string? Emoji { get; init; }
    [JsonPropertyName("occurred_at")] public DateTimeOffset? OccurredAt { get; init; }
}

public record ProfileCommentCreatedData
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("profile_id")] public required string ProfileId { get; init; }
    [JsonPropertyName("platform")] public Platform Platform { get; init; }
    [JsonPropertyName("placement_id")] public string? PlacementId { get; init; }
    [JsonPropertyName("external_id")] public string? ExternalId { get; init; }
    [JsonPropertyName("parent_external_id")] public string? ParentExternalId { get; init; }
    [JsonPropertyName("body")] public string? Body { get; init; }
    [JsonPropertyName("status")] public string? Status { get; init; }
    [JsonPropertyName("author_username")] public string? AuthorUsername { get; init; }
    [JsonPropertyName("author_avatar_url")] public string? AuthorAvatarUrl { get; init; }
    [JsonPropertyName("platform_data")] public IReadOnlyDictionary<string, JsonElement>? PlatformData { get; init; }
    [JsonPropertyName("error")] public string? Error { get; init; }
    [JsonPropertyName("error_details")] public ErrorDetails? ErrorDetails { get; init; }
    [JsonPropertyName("permalink")] public string? Permalink { get; init; }
    [JsonPropertyName("posted_at")] public DateTimeOffset? PostedAt { get; init; }
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>The chat carried by <c>referral.received</c> and the <c>chat.thread_control_*</c> events.</summary>
public record WebhookChatData
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("profile_id")] public string? ProfileId { get; init; }
    [JsonPropertyName("platform")] public Platform Platform { get; init; }
    [JsonPropertyName("external_placement_id")] public string? ExternalPlacementId { get; init; }
    [JsonPropertyName("group")] public bool Group { get; init; }
    [JsonPropertyName("participant_external_id")] public string? ParticipantExternalId { get; init; }
    [JsonPropertyName("participant_username")] public string? ParticipantUsername { get; init; }
    [JsonPropertyName("participant_name")] public string? ParticipantName { get; init; }
    [JsonPropertyName("last_inbound_at")] public DateTimeOffset? LastInboundAt { get; init; }
    [JsonPropertyName("last_outbound_at")] public DateTimeOffset? LastOutboundAt { get; init; }
    [JsonPropertyName("last_message_at")] public DateTimeOffset? LastMessageAt { get; init; }
}

public record ReferralEventData
{
    [JsonPropertyName("chat")] public required WebhookChatData Chat { get; init; }
    [JsonPropertyName("ref")] public string? Ref { get; init; }
    [JsonPropertyName("source")] public string? Source { get; init; }
    [JsonPropertyName("type")] public string? Type { get; init; }
    /// <summary>WhatsApp click-to-WhatsApp ad click id.</summary>
    [JsonPropertyName("ctwa_clid")] public string? CtwaClid { get; init; }
    [JsonPropertyName("occurred_at")] public DateTimeOffset? OccurredAt { get; init; }
}

/// <summary>Payload of <c>chat.thread_control_changed</c>: a Meta app passed or took a Facebook / Instagram conversation.</summary>
public record ThreadControlChangedEventData
{
    [JsonPropertyName("chat")] public required WebhookChatData Chat { get; init; }
    /// <summary>Absent when the conversation was idle.</summary>
    [JsonPropertyName("previous_owner_app_id")] public string? PreviousOwnerAppId { get; init; }
    [JsonPropertyName("new_owner_app_id")] public string? NewOwnerAppId { get; init; }
    [JsonPropertyName("metadata")] public string? Metadata { get; init; }
    [JsonPropertyName("occurred_at")] public DateTimeOffset? OccurredAt { get; init; }
}

/// <summary>Payload of <c>chat.thread_control_requested</c>: another Meta app asked PostProxy to pass a conversation.</summary>
public record ThreadControlRequestedEventData
{
    [JsonPropertyName("chat")] public required WebhookChatData Chat { get; init; }
    [JsonPropertyName("requested_owner_app_id")] public string? RequestedOwnerAppId { get; init; }
    [JsonPropertyName("metadata")] public string? Metadata { get; init; }
    [JsonPropertyName("occurred_at")] public DateTimeOffset? OccurredAt { get; init; }
}

/// <summary>Payload of the <c>message_template.*</c> events.</summary>
public record MessageTemplateEventData
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("external_id")] public string? ExternalId { get; init; }
    [JsonPropertyName("profile_id")] public string? ProfileId { get; init; }
    [JsonPropertyName("platform")] public Platform Platform { get; init; }
    [JsonPropertyName("name")] public string? Name { get; init; }
    [JsonPropertyName("language")] public string? Language { get; init; }
    [JsonPropertyName("category")] public string? Category { get; init; }
    [JsonPropertyName("status")] public string? Status { get; init; }
    [JsonPropertyName("rejected_reason")] public string? RejectedReason { get; init; }
    [JsonPropertyName("quality_score")] public string? QualityScore { get; init; }
}

public record WhatsAppAutomaticEventData
{
    [JsonPropertyName("profile_id")] public string? ProfileId { get; init; }
    [JsonPropertyName("chat_id")] public string? ChatId { get; init; }
    [JsonPropertyName("participant_external_id")] public string? ParticipantExternalId { get; init; }
    [JsonPropertyName("external_message_id")] public string? ExternalMessageId { get; init; }
    [JsonPropertyName("event_name")] public string? EventName { get; init; }
    [JsonPropertyName("ctwa_clid")] public string? CtwaClid { get; init; }
    [JsonPropertyName("custom_data")] public IReadOnlyDictionary<string, JsonElement>? CustomData { get; init; }
    [JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; init; }
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
    public static MessageEventData AsMessageEvent(WebhookEvent ev) => Decode<MessageEventData>(ev);
    public static ReactionEventData AsReactionEvent(WebhookEvent ev) => Decode<ReactionEventData>(ev);
    public static ProfileCommentCreatedData AsProfileCommentCreated(WebhookEvent ev) => Decode<ProfileCommentCreatedData>(ev);
    public static PlacementEventData AsPlacementEvent(WebhookEvent ev) => Decode<PlacementEventData>(ev);
    public static PlacementsSyncedData AsPlacementsSynced(WebhookEvent ev) => Decode<PlacementsSyncedData>(ev);
    public static ReferralEventData AsReferralEvent(WebhookEvent ev) => Decode<ReferralEventData>(ev);
    public static ThreadControlChangedEventData AsThreadControlChanged(WebhookEvent ev) => Decode<ThreadControlChangedEventData>(ev);
    public static ThreadControlRequestedEventData AsThreadControlRequested(WebhookEvent ev) => Decode<ThreadControlRequestedEventData>(ev);
    public static MessageTemplateEventData AsMessageTemplateEvent(WebhookEvent ev) => Decode<MessageTemplateEventData>(ev);
    public static WhatsAppAutomaticEventData AsWhatsAppAutomaticEvent(WebhookEvent ev) => Decode<WhatsAppAutomaticEventData>(ev);

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
