using System.Text.Json;
using System.Text.Json.Serialization;

namespace PostProxy.Models;

public record Placement
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; init; }

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

/// <summary>A connected profile a placement is reachable through.</summary>
public record AccountPlacementProfile
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("connected")]
    public bool Connected { get; init; }
}

/// <summary>
/// Account-level placement, returned by <c>Placements.ListAsync</c>. A placement
/// (a Facebook Page, LinkedIn organization, WhatsApp number, ...) can be
/// reachable through several connected profiles; one of them is active.
/// </summary>
public record AccountPlacement
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("platform")]
    public Platform Platform { get; init; }

    [JsonPropertyName("external_id")]
    public string? ExternalId { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("active_profile")]
    public AccountPlacementProfile? ActiveProfile { get; init; }

    [JsonPropertyName("linked_profiles")]
    public IReadOnlyList<AccountPlacementProfile> LinkedProfiles { get; init; } = Array.Empty<AccountPlacementProfile>();
}
