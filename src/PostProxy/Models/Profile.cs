using System.Text.Json.Serialization;

namespace PostProxy.Models;

public record Profile
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("username")]
    public string? Username { get; init; }

    [JsonPropertyName("status")]
    public ProfileStatus? Status { get; init; }

    /// <summary>
    /// Why an inactive profile was deactivated; <see cref="ProfileInactiveReason.NetworkTaken"/>
    /// means the same account was connected elsewhere.
    /// </summary>
    [JsonPropertyName("inactive_reason")]
    public ProfileInactiveReason? InactiveReason { get; init; }

    [JsonPropertyName("platform")]
    public Platform? Platform { get; init; }

    /// <summary>The account's id on the platform.</summary>
    [JsonPropertyName("platform_id")]
    public string? PlatformId { get; init; }

    [JsonPropertyName("profile_group_id")]
    public string? ProfileGroupId { get; init; }

    [JsonPropertyName("expires_at")]
    public DateTimeOffset? ExpiresAt { get; init; }

    [JsonPropertyName("post_count")]
    public int? PostCount { get; init; }

    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; init; }

    [JsonPropertyName("platform_url")]
    public string? PlatformUrl { get; init; }

    [JsonPropertyName("placements_sync_status")]
    public string? PlacementsSyncStatus { get; init; }
}
