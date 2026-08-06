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
}
