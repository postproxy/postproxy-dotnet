using System.Text.Json.Serialization;

namespace PostProxy.Models;

/// <summary>A post from another account that invited this Instagram profile as a collaborator.</summary>
public record InstagramCollaborationInvite
{
    [JsonPropertyName("media_id")]
    public required string MediaId { get; init; }

    [JsonPropertyName("media_owner_username")]
    public string? MediaOwnerUsername { get; init; }

    [JsonPropertyName("caption")]
    public string? Caption { get; init; }

    /// <summary>Left out by Instagram for videos with copyrighted or licensed audio.</summary>
    [JsonPropertyName("media_url")]
    public string? MediaUrl { get; init; }
}

public record InstagramCollaborationInvitesResponse
{
    [JsonPropertyName("collaboration_invites")]
    public IReadOnlyList<InstagramCollaborationInvite> CollaborationInvites { get; init; } =
        Array.Empty<InstagramCollaborationInvite>();

    /// <summary>Pass as <c>after</c> for the next page; <c>null</c> on the last page.</summary>
    [JsonPropertyName("next_cursor")]
    public string? NextCursor { get; init; }
}

/// <summary>Result of accepting or declining a collaboration invite.</summary>
public record InstagramCollaborationInviteResult
{
    [JsonPropertyName("media_id")]
    public required string MediaId { get; init; }

    /// <summary>Set when accepting.</summary>
    [JsonPropertyName("accepted")]
    public bool? Accepted { get; init; }

    /// <summary>Set when declining.</summary>
    [JsonPropertyName("declined")]
    public bool? Declined { get; init; }
}
