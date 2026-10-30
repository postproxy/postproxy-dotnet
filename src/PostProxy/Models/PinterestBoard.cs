using System.Text.Json.Serialization;

namespace PostProxy.Models;

/// <summary>Result of <c>Profiles.DeletePinterestBoardAsync</c>.</summary>
public record PinterestBoardDeleted
{
    [JsonPropertyName("deleted")]
    public bool Deleted { get; init; }

    [JsonPropertyName("board_id")]
    public required string BoardId { get; init; }
}
