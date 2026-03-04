using System.Text.Json.Serialization;

namespace PostProxy.Models;

public record Webhook
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("url")]
    public string? Url { get; init; }

    [JsonPropertyName("events")]
    public IReadOnlyList<string>? Events { get; init; }

    [JsonPropertyName("enabled")]
    public bool? Enabled { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("secret")]
    public string? Secret { get; init; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; init; }
}

public record WebhookDelivery
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("event_id")]
    public string? EventId { get; init; }

    [JsonPropertyName("event_type")]
    public string? EventType { get; init; }

    [JsonPropertyName("response_status")]
    public int? ResponseStatus { get; init; }

    [JsonPropertyName("attempt_number")]
    public int AttemptNumber { get; init; }

    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("attempted_at")]
    public DateTimeOffset? AttemptedAt { get; init; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }
}
