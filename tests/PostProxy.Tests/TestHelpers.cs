namespace PostProxy.Tests;

internal static class TestHelpers
{
    public static (PostProxyClient Client, MockHttpHandler Handler) CreateMockClient(string? profileGroupId = null)
    {
        var handler = new MockHttpHandler();
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.postproxy.dev"),
        };

        var builder = PostProxyClient.Builder("test-api-key")
            .HttpClient(httpClient);

        if (profileGroupId is not null)
            builder.ProfileGroupId(profileGroupId);

        return (builder.Build(), handler);
    }
}
