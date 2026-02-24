namespace PostProxy.Parameters;

public record PostStatsParams
{
    public required IReadOnlyList<string> PostIds { get; init; }
    public IReadOnlyList<string>? Profiles { get; init; }
    public DateTimeOffset? From { get; init; }
    public DateTimeOffset? To { get; init; }
}
