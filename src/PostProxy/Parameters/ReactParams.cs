namespace PostProxy.Parameters;

public record ReactParams
{
    public string? Reaction { get; init; }
    public string? Emoji { get; init; }
    public string? ProfileGroupId { get; init; }
}
