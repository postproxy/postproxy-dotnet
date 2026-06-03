using System.Text.Json.Serialization;

namespace PostProxy.Models;

public record Reaction
{
    [JsonPropertyName("sender_external_id")]
    public required string SenderExternalId { get; init; }

    [JsonPropertyName("emoji")]
    public string? Emoji { get; init; }

    [JsonPropertyName("reaction")]
    public string? ReactionType { get; init; }

    [JsonPropertyName("at")]
    public DateTimeOffset? At { get; init; }
}
