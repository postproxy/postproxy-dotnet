using System.Text.Json;
using System.Text.Json.Serialization;

namespace PostProxy.Models;

public record ProfileStats
{
    [JsonPropertyName("profile_id")]
    public required string ProfileId { get; init; }

    [JsonPropertyName("platform")]
    public Platform Platform { get; init; }

    [JsonPropertyName("placement_id")]
    public string? PlacementId { get; init; }

    [JsonPropertyName("records")]
    public IReadOnlyList<ProfileStatsRecord> Records { get; init; } = Array.Empty<ProfileStatsRecord>();
}

public record ProfileStatsRecord
{
    [JsonPropertyName("stats")]
    public IReadOnlyDictionary<string, JsonElement> Stats { get; init; } =
        new Dictionary<string, JsonElement>();

    [JsonPropertyName("recorded_at")]
    public DateTimeOffset RecordedAt { get; init; }
}

public record ProfileStatsResponse
{
    [JsonPropertyName("data")]
    public required ProfileStats Data { get; init; }
}
