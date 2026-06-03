namespace PostProxy.Parameters;

public record CreateChatParams
{
    public required string ParticipantExternalId { get; init; }
    public string? ParticipantUsername { get; init; }
    public string? ParticipantName { get; init; }
    public string? ProfileGroupId { get; init; }
}
