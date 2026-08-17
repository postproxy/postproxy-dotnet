using System.Text.Json.Serialization;

namespace PostProxy.Models;

/// <summary>The tap-through on a <see cref="MessageCard"/>. The URL must be https.</summary>
public record CardDefaultAction
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("url")]
    public required string Url { get; init; }

    public static CardDefaultAction WebUrl(string url) =>
        new() { Type = "web_url", Url = url };
}
