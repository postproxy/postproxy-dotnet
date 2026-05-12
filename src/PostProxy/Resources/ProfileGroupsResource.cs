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

    /// <summary>
    /// Initialize an OAuth connection. For BlueSky and Telegram use
    /// <see cref="ConnectBlueskyAsync"/> and <see cref="ConnectTelegramAsync"/>.
    /// </summary>
    public Task<ConnectionResponse> InitializeConnectionAsync(string id, Platform platform, string? redirectUrl = null, CancellationToken cancellationToken = default)
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

    public Task<BlueskyConnectionResponse> ConnectBlueskyAsync(string id, string identifier, string appPassword, CancellationToken cancellationToken = default)
    {
        var body = new BlueskyConnectBody
        {
            Identifier = identifier,
            AppPassword = appPassword,
        };

        return _client.PostAsync<BlueskyConnectionResponse>(
            $"/api/profile_groups/{Uri.EscapeDataString(id)}/initialize_connection",
            body,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Submit a Telegram bot token. Channels populate asynchronously — poll
    /// <see cref="ProfilesResource.PlacementsAsync(string, CancellationToken)"/>
    /// until it returns at least one placement.
    /// </summary>
    public Task<TelegramConnectionResponse> ConnectTelegramAsync(string id, string botToken, CancellationToken cancellationToken = default)
    {
        var body = new TelegramConnectBody { BotToken = botToken };

        return _client.PostAsync<TelegramConnectionResponse>(
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
        public string? RedirectUrl { get; init; }
    }

    private record BlueskyConnectBody
    {
        [JsonPropertyName("platform")]
        public Platform Platform { get; init; } = Platform.Bluesky;

        [JsonPropertyName("identifier")]
        public required string Identifier { get; init; }

        [JsonPropertyName("app_password")]
        public required string AppPassword { get; init; }
    }

    private record TelegramConnectBody
    {
        [JsonPropertyName("platform")]
        public Platform Platform { get; init; } = Platform.Telegram;

        [JsonPropertyName("bot_token")]
        public required string BotToken { get; init; }
    }
}
