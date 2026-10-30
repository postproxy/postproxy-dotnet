using System.Text.Json;
using PostProxy.Models;

namespace PostProxy.Resources;

public class ProfilesResource
{
    private readonly PostProxyHttpClient _client;
    private readonly string? _defaultProfileGroupId;

    internal ProfilesResource(PostProxyHttpClient client, string? defaultProfileGroupId)
    {
        _client = client;
        _defaultProfileGroupId = defaultProfileGroupId;
    }

    public Task<ListResponse<Profile>> ListAsync(CancellationToken cancellationToken = default) =>
        ListAsync(null, cancellationToken);

    public Task<ListResponse<Profile>> ListAsync(string? profileGroupId, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        var pgId = profileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null)
            query["profile_group_id"] = pgId;

        return _client.GetAsync<ListResponse<Profile>>("/api/profiles", query, cancellationToken);
    }

    public Task<Profile> GetAsync(string id, CancellationToken cancellationToken = default) =>
        GetAsync(id, null, cancellationToken);

    public Task<Profile> GetAsync(string id, string? profileGroupId, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        var pgId = profileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null)
            query["profile_group_id"] = pgId;

        return _client.GetAsync<Profile>($"/api/profiles/{Uri.EscapeDataString(id)}", query, cancellationToken);
    }

    public Task<ListResponse<Placement>> PlacementsAsync(string id, CancellationToken cancellationToken = default) =>
        PlacementsAsync(id, null, cancellationToken);

    public Task<ListResponse<Placement>> PlacementsAsync(string id, string? profileGroupId, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        var pgId = profileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null)
            query["profile_group_id"] = pgId;

        return _client.GetAsync<ListResponse<Placement>>($"/api/profiles/{Uri.EscapeDataString(id)}/placements", query, cancellationToken);
    }

    /// <summary>
    /// Fetch the profile stats timeseries. <paramref name="placementId"/> is
    /// required for facebook, linkedin, and telegram profiles.
    /// </summary>
    public Task<ProfileStatsResponse> GetProfileStatsAsync(
        string id,
        string? placementId = null,
        string? from = null,
        string? to = null,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        if (placementId is not null) query["placement_id"] = placementId;
        if (from is not null) query["from"] = from;
        if (to is not null) query["to"] = to;
        var pgId = profileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null) query["profile_group_id"] = pgId;

        return _client.GetAsync<ProfileStatsResponse>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/stats", query, cancellationToken);
    }

    /// <summary>
    /// Per-day stats series. <see cref="DailyStatsAttribution.Publish"/> attributes post
    /// metrics to the day the post was published instead of the day they were received.
    /// </summary>
    public Task<DailyStatsResponse> GetDailyStatsAsync(
        string id,
        string? placementId = null,
        string? from = null,
        string? to = null,
        DailyStatsAttribution? attribution = null,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(profileGroupId);
        if (placementId is not null) query["placement_id"] = placementId;
        if (from is not null) query["from"] = from;
        if (to is not null) query["to"] = to;
        if (attribution is not null) query["attribution"] = PostProxyHttpClient.EnumValue(attribution.Value);

        return _client.GetAsync<DailyStatsResponse>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/daily_stats", query, cancellationToken);
    }

    public Task<FollowerHistoryResponse> GetFollowerHistoryAsync(
        string id,
        string? placementId = null,
        string? from = null,
        string? to = null,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(profileGroupId);
        if (placementId is not null) query["placement_id"] = placementId;
        if (from is not null) query["from"] = from;
        if (to is not null) query["to"] = to;

        return _client.GetAsync<FollowerHistoryResponse>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/follower_history", query, cancellationToken);
    }

    /// <summary>Instagram only: whether a DM participant is verified and follows you.</summary>
    public Task<ParticipantStatus> GetParticipantStatusAsync(
        string id,
        string userId,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default)
    {
        return _client.GetAsync<ParticipantStatus>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/participants/{Uri.EscapeDataString(userId)}", BuildQuery(profileGroupId), cancellationToken);
    }

    /// <summary>
    /// LinkedIn only: resolves a company page URL (or vanity name) to the URN used to
    /// @-mention it in a post.
    /// </summary>
    public Task<ResolvedMention> ResolveMentionAsync(
        string id,
        string url,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(profileGroupId);
        query["url"] = url;

        return _client.GetAsync<ResolvedMention>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/resolve_mention", query, cancellationToken);
    }

    /// <summary>
    /// Music and original sounds an Instagram reel can attach via
    /// <c>InstagramParams.AudioId</c>. Profiles connected via Facebook login only.
    /// </summary>
    public Task<InstagramAvailableAudioResponse> GetInstagramAvailableAudioAsync(
        string id,
        InstagramAudioType? audioType = null,
        string? searchQuery = null,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(profileGroupId);
        if (audioType is not null) query["audio_type"] = PostProxyHttpClient.EnumValue(audioType.Value);
        if (searchQuery is not null) query["search_query"] = searchQuery;

        return _client.GetAsync<InstagramAvailableAudioResponse>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/instagram/available_audio", query, cancellationToken);
    }

    /// <summary>
    /// Posts from other accounts that invited this Instagram profile as a collaborator.
    /// Profiles connected via Facebook login only. Pass <c>NextCursor</c> as
    /// <paramref name="after"/> for the next page.
    /// </summary>
    public Task<InstagramCollaborationInvitesResponse> ListInstagramCollaborationInvitesAsync(
        string id,
        string? after = null,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(profileGroupId);
        if (after is not null) query["after"] = after;

        return _client.GetAsync<InstagramCollaborationInvitesResponse>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/instagram/collaboration_invites", query, cancellationToken);
    }

    public Task<InstagramCollaborationInviteResult> AcceptInstagramCollaborationInviteAsync(
        string id,
        string mediaId,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["media_id"] = mediaId };

        return _client.PostAsync<InstagramCollaborationInviteResult>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/instagram/accept_collaboration_invite", body, BuildQuery(profileGroupId), cancellationToken, idempotencyKey);
    }

    public Task<InstagramCollaborationInviteResult> DeclineInstagramCollaborationInviteAsync(
        string id,
        string mediaId,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["media_id"] = mediaId };

        return _client.PostAsync<InstagramCollaborationInviteResult>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/instagram/decline_collaboration_invite", body, BuildQuery(profileGroupId), cancellationToken, idempotencyKey);
    }

    /// <summary>
    /// Pinterest only. Boards are the profile's placements; the new board's <c>Id</c>
    /// is the <c>BoardId</c> you publish to.
    /// </summary>
    public Task<Placement> CreatePinterestBoardAsync(
        string id,
        string name,
        string? description = null,
        PinterestBoardPrivacy? privacy = null,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["name"] = name };
        if (description is not null) body["description"] = description;
        if (privacy is not null) body["privacy"] = PostProxyHttpClient.EnumValue(privacy.Value);

        return _client.PostAsync<Placement>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/pinterest/create_board", body, BuildQuery(profileGroupId), cancellationToken, idempotencyKey);
    }

    /// <summary>Also deletes every Pin on the board, on Pinterest. Cannot be undone.</summary>
    public Task<PinterestBoardDeleted> DeletePinterestBoardAsync(
        string id,
        string boardId,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(profileGroupId);
        query["board_id"] = boardId;

        return _client.DeleteAsync<PinterestBoardDeleted>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/pinterest/delete_board", query, cancellationToken, idempotencyKey);
    }

    /// <summary>YouTube only. Up to 50 playlists per page; pass <c>NextPageToken</c> as <paramref name="pageToken"/>.</summary>
    public Task<YouTubePlaylistsResponse> ListYouTubePlaylistsAsync(
        string id,
        string? pageToken = null,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(profileGroupId);
        if (pageToken is not null) query["page_token"] = pageToken;

        return _client.GetAsync<YouTubePlaylistsResponse>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/youtube/playlists", query, cancellationToken);
    }

    public Task<YouTubePlaylist> CreateYouTubePlaylistAsync(
        string id,
        string title,
        string? description = null,
        YouTubePrivacy? privacyStatus = null,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["title"] = title };
        if (description is not null) body["description"] = description;
        if (privacyStatus is not null) body["privacy_status"] = PostProxyHttpClient.EnumValue(privacyStatus.Value);

        return _client.PostAsync<YouTubePlaylist>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/youtube/create_playlist", body, BuildQuery(profileGroupId), cancellationToken, idempotencyKey);
    }

    /// <summary>Fields left <c>null</c> keep their current value; an empty <paramref name="description"/> clears it.</summary>
    public Task<YouTubePlaylist> UpdateYouTubePlaylistAsync(
        string id,
        string playlistId,
        string? title = null,
        string? description = null,
        YouTubePrivacy? privacyStatus = null,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["playlist_id"] = playlistId };
        if (title is not null) body["title"] = title;
        if (description is not null) body["description"] = description;
        if (privacyStatus is not null) body["privacy_status"] = PostProxyHttpClient.EnumValue(privacyStatus.Value);

        return _client.PatchAsync<YouTubePlaylist>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/youtube/update_playlist", body, BuildQuery(profileGroupId), cancellationToken, idempotencyKey);
    }

    /// <summary>The videos in the playlist are not deleted.</summary>
    public Task<YouTubePlaylistDeleted> DeleteYouTubePlaylistAsync(
        string id,
        string playlistId,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(profileGroupId);
        query["playlist_id"] = playlistId;

        return _client.DeleteAsync<YouTubePlaylistDeleted>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/youtube/delete_playlist", query, cancellationToken, idempotencyKey);
    }

    /// <summary>Facebook only. <paramref name="pageId"/> is the placement id from <see cref="PlacementsAsync(string, CancellationToken)"/>.</summary>
    public Task<JsonElement> GetFacebookPageAsync(
        string id,
        string pageId,
        string? fields = null,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(profileGroupId);
        query["page_id"] = pageId;
        if (fields is not null) query["fields"] = fields;

        return _client.GetAsync<JsonElement>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/facebook/page", query, cancellationToken);
    }

    /// <summary>
    /// Updates the Page's about / contact fields, e.g. <c>about</c> (255 characters or
    /// fewer), <c>description</c>, <c>website</c>, <c>phone</c>, <c>emails</c>.
    /// </summary>
    public Task<JsonElement> UpdateFacebookPageAsync(
        string id,
        string pageId,
        IReadOnlyDictionary<string, object?> fields,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?>();
        foreach (var (key, value) in fields)
        {
            if (value is not null) body[key] = PostProxyHttpClient.Raw(value);
        }
        body["page_id"] = pageId;

        return _client.PatchAsync<JsonElement>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/facebook/update_page", body, BuildQuery(profileGroupId), cancellationToken, idempotencyKey);
    }

    /// <summary>Moves the profile (with its placements) to another profile group.</summary>
    public Task<Profile> AssignToGroupAsync(
        string id,
        string targetProfileGroupId,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["target_profile_group_id"] = targetProfileGroupId };

        return _client.PatchAsync<Profile>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/assign_to_group", body, BuildQuery(profileGroupId), cancellationToken, idempotencyKey);
    }

    /// <summary>
    /// Moves a placement (e.g. a Facebook Page or Telegram channel) to another
    /// profile group. <paramref name="placementId"/> is the placement's
    /// external ID as returned by <see cref="PlacementsAsync(string, CancellationToken)"/>.
    /// </summary>
    public Task<AssignedPlacement> AssignPlacementToGroupAsync(
        string id,
        string placementId,
        string targetProfileGroupId,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        var pgId = profileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null)
            query["profile_group_id"] = pgId;

        var body = new Dictionary<string, object?>
        {
            ["placement_id"] = placementId,
            ["target_profile_group_id"] = targetProfileGroupId,
        };

        return _client.PatchAsync<AssignedPlacement>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/assign_placement_to_group", body, query, cancellationToken, idempotencyKey);
    }

    /// <summary>
    /// Imports older posts from the platform. Walks the profile's feed
    /// backwards from the newest post until it reaches <paramref name="from"/>
    /// or the platform stops returning posts. Runs in the background — poll
    /// <see cref="GetPostSyncAsync"/> with the returned id for progress.
    /// </summary>
    /// <remarks>
    /// Only one backfill runs per profile at a time; starting a second throws
    /// <see cref="Exceptions.ConflictException"/> carrying the running one's
    /// <c>profile_sync_id</c>. How far back a run reaches depends on the
    /// platform's API, not on PostProxy.
    /// </remarks>
    public Task<PostSync> BackfillPostsAsync(
        string id,
        string from,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        var pgId = profileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null)
            query["profile_group_id"] = pgId;

        var body = new Dictionary<string, object?> { ["from"] = from };

        return _client.PostAsync<PostSync>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/backfill_posts", body, query, cancellationToken, idempotencyKey);
    }

    /// <summary>
    /// Lists post sync runs for a profile, newest first. Runs are kept for 30 days.
    /// </summary>
    public Task<PaginatedResponse<PostSync>> ListPostSyncsAsync(
        string id,
        PostSyncTrigger? trigger = null,
        PostSyncStatus? status = null,
        int? page = null,
        int? perPage = null,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        if (trigger is not null) query["trigger"] = trigger.Value.ToString().ToLowerInvariant();
        if (status is not null) query["status"] = status.Value.ToString().ToLowerInvariant();
        if (page is not null) query["page"] = page.Value.ToString();
        if (perPage is not null) query["per_page"] = perPage.Value.ToString();
        var pgId = profileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null) query["profile_group_id"] = pgId;

        return _client.GetAsync<PaginatedResponse<PostSync>>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/post_syncs", query, cancellationToken);
    }

    /// <summary>
    /// Fetches a single run. Poll this to follow a backfill to completion — the
    /// run is finished when its status is Completed or Failed.
    /// </summary>
    public Task<PostSync> GetPostSyncAsync(
        string id,
        string postSyncId,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        var pgId = profileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null)
            query["profile_group_id"] = pgId;

        return _client.GetAsync<PostSync>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/post_syncs/{Uri.EscapeDataString(postSyncId)}", query, cancellationToken);
    }

    /// <summary>
    /// Imports DM history back to <paramref name="from"/>. Like
    /// <see cref="BackfillPostsAsync"/>, runs in the background and only one runs per profile.
    /// </summary>
    public Task<ChatSync> BackfillChatsAsync(
        string id,
        string from,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["from"] = from };

        return _client.PostAsync<ChatSync>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/backfill_chats", body, BuildQuery(profileGroupId), cancellationToken, idempotencyKey);
    }

    public Task<PaginatedResponse<ChatSync>> ListChatSyncsAsync(
        string id,
        PostSyncTrigger? trigger = null,
        PostSyncStatus? status = null,
        int? page = null,
        int? perPage = null,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(profileGroupId);
        if (trigger is not null) query["trigger"] = PostProxyHttpClient.EnumValue(trigger.Value);
        if (status is not null) query["status"] = PostProxyHttpClient.EnumValue(status.Value);
        if (page is not null) query["page"] = page.Value.ToString();
        if (perPage is not null) query["per_page"] = perPage.Value.ToString();

        return _client.GetAsync<PaginatedResponse<ChatSync>>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/chat_syncs", query, cancellationToken);
    }

    public Task<ChatSync> GetChatSyncAsync(
        string id,
        string chatSyncId,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default)
    {
        return _client.GetAsync<ChatSync>(
            $"/api/profiles/{Uri.EscapeDataString(id)}/chat_syncs/{Uri.EscapeDataString(chatSyncId)}", BuildQuery(profileGroupId), cancellationToken);
    }

    /// <summary>
    /// Lists DM ice breakers. Supported for Instagram profiles only.
    /// </summary>
    public Task<IceBreakersResponse> GetIceBreakersAsync(string id, CancellationToken cancellationToken = default) =>
        GetIceBreakersAsync(id, null, cancellationToken);

    public Task<IceBreakersResponse> GetIceBreakersAsync(string id, string? profileGroupId, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        var pgId = profileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null)
            query["profile_group_id"] = pgId;

        return _client.GetAsync<IceBreakersResponse>($"/api/profiles/{Uri.EscapeDataString(id)}/ice_breakers", query, cancellationToken);
    }

    /// <summary>
    /// Replaces the DM ice breakers for a profile (1-4 items).
    /// </summary>
    public Task<SuccessResponse> SetIceBreakersAsync(string id, IReadOnlyList<IceBreaker> iceBreakers, CancellationToken cancellationToken = default) =>
        SetIceBreakersAsync(id, iceBreakers, null, cancellationToken);

    public Task<SuccessResponse> SetIceBreakersAsync(string id, IReadOnlyList<IceBreaker> iceBreakers, string? profileGroupId, CancellationToken cancellationToken = default) =>
        SetIceBreakersAsync(id, iceBreakers, profileGroupId, null, cancellationToken);

    public Task<SuccessResponse> SetIceBreakersAsync(string id, IReadOnlyList<IceBreaker> iceBreakers, string? profileGroupId, string? idempotencyKey, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        var pgId = profileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null)
            query["profile_group_id"] = pgId;

        var body = new Dictionary<string, object?> { ["ice_breakers"] = iceBreakers };

        return _client.PostAsync<SuccessResponse>($"/api/profiles/{Uri.EscapeDataString(id)}/ice_breakers", body, query, cancellationToken, idempotencyKey);
    }

    public Task<SuccessResponse> DeleteIceBreakersAsync(string id, CancellationToken cancellationToken = default) =>
        DeleteIceBreakersAsync(id, null, cancellationToken);

    public Task<SuccessResponse> DeleteIceBreakersAsync(string id, string? profileGroupId, CancellationToken cancellationToken = default) =>
        DeleteIceBreakersAsync(id, profileGroupId, null, cancellationToken);

    public Task<SuccessResponse> DeleteIceBreakersAsync(string id, string? profileGroupId, string? idempotencyKey, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        var pgId = profileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null)
            query["profile_group_id"] = pgId;

        return _client.DeleteAsync<SuccessResponse>($"/api/profiles/{Uri.EscapeDataString(id)}/ice_breakers", query, cancellationToken, idempotencyKey);
    }

    public Task<SuccessResponse> DeleteAsync(string id, CancellationToken cancellationToken = default) =>
        DeleteAsync(id, null, cancellationToken);

    public Task<SuccessResponse> DeleteAsync(string id, string? profileGroupId, CancellationToken cancellationToken = default) =>
        DeleteAsync(id, profileGroupId, null, cancellationToken);

    public Task<SuccessResponse> DeleteAsync(string id, string? profileGroupId, string? idempotencyKey, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        var pgId = profileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null)
            query["profile_group_id"] = pgId;

        return _client.DeleteAsync<SuccessResponse>($"/api/profiles/{Uri.EscapeDataString(id)}", query, cancellationToken, idempotencyKey);
    }

    private Dictionary<string, string> BuildQuery(string? profileGroupId)
    {
        var query = new Dictionary<string, string>();
        var pgId = profileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null) query["profile_group_id"] = pgId;
        return query;
    }
}
