using System.Text.Json;
using System.Text.Json.Serialization;

namespace PostProxy.Models;

/// <summary>One day of <see cref="DailyStats"/>.</summary>
public record DailyStatsRecord
{
    [JsonPropertyName("date")]
    public required string Date { get; init; }

    [JsonPropertyName("platform_stats")]
    public IReadOnlyDictionary<string, JsonElement>? PlatformStats { get; init; }

    /// <summary>Normalized metrics (e.g. <c>impressions</c>, <c>viewers</c>), keyed by name.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Metrics { get; init; } = new();
}

public record DailyStats
{
    [JsonPropertyName("profile_id")]
    public string? ProfileId { get; init; }

    [JsonPropertyName("platform")]
    public Platform? Platform { get; init; }

    [JsonPropertyName("placement_id")]
    public string? PlacementId { get; init; }

    [JsonPropertyName("attribution")]
    public DailyStatsAttribution? Attribution { get; init; }

    [JsonPropertyName("from")]
    public string? From { get; init; }

    [JsonPropertyName("to")]
    public string? To { get; init; }

    [JsonPropertyName("data_timezone")]
    public string? DataTimezone { get; init; }

    [JsonPropertyName("records")]
    public IReadOnlyList<DailyStatsRecord> Records { get; init; } = Array.Empty<DailyStatsRecord>();

    /// <summary>Describes each metric in <see cref="Records"/>.</summary>
    [JsonPropertyName("metrics")]
    public IReadOnlyDictionary<string, JsonElement>? Metrics { get; init; }
}

public record DailyStatsResponse
{
    [JsonPropertyName("data")]
    public required DailyStats Data { get; init; }
}

public record FollowerHistoryRecord
{
    [JsonPropertyName("date")]
    public required string Date { get; init; }

    [JsonPropertyName("followers_gained")]
    public int? FollowersGained { get; init; }

    [JsonPropertyName("followers_lost")]
    public int? FollowersLost { get; init; }

    [JsonPropertyName("followers_change")]
    public int? FollowersChange { get; init; }

    [JsonPropertyName("followers_total")]
    public int? FollowersTotal { get; init; }
}

public record FollowerHistory
{
    [JsonPropertyName("profile_id")]
    public string? ProfileId { get; init; }

    [JsonPropertyName("platform")]
    public Platform? Platform { get; init; }

    [JsonPropertyName("placement_id")]
    public string? PlacementId { get; init; }

    [JsonPropertyName("from")]
    public string? From { get; init; }

    [JsonPropertyName("to")]
    public string? To { get; init; }

    [JsonPropertyName("records")]
    public IReadOnlyList<FollowerHistoryRecord> Records { get; init; } = Array.Empty<FollowerHistoryRecord>();
}

public record FollowerHistoryResponse
{
    [JsonPropertyName("data")]
    public required FollowerHistory Data { get; init; }
}

/// <summary>Instagram only: a DM participant's status. Cached for a short while server-side.</summary>
public record ParticipantStatus
{
    [JsonPropertyName("is_verified_user")]
    public bool? IsVerifiedUser { get; init; }

    [JsonPropertyName("is_user_follow_business")]
    public bool? IsUserFollowBusiness { get; init; }

    [JsonPropertyName("is_business_follow_user")]
    public bool? IsBusinessFollowUser { get; init; }

    [JsonPropertyName("fetched_at")]
    public DateTimeOffset? FetchedAt { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

/// <summary>
/// LinkedIn only: a company page URL (or vanity name) resolved to the URN you can
/// @-mention in a post body.
/// </summary>
public record ResolvedMention
{
    [JsonPropertyName("urn")]
    public required string Urn { get; init; }

    /// <summary>Always <c>organization</c> today.</summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("vanity_name")]
    public string? VanityName { get; init; }
}

/// <summary>Music and original sounds an Instagram reel can attach.</summary>
public record InstagramAvailableAudioResponse
{
    [JsonPropertyName("data")]
    public IReadOnlyList<IReadOnlyDictionary<string, JsonElement>> Data { get; init; } =
        Array.Empty<IReadOnlyDictionary<string, JsonElement>>();

    /// <summary>Any other top-level fields, e.g. paging.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}
