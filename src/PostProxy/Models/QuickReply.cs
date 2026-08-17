using System.Text.Json.Serialization;

namespace PostProxy.Models;

/// <summary>
/// A tappable chip rendered above the participant's composer, gone once tapped.
/// </summary>
/// <remarks>
/// Facebook and Instagram only; up to 13 per send. <see cref="ContentType"/> is optional on
/// send (only "text" is accepted) and always present on responses. <see cref="Title"/> is
/// capped at 20 characters and <see cref="Payload"/> at 1000; both are required.
/// </remarks>
public record QuickReply
{
    [JsonPropertyName("content_type")]
    public string? ContentType { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("payload")]
    public required string Payload { get; init; }
}
