using System.Text.Json;

namespace PostProxy.Parameters;

public record SendMessageParams
{
    public string? Body { get; init; }
    public IReadOnlyList<string>? Media { get; init; }
    public IReadOnlyList<string>? MediaFiles { get; init; }
    public string? Tag { get; init; }
    public string? ReplyToExternalId { get; init; }
    public IReadOnlyDictionary<string, JsonElement>? ReplyMarkup { get; init; }
    public string? ProfileGroupId { get; init; }
}
