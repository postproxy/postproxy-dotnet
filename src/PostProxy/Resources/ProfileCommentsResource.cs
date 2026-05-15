using PostProxy.Models;

namespace PostProxy.Resources;

public class ProfileCommentsResource
{
    private readonly PostProxyHttpClient _client;

    internal ProfileCommentsResource(PostProxyHttpClient client)
    {
        _client = client;
    }

    public Task<PaginatedResponse<ProfileComment>> ListAsync(string profileId, string? placementId = null, int? page = null, int? perPage = null, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        if (placementId is not null) query["placement_id"] = placementId;
        if (page is not null) query["page"] = page.Value.ToString();
        if (perPage is not null) query["per_page"] = perPage.Value.ToString();

        return _client.GetAsync<PaginatedResponse<ProfileComment>>($"/api/profiles/{Uri.EscapeDataString(profileId)}/comments", query, cancellationToken);
    }

    public Task<ProfileComment> GetAsync(string profileId, string commentId, CancellationToken cancellationToken = default)
    {
        return _client.GetAsync<ProfileComment>($"/api/profiles/{Uri.EscapeDataString(profileId)}/comments/{Uri.EscapeDataString(commentId)}", null, cancellationToken);
    }

    public Task<ProfileComment> CreateAsync(string profileId, string parentId, string text, CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object> { ["parent_id"] = parentId, ["text"] = text };

        return _client.PostAsync<ProfileComment>($"/api/profiles/{Uri.EscapeDataString(profileId)}/comments", body, null, cancellationToken);
    }

    public Task<AcceptedResponse> DeleteAsync(string profileId, string commentId, CancellationToken cancellationToken = default)
    {
        return _client.DeleteAsync<AcceptedResponse>($"/api/profiles/{Uri.EscapeDataString(profileId)}/comments/{Uri.EscapeDataString(commentId)}", null, cancellationToken);
    }
}
