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
