namespace PostProxy.Parameters;

public record DeleteOnPlatformParams
{
    public string? PostProfileId { get; init; }
    public string? ProfileId { get; init; }
    public string? Network { get; init; }
    public string? ProfileGroupId { get; init; }
}
