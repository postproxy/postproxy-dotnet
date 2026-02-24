using System.Text.Json.Serialization;

namespace PostProxy.Models;

public record StatsResponse
{
    [JsonPropertyName("data")]
    public required IReadOnlyDictionary<string, PostStats> Data { get; init; }
}

public record PostStats
{
    [JsonPropertyName("platforms")]
    public required IReadOnlyList<PlatformStats> Platforms { get; init; }
}

public record PlatformStats
{
    [JsonPropertyName("profile_id")]
    public string? ProfileId { get; init; }

    [JsonPropertyName("platform")]
    public Platform? Platform { get; init; }

    [JsonPropertyName("records")]
    public required IReadOnlyList<StatsRecord> Records { get; init; }
}

public record StatsRecord
{
    [JsonPropertyName("stats")]
    public required IReadOnlyDictionary<string, long> Stats { get; init; }

    [JsonPropertyName("recorded_at")]
    public DateTimeOffset RecordedAt { get; init; }
}
