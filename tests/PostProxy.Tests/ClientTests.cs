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

    [Fact]
    public async Task ConflictException_On409()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(System.Net.HttpStatusCode.Conflict, """
        {"error": "Duplicate post", "duplicate_post_id": "post-1"}
        """);

        var ex = await Assert.ThrowsAsync<PostProxy.Exceptions.ConflictException>(
            () => client.Posts.CreateAsync(new PostProxy.Parameters.CreatePostParams
            {
                Body = "hello",
                Profiles = ["prof-1"],
            }));

        Assert.Equal(409, ex.StatusCode);
        Assert.Equal("post-1", ex.Response!["duplicate_post_id"].ToString());
    }

    [Fact]
    public async Task IdempotencyKey_IsSentWhenProvided()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"id": "post-1", "body": "hello", "created_at": "2026-08-06T00:00:00Z"}""");

        await client.Posts.CreateAsync(new PostProxy.Parameters.CreatePostParams
        {
            Body = "hello",
            Profiles = ["prof-1"],
            IdempotencyKey = "3f8b1c94-6a2d-4f0e-9d31-7c5e2a8b4f10",
        });

        Assert.Equal("3f8b1c94-6a2d-4f0e-9d31-7c5e2a8b4f10", handler.Requests[0].Headers["Idempotency-Key"]);
    }

    [Fact]
    public async Task IdempotencyKey_IsOmittedByDefault()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"id": "post-1", "body": "hello", "created_at": "2026-08-06T00:00:00Z"}""");

        await client.Posts.CreateAsync(new PostProxy.Parameters.CreatePostParams
        {
            Body = "hello",
            Profiles = ["prof-1"],
        });

        Assert.False(handler.Requests[0].Headers.ContainsKey("Idempotency-Key"));
    }
}
