using PostProxy.Resources;

namespace PostProxy;

public sealed class PostProxyClient
{
    private readonly PostProxyHttpClient _httpClient;
    private readonly string? _defaultProfileGroupId;

    private PostsResource? _posts;
    private ProfilesResource? _profiles;
    private ProfileGroupsResource? _profileGroups;
    private WebhooksResource? _webhooks;

    internal PostProxyClient(PostProxyHttpClient httpClient, string? defaultProfileGroupId)
    {
        _httpClient = httpClient;
        _defaultProfileGroupId = defaultProfileGroupId;
    }

    public PostsResource Posts => _posts ??= new PostsResource(_httpClient, _defaultProfileGroupId);
    public ProfilesResource Profiles => _profiles ??= new ProfilesResource(_httpClient, _defaultProfileGroupId);
    public ProfileGroupsResource ProfileGroups => _profileGroups ??= new ProfileGroupsResource(_httpClient);
    public WebhooksResource Webhooks => _webhooks ??= new WebhooksResource(_httpClient);

    public static PostProxyClientBuilder Builder(string apiKey) => new(apiKey);
}

public class PostProxyClientBuilder
{
    private readonly string _apiKey;
    private string _baseUrl = "https://api.postproxy.dev";
    private string? _profileGroupId;
    private HttpClient? _httpClient;

    internal PostProxyClientBuilder(string apiKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        _apiKey = apiKey;
    }

    public PostProxyClientBuilder BaseUrl(string baseUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseUrl);
        _baseUrl = baseUrl.TrimEnd('/');
        return this;
    }

    public PostProxyClientBuilder ProfileGroupId(string profileGroupId)
    {
        _profileGroupId = profileGroupId;
        return this;
    }

    public PostProxyClientBuilder HttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        return this;
    }

    public PostProxyClient Build()
    {
        var httpClient = _httpClient ?? new HttpClient();
        httpClient.BaseAddress ??= new Uri(_baseUrl);
        httpClient.DefaultRequestHeaders.Authorization ??=
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

        var client = new PostProxyHttpClient(httpClient);
        return new PostProxyClient(client, _profileGroupId);
    }
}
