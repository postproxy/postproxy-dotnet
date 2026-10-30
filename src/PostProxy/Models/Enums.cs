using System.Text.Json.Serialization;

namespace PostProxy.Models;

[JsonConverter(typeof(JsonStringEnumConverter<Platform>))]
public enum Platform
{
    [JsonStringEnumMemberName("facebook")] Facebook,
    [JsonStringEnumMemberName("instagram")] Instagram,
    [JsonStringEnumMemberName("tiktok")] TikTok,
    [JsonStringEnumMemberName("linkedin")] LinkedIn,
    [JsonStringEnumMemberName("youtube")] YouTube,
    [JsonStringEnumMemberName("twitter")] Twitter,
    [JsonStringEnumMemberName("threads")] Threads,
    [JsonStringEnumMemberName("pinterest")] Pinterest,
    [JsonStringEnumMemberName("bluesky")] Bluesky,
    [JsonStringEnumMemberName("telegram")] Telegram,
    [JsonStringEnumMemberName("google_business")] GoogleBusiness,
    [JsonStringEnumMemberName("whatsapp")] WhatsApp,
}

[JsonConverter(typeof(JsonStringEnumConverter<PostStatus>))]
public enum PostStatus
{
    [JsonStringEnumMemberName("pending")] Pending,
    [JsonStringEnumMemberName("draft")] Draft,
    [JsonStringEnumMemberName("processing")] Processing,
    [JsonStringEnumMemberName("processed")] Processed,
    [JsonStringEnumMemberName("scheduled")] Scheduled,
    [JsonStringEnumMemberName("media_processing_failed")] MediaProcessingFailed,
}

[JsonConverter(typeof(JsonStringEnumConverter<MediaStatus>))]
public enum MediaStatus
{
    [JsonStringEnumMemberName("pending")] Pending,
    [JsonStringEnumMemberName("processed")] Processed,
    [JsonStringEnumMemberName("failed")] Failed,
}

[JsonConverter(typeof(JsonStringEnumConverter<ProfileStatus>))]
public enum ProfileStatus
{
    [JsonStringEnumMemberName("active")] Active,
    [JsonStringEnumMemberName("expired")] Expired,
    [JsonStringEnumMemberName("inactive")] Inactive,
}

[JsonConverter(typeof(JsonStringEnumConverter<PlatformPostStatus>))]
public enum PlatformPostStatus
{
    [JsonStringEnumMemberName("pending")] Pending,
    [JsonStringEnumMemberName("processing")] Processing,
    [JsonStringEnumMemberName("published")] Published,
    [JsonStringEnumMemberName("failed")] Failed,
    [JsonStringEnumMemberName("deleted")] Deleted,
}

[JsonConverter(typeof(JsonStringEnumConverter<FacebookFormat>))]
public enum FacebookFormat
{
    [JsonStringEnumMemberName("post")] Post,
    [JsonStringEnumMemberName("story")] Story,
    [JsonStringEnumMemberName("reel")] Reel,
}

[JsonConverter(typeof(JsonStringEnumConverter<InstagramFormat>))]
public enum InstagramFormat
{
    [JsonStringEnumMemberName("post")] Post,
    [JsonStringEnumMemberName("reel")] Reel,
    [JsonStringEnumMemberName("story")] Story,
}

[JsonConverter(typeof(JsonStringEnumConverter<TikTokFormat>))]
public enum TikTokFormat
{
    [JsonStringEnumMemberName("video")] Video,
    [JsonStringEnumMemberName("image")] Image,
}

[JsonConverter(typeof(JsonStringEnumConverter<LinkedInFormat>))]
public enum LinkedInFormat
{
    [JsonStringEnumMemberName("post")] Post,
    [JsonStringEnumMemberName("poll")] Poll,
}

[JsonConverter(typeof(JsonStringEnumConverter<YouTubeFormat>))]
public enum YouTubeFormat
{
    [JsonStringEnumMemberName("post")] Post,
}

[JsonConverter(typeof(JsonStringEnumConverter<TwitterFormat>))]
public enum TwitterFormat
{
    [JsonStringEnumMemberName("post")] Post,
    [JsonStringEnumMemberName("poll")] Poll,
}

[JsonConverter(typeof(JsonStringEnumConverter<ThreadsFormat>))]
public enum ThreadsFormat
{
    [JsonStringEnumMemberName("post")] Post,
}

[JsonConverter(typeof(JsonStringEnumConverter<PinterestFormat>))]
public enum PinterestFormat
{
    [JsonStringEnumMemberName("pin")] Pin,
}

[JsonConverter(typeof(JsonStringEnumConverter<YouTubePrivacy>))]
public enum YouTubePrivacy
{
    [JsonStringEnumMemberName("public")] Public,
    [JsonStringEnumMemberName("unlisted")] Unlisted,
    [JsonStringEnumMemberName("private")] Private,
}

[JsonConverter(typeof(JsonStringEnumConverter<TikTokPrivacy>))]
public enum TikTokPrivacy
{
    [JsonStringEnumMemberName("PUBLIC_TO_EVERYONE")] PublicToEveryone,
    [JsonStringEnumMemberName("MUTUAL_FOLLOW_FRIENDS")] MutualFollowFriends,
    [JsonStringEnumMemberName("FOLLOWER_OF_CREATOR")] FollowerOfCreator,
    [JsonStringEnumMemberName("SELF_ONLY")] SelfOnly,
}

[JsonConverter(typeof(JsonStringEnumConverter<BlueskyFormat>))]
public enum BlueskyFormat
{
    [JsonStringEnumMemberName("post")] Post,
}

[JsonConverter(typeof(JsonStringEnumConverter<TelegramFormat>))]
public enum TelegramFormat
{
    [JsonStringEnumMemberName("post")] Post,
}

[JsonConverter(typeof(JsonStringEnumConverter<TelegramParseMode>))]
public enum TelegramParseMode
{
    [JsonStringEnumMemberName("HTML")] Html,
    [JsonStringEnumMemberName("MarkdownV2")] MarkdownV2,
}

[JsonConverter(typeof(JsonStringEnumConverter<MessageDirection>))]
public enum MessageDirection
{
    [JsonStringEnumMemberName("inbound")] Inbound,
    [JsonStringEnumMemberName("outbound")] Outbound,
}

[JsonConverter(typeof(JsonStringEnumConverter<MessageStatus>))]
public enum MessageStatus
{
    [JsonStringEnumMemberName("pending")] Pending,
    [JsonStringEnumMemberName("published")] Published,
    [JsonStringEnumMemberName("failed_waiting_for_retry")] FailedWaitingForRetry,
    [JsonStringEnumMemberName("failed")] Failed,
    [JsonStringEnumMemberName("received")] Received,
}

/// <summary>Which interactive element a participant tapped.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<TappedActionKind>))]
public enum TappedActionKind
{
    [JsonStringEnumMemberName("quick_reply")] QuickReply,
    [JsonStringEnumMemberName("postback")] Postback,
    /// <summary>Telegram — a tap on an inline-keyboard button.</summary>
    [JsonStringEnumMemberName("callback_query")] CallbackQuery,
}

[JsonConverter(typeof(JsonStringEnumConverter<WebhookEventType>))]
public enum WebhookEventType
{
    [JsonStringEnumMemberName("post.processed")] PostProcessed,
    [JsonStringEnumMemberName("post.imported")] PostImported,
    [JsonStringEnumMemberName("platform_post.published")] PlatformPostPublished,
    [JsonStringEnumMemberName("platform_post.failed")] PlatformPostFailed,
    [JsonStringEnumMemberName("platform_post.failed_waiting_for_retry")] PlatformPostFailedWaitingForRetry,
    [JsonStringEnumMemberName("platform_post.edited")] PlatformPostEdited,
    [JsonStringEnumMemberName("platform_post.edit_failed")] PlatformPostEditFailed,
    [JsonStringEnumMemberName("platform_post.insights")] PlatformPostInsights,
    [JsonStringEnumMemberName("profile.connected")] ProfileConnected,
    [JsonStringEnumMemberName("profile.disconnected")] ProfileDisconnected,
    [JsonStringEnumMemberName("profile.stats")] ProfileStats,
    [JsonStringEnumMemberName("profile.placements_synced")] ProfilePlacementsSynced,
    [JsonStringEnumMemberName("placement.orphaned")] PlacementOrphaned,
    [JsonStringEnumMemberName("placement.restored")] PlacementRestored,
    [JsonStringEnumMemberName("media.failed")] MediaFailed,
    [JsonStringEnumMemberName("comment.created")] CommentCreated,
    [JsonStringEnumMemberName("comment.failed")] CommentFailed,
    [JsonStringEnumMemberName("comment.failed_waiting_for_retry")] CommentFailedWaitingForRetry,
    [JsonStringEnumMemberName("comment.delivery_unconfirmed")] CommentDeliveryUnconfirmed,
    [JsonStringEnumMemberName("comment.edited")] CommentEdited,
    [JsonStringEnumMemberName("comment.edit_failed")] CommentEditFailed,
    [JsonStringEnumMemberName("profile_comment.created")] ProfileCommentCreated,
    [JsonStringEnumMemberName("profile_comment.failed")] ProfileCommentFailed,
    [JsonStringEnumMemberName("profile_comment.failed_waiting_for_retry")] ProfileCommentFailedWaitingForRetry,
    [JsonStringEnumMemberName("profile_comment.edited")] ProfileCommentEdited,
    [JsonStringEnumMemberName("profile_comment.deleted")] ProfileCommentDeleted,
    [JsonStringEnumMemberName("message.received")] MessageReceived,
    [JsonStringEnumMemberName("message.sent")] MessageSent,
    [JsonStringEnumMemberName("message.delivered")] MessageDelivered,
    [JsonStringEnumMemberName("message.read")] MessageRead,
    [JsonStringEnumMemberName("message.edited")] MessageEdited,
    [JsonStringEnumMemberName("message.deleted")] MessageDeleted,
    [JsonStringEnumMemberName("message.failed_waiting_for_retry")] MessageFailedWaitingForRetry,
    [JsonStringEnumMemberName("message.failed")] MessageFailed,
    [JsonStringEnumMemberName("reaction.received")] ReactionReceived,
    [JsonStringEnumMemberName("referral.received")] ReferralReceived,
    [JsonStringEnumMemberName("chat.thread_control_changed")] ChatThreadControlChanged,
    [JsonStringEnumMemberName("chat.thread_control_requested")] ChatThreadControlRequested,
    [JsonStringEnumMemberName("message_template.status_updated")] MessageTemplateStatusUpdated,
    [JsonStringEnumMemberName("message_template.category_updated")] MessageTemplateCategoryUpdated,
    [JsonStringEnumMemberName("placement.name_status_updated")] PlacementNameStatusUpdated,
    [JsonStringEnumMemberName("placement.quality_updated")] PlacementQualityUpdated,
    [JsonStringEnumMemberName("whatsapp.automatic_event")] WhatsAppAutomaticEvent,
}

/// <summary>What started a post sync run.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<PostSyncTrigger>))]
public enum PostSyncTrigger
{
    [JsonStringEnumMemberName("connect")] Connect,
    [JsonStringEnumMemberName("scheduled")] Scheduled,
    [JsonStringEnumMemberName("backfill")] Backfill,
}

/// <summary>The state of a post sync run.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<PostSyncStatus>))]
public enum PostSyncStatus
{
    [JsonStringEnumMemberName("pending")] Pending,
    [JsonStringEnumMemberName("running")] Running,
    [JsonStringEnumMemberName("completed")] Completed,
    [JsonStringEnumMemberName("failed")] Failed,
}

/// <summary>Instagram trial reel strategy.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<InstagramTrialStrategy>))]
public enum InstagramTrialStrategy
{
    [JsonStringEnumMemberName("MANUAL")] Manual,
    [JsonStringEnumMemberName("SS_PERFORMANCE")] SsPerformance,
}

[JsonConverter(typeof(JsonStringEnumConverter<InstagramAudioType>))]
public enum InstagramAudioType
{
    [JsonStringEnumMemberName("music")] Music,
    [JsonStringEnumMemberName("original_sound")] OriginalSound,
}

/// <summary>Who can see a Pinterest board created via <c>Profiles.CreatePinterestBoardAsync</c>.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<PinterestBoardPrivacy>))]
public enum PinterestBoardPrivacy
{
    [JsonStringEnumMemberName("PUBLIC")] Public,
    [JsonStringEnumMemberName("PROTECTED")] Protected,
}

/// <summary>
/// Which connection flow to start. Instagram connects through Instagram login
/// (default) or Facebook login; WhatsApp through the Cloud API or the WhatsApp
/// Business app.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ConnectionOnboarding>))]
public enum ConnectionOnboarding
{
    [JsonStringEnumMemberName("instagram")] Instagram,
    [JsonStringEnumMemberName("facebook")] Facebook,
    [JsonStringEnumMemberName("api")] Api,
    [JsonStringEnumMemberName("business_app")] BusinessApp,
}

/// <summary>
/// Which day post metrics count toward in daily stats: the day they were
/// received, or the day the post was published.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<DailyStatsAttribution>))]
public enum DailyStatsAttribution
{
    [JsonStringEnumMemberName("received")] Received,
    [JsonStringEnumMemberName("publish")] Publish,
}

/// <summary>
/// Why an inactive profile was deactivated. <see cref="NetworkTaken"/> means the
/// same account was connected elsewhere.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ProfileInactiveReason>))]
public enum ProfileInactiveReason
{
    [JsonStringEnumMemberName("network_taken")] NetworkTaken,
}
