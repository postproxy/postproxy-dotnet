using System.Text.Json.Serialization;

namespace PostProxy.Models;

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

    [JsonPropertyName("platforms")]
    public IReadOnlyList<PlatformResult>? Platforms { get; init; }
}

public record PlatformResult
{
    [JsonPropertyName("platform")]
    public Platform? Platform { get; init; }

    [JsonPropertyName("status")]
    public PlatformPostStatus? Status { get; init; }

    [JsonPropertyName("params")]
    public Dictionary<string, object>? Params { get; init; }

    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("attempted_at")]
    public DateTimeOffset? AttemptedAt { get; init; }

    [JsonPropertyName("insights")]
    public Insights? Insights { get; init; }
}

public record Insights
{
    [JsonPropertyName("impressions")]
    public int? Impressions { get; init; }

    [JsonPropertyName("on")]
    public string? On { get; init; }
}
