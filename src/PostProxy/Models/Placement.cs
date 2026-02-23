using System.Text.Json.Serialization;

namespace PostProxy.Models;

public record Placement
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}
