using System.Text.Json.Serialization;
using System.Text.Json;

namespace PostProxy.Parameters;

public record EditMessageParams
{
    public string? Body { get; init; }
    public IReadOnlyDictionary<string, JsonElement>? ReplyMarkup { get; init; }
    public string? ProfileGroupId { get; init; }

    /// <summary>
    /// Optional Idempotency-Key. Retrying with the same key replays the
    /// original response instead of running the request again.
    /// </summary>
    [JsonIgnore]
    public string? IdempotencyKey { get; init; }
}
