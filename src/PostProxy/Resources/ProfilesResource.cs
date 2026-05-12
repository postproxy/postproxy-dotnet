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

    public Task<SuccessResponse> DeleteAsync(string id, CancellationToken cancellationToken = default) =>
        DeleteAsync(id, null, cancellationToken);

    public Task<SuccessResponse> DeleteAsync(string id, string? profileGroupId, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        var pgId = profileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null)
            query["profile_group_id"] = pgId;

        return _client.DeleteAsync<SuccessResponse>($"/api/profiles/{Uri.EscapeDataString(id)}", query, cancellationToken);
    }
}
