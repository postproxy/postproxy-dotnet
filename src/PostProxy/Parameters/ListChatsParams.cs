namespace PostProxy.Parameters;

public record ListChatsParams
{
    public int? Page { get; init; }
    public int? PerPage { get; init; }
    public DateTimeOffset? Before { get; init; }
    public DateTimeOffset? After { get; init; }
    public string? ProfileGroupId { get; init; }
}
