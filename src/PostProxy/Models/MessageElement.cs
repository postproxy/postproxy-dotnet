using System.Text.Json.Serialization;

namespace PostProxy.Models;

/// <summary>
/// One card of a horizontally scrolling carousel.
/// </summary>
/// <remarks>
/// Facebook and Instagram only; up to 10 per send. <see cref="Title"/> (80 characters or
/// fewer) plus at least one other field is required.
/// </remarks>
public record MessageElement
{
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("subtitle")]
    public string? Subtitle { get; init; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; init; }

    [JsonPropertyName("default_action")]
    public CardDefaultAction? DefaultAction { get; init; }

    /// <summary>Up to 3.</summary>
    [JsonPropertyName("buttons")]
    public IReadOnlyList<MessageButton>? Buttons { get; init; }
}
