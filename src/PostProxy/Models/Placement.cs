using System.Text.Json;
using System.Text.Json.Serialization;

namespace PostProxy.Models;

public record Placement
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("metadata")]
    public IReadOnlyDictionary<string, JsonElement>? Metadata { get; init; }
}

/// <summary>
/// Placement returned by <c>Profiles.AssignPlacementToGroupAsync</c>,
/// including the profile group it now belongs to.
/// </summary>
public record AssignedPlacement
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("metadata")]
    public IReadOnlyDictionary<string, JsonElement>? Metadata { get; init; }

    [JsonPropertyName("profile_group_id")]
    public required string ProfileGroupId { get; init; }
}
