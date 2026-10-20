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

    [Fact]
    public void Builder_ExposesNewResources()
    {
        var client = PostProxyClient.Builder("test-key").Build();

        Assert.Same(client.Placements, client.Placements);
        Assert.Same(client.ApiKeys, client.ApiKeys);
        Assert.Same(client.GoogleBusiness, client.GoogleBusiness);
        Assert.Same(client.WhatsAppTemplates, client.WhatsAppTemplates);
        Assert.Same(client.WhatsApp, client.WhatsApp);
    }

    [Fact]
    public async Task ForbiddenException_On403()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(System.Net.HttpStatusCode.Forbidden, """{"error": "Account is past due"}""");

        var ex = await Assert.ThrowsAsync<PostProxy.Exceptions.ForbiddenException>(() => client.Profiles.ListAsync());

        Assert.Equal(403, ex.StatusCode);
        Assert.Equal("Account is past due", ex.Message);
    }

    [Fact]
    public async Task RateLimitException_On429_WithRetryAfter()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(System.Net.HttpStatusCode.TooManyRequests, """{"error": "rate limited"}""",
            new Dictionary<string, string> { ["Retry-After"] = "30" });

        var ex = await Assert.ThrowsAsync<PostProxy.Exceptions.RateLimitException>(() => client.Profiles.ListAsync());

        Assert.Equal(429, ex.StatusCode);
        Assert.Equal(30, ex.RetryAfter);
    }

    [Fact]
    public async Task RateLimitException_WithoutRetryAfter_IsNull()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(System.Net.HttpStatusCode.TooManyRequests, """{"error": "rate limited"}""");

        var ex = await Assert.ThrowsAsync<PostProxy.Exceptions.RateLimitException>(() => client.Profiles.ListAsync());

        Assert.Null(ex.RetryAfter);
    }

    [Fact]
    public async Task SkipDuplicateCheck_SendsHeaderOnlyWhenSet()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"id": "post-1"}""");
        handler.EnqueueResponse("""{"id": "post-2"}""");

        await client.Posts.CreateAsync(new PostProxy.Parameters.CreatePostParams
        {
            Body = "hello",
            Profiles = ["prof-1"],
            SkipDuplicateCheck = true,
        });
        await client.Posts.CreateAsync(new PostProxy.Parameters.CreatePostParams
        {
            Body = "hello",
            Profiles = ["prof-1"],
        });

        Assert.Equal("true", handler.Requests[0].Headers["X-Skip-Duplicate-Check"]);
        Assert.False(handler.Requests[1].Headers.ContainsKey("X-Skip-Duplicate-Check"));
    }

    [Fact]
    public async Task WhatsAppMedia_ReturnsRawBytes()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueBinaryResponse([1, 2, 3], "image/jpeg");

        var result = await client.WhatsApp.DownloadMediaAsync("prof-1", "pn-1", "media-1");

        Assert.Equal("image/jpeg", result.ContentType);
        Assert.Equal(new byte[] { 1, 2, 3 }, result.Data);
        Assert.Contains("/api/profiles/prof-1/whatsapp/media?", handler.Requests[0].Url);
        Assert.Contains("phone_number_id=pn-1", handler.Requests[0].Url);
        Assert.Contains("media_id=media-1", handler.Requests[0].Url);
    }
}
