using System.Text.Json.Serialization;

namespace PostProxy.Models;

public record ProfileGroup
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("profiles_count")]
    public int? ProfilesCount { get; init; }
}
