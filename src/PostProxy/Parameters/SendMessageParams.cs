using System.Text.Json.Serialization;
using System.Text.Json;
using PostProxy.Models;

namespace PostProxy.Parameters;

public record SendMessageParams
{
    public string? Body { get; init; }
    public IReadOnlyList<string>? Media { get; init; }
    public IReadOnlyList<string>? MediaFiles { get; init; }
    public string? Tag { get; init; }
    public string? ReplyToExternalId { get; init; }
    public IReadOnlyDictionary<string, JsonElement>? ReplyMarkup { get; init; }

    /// <summary>
    /// Up to 13 tappable chips above the participant's composer. Facebook and Instagram only —
    /// 422 on Telegram and Bluesky, where <see cref="ReplyMarkup"/> is the equivalent. Sent on
    /// the JSON path only, so pass <see cref="Media"/> as hosted URLs rather than
    /// <see cref="MediaFiles"/> when combining with an attachment.
    /// </summary>
    public IReadOnlyList<QuickReply>? QuickReplies { get; init; }

    /// <summary>
    /// Up to 3 buttons attached to the message. Facebook and Instagram only. Requires
    /// <see cref="Body"/>, which is capped at 80 characters when buttons are present, and cannot
    /// be combined with media.
    /// </summary>
    public IReadOnlyList<MessageButton>? Buttons { get; init; }

    /// <summary>Extra card fields for the element carrying <see cref="Buttons"/>. Requires them.</summary>
    public MessageCard? Card { get; init; }

    /// <summary>
    /// Carousel of up to 10 cards (Facebook and Instagram). Can't be combined with
    /// <see cref="Body"/>, <see cref="Buttons"/> or <see cref="Card"/>.
    /// </summary>
    public IReadOnlyList<MessageElement>? Elements { get; init; }

    /// <summary>
    /// WhatsApp only. Send exactly one of <see cref="Body"/>, <see cref="Media"/>,
    /// <see cref="Template"/>, <see cref="Interactive"/>, <see cref="Location"/> or
    /// <see cref="Contacts"/>. Outside the 24-hour window only a template gets through.
    /// </summary>
    public MessageTemplateInput? Template { get; init; }

    /// <summary>WhatsApp only: Meta's interactive message object.</summary>
    public object? Interactive { get; init; }

    /// <summary>WhatsApp only.</summary>
    public MessageLocation? Location { get; init; }

    /// <summary>WhatsApp only: Meta's contact objects.</summary>
    public IReadOnlyList<object>? Contacts { get; init; }

    /// <summary>WhatsApp only: <c>utility</c> sends a text message under the utility category.</summary>
    public string? Category { get; init; }

    /// <summary>WhatsApp only.</summary>
    public bool? LinkPreview { get; init; }

    /// <summary>WhatsApp only: deliver an audio attachment as a voice note.</summary>
    public bool? VoiceNote { get; init; }

    /// <summary>WhatsApp only: file name shown for a document attachment.</summary>
    public string? Filename { get; init; }
    public string? ProfileGroupId { get; init; }

    /// <summary>
    /// Optional Idempotency-Key. Retrying with the same key replays the
    /// original response instead of running the request again.
    /// </summary>
    [JsonIgnore]
    public string? IdempotencyKey { get; init; }
}
