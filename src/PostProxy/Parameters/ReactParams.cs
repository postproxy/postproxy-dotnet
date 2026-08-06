using System.Text.Json.Serialization;
namespace PostProxy.Parameters;

public record ReactParams
{
    public string? Reaction { get; init; }
    public string? Emoji { get; init; }
    public string? ProfileGroupId { get; init; }

    /// <summary>
    /// Optional Idempotency-Key. Retrying with the same key replays the
    /// original response instead of running the request again.
    /// </summary>
    [JsonIgnore]
    public string? IdempotencyKey { get; init; }
}
