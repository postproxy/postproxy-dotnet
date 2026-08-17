using System.Text.Json.Serialization;

namespace PostProxy.Models;

/// <summary>
/// A button attached to the message itself, delivered as a Meta generic template.
/// </summary>
/// <remarks>
/// Facebook and Instagram only; up to 3 per send. <see cref="Url"/> is required and must be
/// https when <see cref="Type"/> is "web_url"; <see cref="Payload"/> is required when
/// <see cref="Type"/> is "postback". <see cref="Type"/> is a plain string rather than an enum
/// so a new Meta button type needs no SDK release.
/// </remarks>
public record MessageButton
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("url")]
    public string? Url { get; init; }

    [JsonPropertyName("payload")]
    public string? Payload { get; init; }

    /// <summary>A button that opens a link. The URL must be https.</summary>
    public static MessageButton WebUrl(string title, string url) =>
        new() { Type = "web_url", Title = title, Url = url };

    /// <summary>A button that posts your payload back as an inbound message.</summary>
    public static MessageButton Postback(string title, string payload) =>
        new() { Type = "postback", Title = title, Payload = payload };
}
