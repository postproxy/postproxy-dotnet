using System.Text.Json;

namespace PostProxy.Parameters;

public record EditMessageParams
{
    public string? Body { get; init; }
    public IReadOnlyDictionary<string, JsonElement>? ReplyMarkup { get; init; }
    public string? ProfileGroupId { get; init; }
}
