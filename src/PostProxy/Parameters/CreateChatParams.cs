using System.Text.Json.Serialization;
namespace PostProxy.Parameters;

public record CreateChatParams
{
    public required string ParticipantExternalId { get; init; }
    public string? ParticipantUsername { get; init; }
    public string? ParticipantName { get; init; }
    public string? ProfileGroupId { get; init; }

    /// <summary>
    /// Optional Idempotency-Key. Retrying with the same key replays the
    /// original response instead of running the request again.
    /// </summary>
    [JsonIgnore]
    public string? IdempotencyKey { get; init; }
}
