using System.Text.Json.Serialization;

namespace PostProxy.Models;

public record SyncProfile
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("network")]
    public Platform Network { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("external_username")]
    public string? ExternalUsername { get; init; }
}

public record BlueskyConnectionResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("profile")]
    public required SyncProfile Profile { get; init; }
}

public record TelegramConnectionResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("profile")]
    public required SyncProfile Profile { get; init; }

    [JsonPropertyName("next_step")]
    public string? NextStep { get; init; }
}
