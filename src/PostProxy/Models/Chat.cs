using System.Text.Json;
using System.Text.Json.Serialization;

namespace PostProxy.Models;

public record Chat
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("profile_id")]
    public required string ProfileId { get; init; }

    [JsonPropertyName("platform")]
    public Platform Platform { get; init; }

    /// <summary>The page / phone number the chat runs through.</summary>
    [JsonPropertyName("external_placement_id")]
    public string? ExternalPlacementId { get; init; }

    /// <summary>True for a WhatsApp group chat.</summary>
    [JsonPropertyName("group")]
    public bool Group { get; init; }

    [JsonPropertyName("participant_external_id")]
    public required string ParticipantExternalId { get; init; }

    [JsonPropertyName("participant_username")]
    public string? ParticipantUsername { get; init; }

    [JsonPropertyName("participant_name")]
    public string? ParticipantName { get; init; }

    [JsonPropertyName("participant_avatar_url")]
    public string? ParticipantAvatarUrl { get; init; }

    [JsonPropertyName("external_conversation_id")]
    public string? ExternalConversationId { get; init; }

    [JsonPropertyName("last_inbound_at")]
    public DateTimeOffset? LastInboundAt { get; init; }

    [JsonPropertyName("last_outbound_at")]
    public DateTimeOffset? LastOutboundAt { get; init; }

    [JsonPropertyName("last_message_at")]
    public DateTimeOffset? LastMessageAt { get; init; }

    [JsonPropertyName("metadata")]
    public IReadOnlyDictionary<string, JsonElement>? Metadata { get; init; }

    [JsonPropertyName("archived")]
    public bool? Archived { get; init; }

    /// <summary>
    /// WhatsApp only: whether the 24-hour customer service window is open. When
    /// it's closed only a template can be sent.
    /// </summary>
    [JsonPropertyName("within_messaging_window")]
    public bool? WithinMessagingWindow { get; init; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }
}
