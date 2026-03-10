using System.Text.Json.Serialization;

namespace PostProxy.Models;

public record Timeslot
{
    [JsonPropertyName("id")]
    public int? Id { get; init; }

    [JsonPropertyName("day")]
    public int Day { get; init; }

    [JsonPropertyName("time")]
    public string? Time { get; init; }
}

public record Queue
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; init; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; init; }

    [JsonPropertyName("jitter")]
    public int Jitter { get; init; }

    [JsonPropertyName("profile_group_id")]
    public string? ProfileGroupId { get; init; }

    [JsonPropertyName("timeslots")]
    public IReadOnlyList<Timeslot>? Timeslots { get; init; }

    [JsonPropertyName("posts_count")]
    public int PostsCount { get; init; }
}

public record NextSlotResponse
{
    [JsonPropertyName("next_slot")]
    public string? NextSlot { get; init; }
}
