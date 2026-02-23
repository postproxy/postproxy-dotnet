using PostProxy.Models;

namespace PostProxy.Parameters;

public record ListPostsParams
{
    public int? Page { get; init; }
    public int? PerPage { get; init; }
    public PostStatus? Status { get; init; }
    public IReadOnlyList<Platform>? Platforms { get; init; }
    public DateTimeOffset? ScheduledAfter { get; init; }
    public string? ProfileGroupId { get; init; }
}
