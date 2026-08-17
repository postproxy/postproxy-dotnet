using System.Text.Json.Serialization;

namespace PostProxy.Models;

/// <summary>
/// Extra fields for the generic-template element that carries <c>Buttons</c>, for a richer
/// product-style card.
/// </summary>
/// <remarks>
/// Requires <c>Buttons</c>. <see cref="Subtitle"/> is capped at 80 characters, and both
/// <see cref="ImageUrl"/> and the default action's URL must be https.
/// </remarks>
public record MessageCard
{
    [JsonPropertyName("subtitle")]
    public string? Subtitle { get; init; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; init; }

    [JsonPropertyName("default_action")]
    public CardDefaultAction? DefaultAction { get; init; }
}
