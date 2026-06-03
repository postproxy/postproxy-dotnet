using System.Text.Json.Serialization;

namespace PostProxy.Models;

public record Attachment
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("url")]
    public string? Url { get; init; }

    [JsonPropertyName("status")]
    public MediaStatus Status { get; init; }

    [JsonPropertyName("external_id")]
    public string? ExternalId { get; init; }
}
