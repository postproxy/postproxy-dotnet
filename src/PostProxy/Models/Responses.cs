using System.Text.Json.Serialization;

namespace PostProxy.Models;

public record ListResponse<T>
{
    [JsonPropertyName("data")]
    public required IReadOnlyList<T> Data { get; init; }
}

public record PaginatedResponse<T>
{
    [JsonPropertyName("data")]
    public required IReadOnlyList<T> Data { get; init; }

    [JsonPropertyName("total")]
    public int Total { get; init; }

    [JsonPropertyName("page")]
    public int Page { get; init; }

    [JsonPropertyName("per_page")]
    public int PerPage { get; init; }
}

public record DeleteResponse
{
    [JsonPropertyName("deleted")]
    public bool Deleted { get; init; }
}

public record DeletingPlatform
{
    [JsonPropertyName("post_profile_id")]
    public required string PostProfileId { get; init; }

    [JsonPropertyName("platform")]
    public required Platform Platform { get; init; }
}

public record DeleteOnPlatformResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("deleting")]
    public IReadOnlyList<DeletingPlatform> Deleting { get; init; } = Array.Empty<DeletingPlatform>();
}

public record SuccessResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }
}

public record ConnectionResponse
{
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    [JsonPropertyName("success")]
    public bool Success { get; init; }
}
