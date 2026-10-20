using PostProxy.Models;

namespace PostProxy.Resources;

/// <summary>Account-wide API keys only — placements span profile groups.</summary>
public class PlacementsResource
{
    private readonly PostProxyHttpClient _client;

    internal PlacementsResource(PostProxyHttpClient client)
    {
        _client = client;
    }

    public Task<PaginatedResponse<AccountPlacement>> ListAsync(int? page = null, int? perPage = null, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        if (page is not null) query["page"] = page.Value.ToString();
        if (perPage is not null) query["per_page"] = perPage.Value.ToString();

        return _client.GetAsync<PaginatedResponse<AccountPlacement>>("/api/placements", query, cancellationToken);
    }

    /// <summary>
    /// When the same placement is reachable through several connected profiles,
    /// chooses which one PostProxy posts and reads through.
    /// </summary>
    public Task<AccountPlacement> SetActiveProfileAsync(string id, string profileId, string? idempotencyKey = null, CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object> { ["profile_id"] = profileId };

        return _client.PatchAsync<AccountPlacement>($"/api/placements/{Uri.EscapeDataString(id)}/set_active_profile", body, null, cancellationToken, idempotencyKey);
    }
}
