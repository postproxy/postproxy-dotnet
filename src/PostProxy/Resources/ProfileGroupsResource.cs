using System.Text.Json.Serialization;
using PostProxy.Models;

namespace PostProxy.Resources;

public class ProfileGroupsResource
{
    private readonly PostProxyHttpClient _client;

    internal ProfileGroupsResource(PostProxyHttpClient client)
    {
        _client = client;
    }

    public Task<ListResponse<ProfileGroup>> ListAsync(CancellationToken cancellationToken = default) =>
        _client.GetAsync<ListResponse<ProfileGroup>>("/api/profile_groups", cancellationToken: cancellationToken);

    public Task<ProfileGroup> GetAsync(string id, CancellationToken cancellationToken = default) =>
        _client.GetAsync<ProfileGroup>($"/api/profile_groups/{Uri.EscapeDataString(id)}", cancellationToken: cancellationToken);

    public Task<ProfileGroup> CreateAsync(string name, CancellationToken cancellationToken = default)
    {
        var body = new CreateProfileGroupBody
        {
            ProfileGroup = new ProfileGroupNameBody { Name = name },
        };

        return _client.PostAsync<ProfileGroup>("/api/profile_groups", body, cancellationToken: cancellationToken);
    }

    public Task<DeleteResponse> DeleteAsync(string id, CancellationToken cancellationToken = default) =>
        _client.DeleteAsync<DeleteResponse>($"/api/profile_groups/{Uri.EscapeDataString(id)}", cancellationToken: cancellationToken);

    public Task<ConnectionResponse> InitializeConnectionAsync(string id, Platform platform, string redirectUrl, CancellationToken cancellationToken = default)
    {
        var body = new InitializeConnectionBody
        {
            Platform = platform,
            RedirectUrl = redirectUrl,
        };

        return _client.PostAsync<ConnectionResponse>(
            $"/api/profile_groups/{Uri.EscapeDataString(id)}/initialize_connection",
            body,
            cancellationToken: cancellationToken);
    }

    private record CreateProfileGroupBody
    {
        [JsonPropertyName("profile_group")]
        public required ProfileGroupNameBody ProfileGroup { get; init; }
    }

    private record ProfileGroupNameBody
    {
        [JsonPropertyName("name")]
        public required string Name { get; init; }
    }

    private record InitializeConnectionBody
    {
        [JsonPropertyName("platform")]
        public required Platform Platform { get; init; }

        [JsonPropertyName("redirect_url")]
        public required string RedirectUrl { get; init; }
    }
}
