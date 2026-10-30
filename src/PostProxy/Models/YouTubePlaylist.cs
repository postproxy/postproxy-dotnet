using System.Text.Json.Serialization;

namespace PostProxy.Models;

/// <summary>
/// A YouTube playlist. Publish a video into it with <c>YouTubeParams.PlaylistId</c>.
/// </summary>
public record YouTubePlaylist
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("privacy_status")]
    public YouTubePrivacy? PrivacyStatus { get; init; }

    [JsonPropertyName("item_count")]
    public int ItemCount { get; init; }

    [JsonPropertyName("thumbnail_url")]
    public string? ThumbnailUrl { get; init; }

    [JsonPropertyName("published_at")]
    public DateTimeOffset? PublishedAt { get; init; }
}

public record YouTubePlaylistsResponse
{
    [JsonPropertyName("data")]
    public IReadOnlyList<YouTubePlaylist> Data { get; init; } = Array.Empty<YouTubePlaylist>();

    /// <summary>Pass as <c>pageToken</c> for the next page; <c>null</c> on the last page.</summary>
    [JsonPropertyName("next_page_token")]
    public string? NextPageToken { get; init; }
}

/// <summary>Result of <c>Profiles.DeleteYouTubePlaylistAsync</c>.</summary>
public record YouTubePlaylistDeleted
{
    [JsonPropertyName("deleted")]
    public bool Deleted { get; init; }

    [JsonPropertyName("playlist_id")]
    public required string PlaylistId { get; init; }
}
