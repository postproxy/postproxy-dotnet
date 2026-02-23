namespace PostProxy.Tests;

public class ClientTests
{
    [Fact]
    public void Builder_ThrowsOnNullApiKey()
    {
        Assert.Throws<ArgumentNullException>(() => PostProxyClient.Builder(null!));
    }

    [Fact]
    public void Builder_ThrowsOnEmptyApiKey()
    {
        Assert.Throws<ArgumentException>(() => PostProxyClient.Builder(""));
    }

    [Fact]
    public void Builder_CreatesClient()
    {
        var client = PostProxyClient.Builder("test-key").Build();

        Assert.NotNull(client);
        Assert.NotNull(client.Posts);
        Assert.NotNull(client.Profiles);
        Assert.NotNull(client.ProfileGroups);
    }

    [Fact]
    public void Builder_ResourcesAreCached()
    {
        var client = PostProxyClient.Builder("test-key").Build();

        Assert.Same(client.Posts, client.Posts);
        Assert.Same(client.Profiles, client.Profiles);
        Assert.Same(client.ProfileGroups, client.ProfileGroups);
    }
}
