using System.Text.Json.Serialization;
using System.Text.Json;

namespace PostProxy.Parameters;

public record SendMessageParams
{
    public string? Body { get; init; }
    public IReadOnlyList<string>? Media { get; init; }
    public IReadOnlyList<string>? MediaFiles { get; init; }
    public string? Tag { get; init; }
    public string? ReplyToExternalId { get; init; }
    public IReadOnlyDictionary<string, JsonElement>? ReplyMarkup { get; init; }
    public string? ProfileGroupId { get; init; }

    /// <summary>
    /// Optional Idempotency-Key. Retrying with the same key replays the
    /// original response instead of running the request again.
    /// </summary>
    [JsonIgnore]
    public string? IdempotencyKey { get; init; }
}
