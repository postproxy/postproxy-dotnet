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
}

public record FacebookParams
{
    [JsonPropertyName("format")]
    public FacebookFormat? Format { get; init; }

    [JsonPropertyName("first_comment")]
    public string? FirstComment { get; init; }

    [JsonPropertyName("page_id")]
    public string? PageId { get; init; }
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

    [JsonPropertyName("audio_name")]
    public string? AudioName { get; init; }

    [JsonPropertyName("trial_strategy")]
    public bool? TrialStrategy { get; init; }

    [JsonPropertyName("thumb_offset")]
    public int? ThumbOffset { get; init; }
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
}

public record LinkedInParams
{
    [JsonPropertyName("format")]
    public LinkedInFormat? Format { get; init; }

    [JsonPropertyName("organization_id")]
    public string? OrganizationId { get; init; }
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

    [JsonPropertyName("made_for_kids")]
    public bool? MadeForKids { get; init; }
}

public record TwitterParams
{
    [JsonPropertyName("format")]
    public TwitterFormat? Format { get; init; }
}

public record ThreadsParams
{
    [JsonPropertyName("format")]
    public ThreadsFormat? Format { get; init; }
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

    [JsonPropertyName("thumb_offset")]
    public int? ThumbOffset { get; init; }
}
