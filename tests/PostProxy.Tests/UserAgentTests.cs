namespace PostProxy.Tests;

public class UserAgentTests
{
    [Fact]
    public void HttpClient_DefaultUserAgent_StartsWithSdkPrefix()
    {
        var httpClient = new HttpClient();
        PostProxyClient.Builder("test-key").HttpClient(httpClient).Build();

        var ua = httpClient.DefaultRequestHeaders.UserAgent.ToString();
        Assert.StartsWith($"postproxy-dotnet/{SdkVersion.Version}", ua);
    }

    [Fact]
    public void Version_IsBumped()
    {
        Assert.Equal("1.8.0", SdkVersion.Version);
    }
}
