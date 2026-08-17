using System.Text.Json.Serialization;

namespace PostProxy.Models;

/// <summary>
/// Set on inbound messages created by a tap on an interactive element you sent.
/// </summary>
/// <remarks>
/// Derived from <c>PlatformData</c> rather than stored, so it also resolves for taps ingested
/// before PostProxy exposed this field. <see cref="TappedActionKind.CallbackQuery"/> is
/// Telegram, so this is not Meta-only even though the send parameters are.
/// </remarks>
public record TappedAction
{
    [JsonPropertyName("kind")]
    public required TappedActionKind Kind { get; init; }

    /// <summary>The payload you set on the quick reply, button, or ice breaker.</summary>
    [JsonPropertyName("payload")]
    public required string Payload { get; init; }

    /// <summary>The label the participant tapped.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }
}
