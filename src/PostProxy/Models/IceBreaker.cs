using System.Text.Json.Serialization;

namespace PostProxy.Models;

/// <summary>
/// Instagram DM ice breaker (FAQ prompt shown when a user opens a chat).
/// </summary>
public record IceBreaker
{
    [JsonPropertyName("question")]
    public required string Question { get; init; }

    [JsonPropertyName("payload")]
    public required string Payload { get; init; }
}

public record IceBreakersResponse
{
    [JsonPropertyName("ice_breakers")]
    public IReadOnlyList<IceBreaker> IceBreakers { get; init; } = [];
}
