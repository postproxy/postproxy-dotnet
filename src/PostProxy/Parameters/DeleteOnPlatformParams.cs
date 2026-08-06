using System.Text.Json.Serialization;
namespace PostProxy.Parameters;

public record DeleteOnPlatformParams
{
    public string? PostProfileId { get; init; }
    public string? ProfileId { get; init; }
    public string? Network { get; init; }
    public string? ProfileGroupId { get; init; }

    /// <summary>
    /// Optional Idempotency-Key. Retrying with the same key replays the
    /// original response instead of running the request again.
    /// </summary>
    [JsonIgnore]
    public string? IdempotencyKey { get; init; }
}
