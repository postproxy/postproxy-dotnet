using System.Text.Json.Serialization;

namespace PostProxy.Models;

/// <summary>A search term that showed a Google Business location.</summary>
public record SearchKeyword
{
    [JsonPropertyName("keyword")]
    public required string Keyword { get; init; }

    /// <summary><c>null</c> for low-volume keywords; see <see cref="ImpressionsThreshold"/>.</summary>
    [JsonPropertyName("impressions")]
    public long? Impressions { get; init; }

    /// <summary>Upper bound when Google reports no exact count.</summary>
    [JsonPropertyName("impressions_threshold")]
    public long? ImpressionsThreshold { get; init; }
}

public record SearchKeywordsMonth
{
    /// <summary><c>YYYY-MM</c>.</summary>
    [JsonPropertyName("month")]
    public required string Month { get; init; }

    [JsonPropertyName("keywords")]
    public IReadOnlyList<SearchKeyword> Keywords { get; init; } = Array.Empty<SearchKeyword>();
}

/// <summary>Result of <c>GoogleBusiness.GetSearchKeywordsAsync</c>.</summary>
public record SearchKeywordsResponse
{
    [JsonPropertyName("months")]
    public IReadOnlyList<SearchKeywordsMonth> Months { get; init; } = Array.Empty<SearchKeywordsMonth>();
}
