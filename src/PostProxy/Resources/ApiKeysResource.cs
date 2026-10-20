using PostProxy.Models;

namespace PostProxy.Resources;

/// <summary>Manages profile-group-scoped API keys. Requires an account-wide key.</summary>
public class ApiKeysResource
{
    private readonly PostProxyHttpClient _client;

    internal ApiKeysResource(PostProxyHttpClient client)
    {
        _client = client;
    }

    public Task<ListResponse<ApiKey>> ListAsync(CancellationToken cancellationToken = default) =>
        _client.GetAsync<ListResponse<ApiKey>>("/api/api_keys", cancellationToken: cancellationToken);

    /// <summary>The returned <see cref="ApiKey.Token"/> is shown only once.</summary>
    public Task<ApiKey> CreateAsync(string name, string profileGroupId, string? idempotencyKey = null, CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object>
        {
            ["name"] = name,
            ["profile_group_id"] = profileGroupId,
        };

        return _client.PostAsync<ApiKey>("/api/api_keys", body, cancellationToken: cancellationToken, idempotencyKey: idempotencyKey);
    }

    public Task<DeleteResponse> DeleteAsync(string id, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        _client.DeleteAsync<DeleteResponse>($"/api/api_keys/{Uri.EscapeDataString(id)}", cancellationToken: cancellationToken, idempotencyKey: idempotencyKey);
}
