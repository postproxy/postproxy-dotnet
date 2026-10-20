using System.Text.Json;
using System.Text.Json.Serialization;

namespace PostProxy.Models;

/// <summary>A WhatsApp message template, which lives on the WhatsApp Business account (the profile).</summary>
public record WhatsAppTemplate
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("external_id")]
    public string? ExternalId { get; init; }

    [JsonPropertyName("profile_id")]
    public string? ProfileId { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("language")]
    public string? Language { get; init; }

    /// <summary>MARKETING, UTILITY or AUTHENTICATION.</summary>
    [JsonPropertyName("category")]
    public string? Category { get; init; }

    /// <summary>e.g. PENDING, APPROVED, REJECTED.</summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    /// <summary>POSITIONAL or NAMED.</summary>
    [JsonPropertyName("parameter_format")]
    public string? ParameterFormat { get; init; }

    [JsonPropertyName("components")]
    public IReadOnlyList<IReadOnlyDictionary<string, JsonElement>> Components { get; init; } =
        Array.Empty<IReadOnlyDictionary<string, JsonElement>>();

    [JsonPropertyName("message_send_ttl_seconds")]
    public int? MessageSendTtlSeconds { get; init; }

    [JsonPropertyName("rejected_reason")]
    public string? RejectedReason { get; init; }

    [JsonPropertyName("quality_score")]
    public string? QualityScore { get; init; }

    [JsonPropertyName("variable_count")]
    public int VariableCount { get; init; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; init; }
}
