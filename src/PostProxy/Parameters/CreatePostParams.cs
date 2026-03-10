using System.Text.Json.Serialization;
using PostProxy.Models;

namespace PostProxy.Parameters;

public record ThreadChildInput
{
    [JsonPropertyName("body")]
    public required string Body { get; init; }

    [JsonPropertyName("media")]
    public IReadOnlyList<string>? Media { get; init; }

    [JsonIgnore]
    public IReadOnlyList<string>? MediaFiles { get; init; }
}

public record CreatePostParams
{
    public required string Body { get; init; }
    public required IReadOnlyList<string> Profiles { get; init; }
    public IReadOnlyList<string>? Media { get; init; }
    public IReadOnlyList<string>? MediaFiles { get; init; }
    public PlatformParams? Platforms { get; init; }
    public IReadOnlyList<ThreadChildInput>? Thread { get; init; }
    public DateTimeOffset? ScheduledAt { get; init; }
    public bool? Draft { get; init; }
    public string? QueueId { get; init; }
    public string? QueuePriority { get; init; }
    public string? ProfileGroupId { get; init; }
}
