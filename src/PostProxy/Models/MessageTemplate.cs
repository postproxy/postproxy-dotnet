using System.Text.Json;
using System.Text.Json.Serialization;

namespace PostProxy.Models;

/// <summary>
/// A WhatsApp template send. Address the template by <see cref="Id"/> (PostProxy or Meta
/// id) or by <see cref="Name"/> (+ <see cref="Language"/> when it exists in several).
/// </summary>
public record MessageTemplateInput
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("language")]
    public string? Language { get; init; }

    /// <summary>Positional values: header, then body, then dynamic URL buttons.</summary>
    [JsonPropertyName("variables")]
    public IReadOnlyList<string>? Variables { get; init; }

    [JsonPropertyName("button_params")]
    public IReadOnlyList<TemplateButtonParam>? ButtonParams { get; init; }

    /// <summary>Media for an image / video / document header.</summary>
    [JsonPropertyName("header_media")]
    public TemplateHeaderMedia? HeaderMedia { get; init; }

    [JsonPropertyName("header_location")]
    public MessageLocation? HeaderLocation { get; init; }
}

/// <summary>A template header's media, by public <see cref="Link"/> or uploaded media <see cref="Id"/>.</summary>
public record TemplateHeaderMedia
{
    [JsonPropertyName("link")]
    public string? Link { get; init; }

    [JsonPropertyName("id")]
    public string? Id { get; init; }
}

/// <summary>
/// A parameter for one of the template's buttons. <see cref="SubType"/> is <c>url</c>,
/// <c>copy_code</c>, <c>flow</c>, <c>quick_reply</c> or <c>voice_call</c>; the remaining
/// fields Meta expects for it (e.g. <c>text</c>) go in <see cref="Fields"/>.
/// </summary>
public record TemplateButtonParam
{
    [JsonPropertyName("index")]
    public int Index { get; init; }

    [JsonPropertyName("sub_type")]
    public required string SubType { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Fields { get; init; }
}

public record MessageLocation
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; init; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("address")]
    public string? Address { get; init; }
}

/// <summary>The template a WhatsApp message was sent from.</summary>
public record MessageTemplateRef
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("external_id")]
    public string? ExternalId { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("language")]
    public string? Language { get; init; }

    [JsonPropertyName("variables")]
    public IReadOnlyList<string>? Variables { get; init; }
}
