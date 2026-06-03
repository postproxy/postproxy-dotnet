using PostProxy.Models;

namespace PostProxy.Parameters;

public record ListMessagesParams
{
    public int? Page { get; init; }
    public int? PerPage { get; init; }
    public MessageDirection? Direction { get; init; }
    public MessageStatus? Status { get; init; }
    public string? ProfileGroupId { get; init; }
}
