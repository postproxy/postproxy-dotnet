using System.Text.Json.Serialization;

namespace PostProxy.Models;

public record PlatformParams
{
    [JsonPropertyName("facebook")]
    public FacebookParams? Facebook { get; init; }

    [JsonPropertyName("instagram")]
    public InstagramParams? Instagram { get; init; }

    [JsonPropertyName("tiktok")]
    public TikTokParams? TikTok { get; init; }

    [JsonPropertyName("linkedin")]
    public LinkedInParams? LinkedIn { get; init; }

    [JsonPropertyName("youtube")]
    public YouTubeParams? YouTube { get; init; }

    [JsonPropertyName("twitter")]
    public TwitterParams? Twitter { get; init; }

    [JsonPropertyName("threads")]
    public ThreadsParams? Threads { get; init; }

    [JsonPropertyName("pinterest")]
    public PinterestParams? Pinterest { get; init; }

    [JsonPropertyName("bluesky")]
    public BlueskyParams? Bluesky { get; init; }

    [JsonPropertyName("telegram")]
    public TelegramParams? Telegram { get; init; }

    [JsonPropertyName("google_business")]
    public Dictionary<string, object>? GoogleBusiness { get; init; }
}

public record BlueskyParams
{
    [JsonPropertyName("format")]
    public BlueskyFormat? Format { get; init; }

    /// <summary>One string for every media item, or one per item in order.</summary>
    [JsonPropertyName("alt_text")]
    public AltText? AltText { get; init; }
}

public record TelegramParams
{
    [JsonPropertyName("format")]
    public TelegramFormat? Format { get; init; }

    [JsonPropertyName("chat_id")]
    public required string ChatId { get; init; }

    [JsonPropertyName("parse_mode")]
    public TelegramParseMode? ParseMode { get; init; }

    [JsonPropertyName("disable_link_preview")]
    public bool? DisableLinkPreview { get; init; }

    [JsonPropertyName("disable_notification")]
    public bool? DisableNotification { get; init; }
}

public record FacebookParams
{
    [JsonPropertyName("format")]
    public FacebookFormat? Format { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("first_comment")]
    public string? FirstComment { get; init; }

    [JsonPropertyName("page_id")]
    public string? PageId { get; init; }

    /// <summary>Link post: Facebook renders the preview card. Text-only, no media.</summary>
    [JsonPropertyName("link")]
    public string? Link { get; init; }

    /// <summary>One string for every media item, or one per item in order.</summary>
    [JsonPropertyName("alt_text")]
    public AltText? AltText { get; init; }

    /// <summary>Photo posts only; videos and reels ignore it.</summary>
    [JsonPropertyName("made_with_ai")]
    public bool? MadeWithAi { get; init; }

    /// <summary>ISO 3166-1 alpha-2 codes; viewers elsewhere won't see the post.</summary>
    [JsonPropertyName("countries")]
    public IReadOnlyList<string>? Countries { get; init; }
}

public record InstagramParams
{
    [JsonPropertyName("format")]
    public InstagramFormat? Format { get; init; }

    [JsonPropertyName("first_comment")]
    public string? FirstComment { get; init; }

    [JsonPropertyName("collaborators")]
    public IReadOnlyList<string>? Collaborators { get; init; }

    [JsonPropertyName("cover_url")]
    public string? CoverUrl { get; init; }

    /// <summary>
    /// Local path to a cover image, uploaded with the post (the request switches
    /// to multipart). A base64 data URL (<c>data:image/jpeg;base64,...</c>) is
    /// sent as-is instead.
    /// </summary>
    [JsonPropertyName("cover_file")]
    public string? CoverFile { get; init; }

    [JsonPropertyName("audio_name")]
    public string? AudioName { get; init; }

    /// <summary>
    /// Music or sound id from <c>Profiles.GetInstagramAvailableAudioAsync</c>.
    /// Profiles connected via Facebook login only.
    /// </summary>
    [JsonPropertyName("audio_id")]
    public string? AudioId { get; init; }

    /// <summary>1-100, default 100. Facebook-login profiles only.</summary>
    [JsonPropertyName("audio_volume")]
    public int? AudioVolume { get; init; }

    [JsonPropertyName("video_volume")]
    public int? VideoVolume { get; init; }

    /// <summary>Remove the video's own audio track (reel, story).</summary>
    [JsonPropertyName("strip_audio")]
    public bool? StripAudio { get; init; }

    [JsonPropertyName("trial_strategy")]
    public InstagramTrialStrategy? TrialStrategy { get; init; }

    [JsonPropertyName("thumb_offset")]
    public int? ThumbOffset { get; init; }

    /// <summary>Instagram accounts to tag. See <see cref="InstagramUserTag"/>.</summary>
    [JsonPropertyName("user_tags")]
    public IReadOnlyList<InstagramUserTag>? UserTags { get; init; }

    /// <summary>One string for every media item, or one per item in order.</summary>
    [JsonPropertyName("alt_text")]
    public AltText? AltText { get; init; }

    [JsonPropertyName("made_with_ai")]
    public bool? MadeWithAi { get; init; }
}

/// <summary>
/// An Instagram account to tag in a post. Images require <see cref="X"/> and
/// <see cref="Y"/>; reels and video slides are tagged by username only —
/// Instagram ignores coordinates there.
/// </summary>
public record InstagramUserTag
{
    [JsonPropertyName("username")]
    public required string Username { get; init; }

    /// <summary>Horizontal position, 0.0-1.0 from the left edge. Required for images.</summary>
    [JsonPropertyName("x")]
    public double? X { get; init; }

    /// <summary>Vertical position, 0.0-1.0 from the top edge. Required for images.</summary>
    [JsonPropertyName("y")]
    public double? Y { get; init; }

    /// <summary>Which media item to tag, 0-based. Defaults to 0.</summary>
    [JsonPropertyName("media_index")]
    public int? MediaIndex { get; init; }
}

public record TikTokParams
{
    [JsonPropertyName("format")]
    public TikTokFormat? Format { get; init; }

    [JsonPropertyName("privacy_status")]
    public TikTokPrivacy? PrivacyStatus { get; init; }

    [JsonPropertyName("photo_cover_index")]
    public int? PhotoCoverIndex { get; init; }

    [JsonPropertyName("auto_add_music")]
    public bool? AutoAddMusic { get; init; }

    [JsonPropertyName("made_with_ai")]
    public bool? MadeWithAi { get; init; }

    [JsonPropertyName("disable_comment")]
    public bool? DisableComment { get; init; }

    [JsonPropertyName("disable_duet")]
    public bool? DisableDuet { get; init; }

    [JsonPropertyName("disable_stitch")]
    public bool? DisableStitch { get; init; }

    [JsonPropertyName("brand_content_toggle")]
    public bool? BrandContentToggle { get; init; }

    [JsonPropertyName("brand_organic_toggle")]
    public bool? BrandOrganicToggle { get; init; }

    /// <summary>Send to the creator's TikTok inbox as a draft instead of publishing.</summary>
    [JsonPropertyName("upload_as_draft")]
    public bool? UploadAsDraft { get; init; }

    /// <summary>Custom video cover. Profiles connected before 2026-09-30 must reconnect.</summary>
    [JsonPropertyName("cover_url")]
    public string? CoverUrl { get; init; }

    /// <summary>
    /// Local path to a cover image, uploaded with the post (the request switches
    /// to multipart). A base64 data URL (<c>data:image/jpeg;base64,...</c>) is
    /// sent as-is instead.
    /// </summary>
    [JsonPropertyName("cover_file")]
    public string? CoverFile { get; init; }

    /// <summary>Frame to use as cover, in milliseconds. <see cref="CoverUrl"/> wins if both are set.</summary>
    [JsonPropertyName("thumb_offset")]
    public int? ThumbOffset { get; init; }
}

public record LinkedInParams
{
    [JsonPropertyName("format")]
    public LinkedInFormat? Format { get; init; }

    [JsonPropertyName("organization_id")]
    public string? OrganizationId { get; init; }

    /// <summary>URN of a post to reshare, e.g. <c>urn:li:share:123</c>.</summary>
    [JsonPropertyName("reshare_urn")]
    public string? ReshareUrn { get; init; }

    /// <summary>One string for every media item, or one per item in order.</summary>
    [JsonPropertyName("alt_text")]
    public AltText? AltText { get; init; }

    /// <summary>Organization pages only; up to 25 ISO 3166-1 alpha-2 codes.</summary>
    [JsonPropertyName("countries")]
    public IReadOnlyList<string>? Countries { get; init; }

    /// <summary>Required when <see cref="Format"/> is Poll: 140 characters or fewer.</summary>
    [JsonPropertyName("poll_question")]
    public string? PollQuestion { get; init; }

    /// <summary>Required when <see cref="Format"/> is Poll: 2-4 options, max 30 characters each.</summary>
    [JsonPropertyName("poll_options")]
    public IReadOnlyList<string>? PollOptions { get; init; }

    /// <summary>1, 3, 7 (default) or 14.</summary>
    [JsonPropertyName("poll_duration_days")]
    public int? PollDurationDays { get; init; }
}

public record YouTubeParams
{
    [JsonPropertyName("format")]
    public YouTubeFormat? Format { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("privacy_status")]
    public YouTubePrivacy? PrivacyStatus { get; init; }

    [JsonPropertyName("cover_url")]
    public string? CoverUrl { get; init; }

    /// <summary>
    /// Local path to a cover image, uploaded with the post (the request switches
    /// to multipart). A base64 data URL (<c>data:image/jpeg;base64,...</c>) is
    /// sent as-is instead.
    /// </summary>
    [JsonPropertyName("cover_file")]
    public string? CoverFile { get; init; }

    [JsonPropertyName("made_for_kids")]
    public bool? MadeForKids { get; init; }

    [JsonPropertyName("tags")]
    public IReadOnlyList<string>? Tags { get; init; }

    [JsonPropertyName("category_id")]
    public string? CategoryId { get; init; }

    [JsonPropertyName("contains_synthetic_media")]
    public bool? ContainsSyntheticMedia { get; init; }
}

public record TwitterParams
{
    [JsonPropertyName("format")]
    public TwitterFormat? Format { get; init; }

    /// <summary>One string for every media item, or one per item in order.</summary>
    [JsonPropertyName("alt_text")]
    public AltText? AltText { get; init; }

    /// <summary>Required when <see cref="Format"/> is Poll: 2-4 options, max 25 characters each.</summary>
    [JsonPropertyName("poll_options")]
    public IReadOnlyList<string>? PollOptions { get; init; }

    /// <summary>Required when <see cref="Format"/> is Poll: 5 to 10080 minutes (7 days).</summary>
    [JsonPropertyName("poll_duration_minutes")]
    public int? PollDurationMinutes { get; init; }
}

public record ThreadsParams
{
    [JsonPropertyName("format")]
    public ThreadsFormat? Format { get; init; }

    /// <summary>One string for every media item, or one per item in order.</summary>
    [JsonPropertyName("alt_text")]
    public AltText? AltText { get; init; }

    /// <summary>One tag, 50 characters or fewer, without the leading # — no "." or "&amp;".</summary>
    [JsonPropertyName("topic_tag")]
    public string? TopicTag { get; init; }
}

public record PinterestParams
{
    [JsonPropertyName("format")]
    public PinterestFormat? Format { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("board_id")]
    public string? BoardId { get; init; }

    [JsonPropertyName("destination_link")]
    public string? DestinationLink { get; init; }

    [JsonPropertyName("cover_url")]
    public string? CoverUrl { get; init; }

    /// <summary>
    /// Local path to a cover image, uploaded with the post (the request switches
    /// to multipart). A base64 data URL (<c>data:image/jpeg;base64,...</c>) is
    /// sent as-is instead.
    /// </summary>
    [JsonPropertyName("cover_file")]
    public string? CoverFile { get; init; }

    [JsonPropertyName("thumb_offset")]
    public int? ThumbOffset { get; init; }

    /// <summary>One string for every media item, or one per item in order.</summary>
    [JsonPropertyName("alt_text")]
    public AltText? AltText { get; init; }
}
