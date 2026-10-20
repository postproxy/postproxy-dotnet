using System.Text.Json;
using System.Text.Json.Serialization;

namespace PostProxy.Models;

/// <summary>
/// Alt text for a post's media: a single string for every item, or one per
/// media item in order. Assign a <c>string</c> or a <c>string[]</c> directly,
/// or use <see cref="PerMedia"/>.
/// </summary>
[JsonConverter(typeof(AltTextJsonConverter))]
public sealed record AltText
{
    private AltText(string? single, IReadOnlyList<string>? items)
    {
        Single = single;
        Items = items;
    }

    /// <summary>Set when one string applies to every media item.</summary>
    public string? Single { get; }

    /// <summary>Set when there is one string per media item.</summary>
    public IReadOnlyList<string>? Items { get; }

    public static AltText Of(string text) => new(text, null);

    public static AltText PerMedia(params string[] texts) => new(null, texts);

    public static implicit operator AltText(string text) => Of(text);

    public static implicit operator AltText(string[] texts) => PerMedia(texts);
}

internal sealed class AltTextJsonConverter : JsonConverter<AltText>
{
    public override AltText Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.StartArray
            ? AltText.PerMedia(JsonSerializer.Deserialize<string[]>(ref reader, options) ?? [])
            : AltText.Of(reader.GetString() ?? string.Empty);

    public override void Write(Utf8JsonWriter writer, AltText value, JsonSerializerOptions options)
    {
        if (value.Items is not null)
            JsonSerializer.Serialize(writer, value.Items, options);
        else
            writer.WriteStringValue(value.Single);
    }
}
