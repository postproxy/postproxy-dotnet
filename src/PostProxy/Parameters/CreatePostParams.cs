using PostProxy.Models;

namespace PostProxy.Parameters;

public record CreatePostParams
{
    public required string Body { get; init; }
    public required IReadOnlyList<string> Profiles { get; init; }
    public IReadOnlyList<string>? Media { get; init; }
    public IReadOnlyList<string>? MediaFiles { get; init; }
    public PlatformParams? Platforms { get; init; }
    public DateTimeOffset? ScheduledAt { get; init; }
    public bool? Draft { get; init; }
    public string? ProfileGroupId { get; init; }
}
