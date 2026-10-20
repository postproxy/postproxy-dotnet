namespace PostProxy.Tests;

public class PlacementsResourceTests
{
    private const string MockPlacement = """
    {
        "id": "pl_1",
        "platform": "facebook",
        "external_id": "1234567890",
        "name": "Acme Page",
        "avatar_url": null,
        "status": "active",
        "active_profile": {"id": "prof_1", "name": "Jane", "connected": true},
        "linked_profiles": [{"id": "prof_2", "name": "John", "connected": true}]
    }
    """;

    [Fact]
    public async Task ListAsync_ReturnsPlacements()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse($$"""{"total": 1, "page": 2, "per_page": 10, "data": [{{MockPlacement}}]}""");

        var result = await client.Placements.ListAsync(page: 2, perPage: 10);

        Assert.Equal("prof_1", result.Data[0].ActiveProfile!.Id);
        Assert.Equal("prof_2", result.Data[0].LinkedProfiles[0].Id);
        Assert.Equal(HttpMethod.Get, handler.Requests[0].Method);
        Assert.Equal("/api/placements?page=2&per_page=10", handler.Requests[0].Url);
    }

    [Fact]
    public async Task SetActiveProfileAsync_PatchesProfileId()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(MockPlacement);

        await client.Placements.SetActiveProfileAsync("pl_1", "prof_2");

        var req = handler.Requests[0];
        Assert.Equal(HttpMethod.Patch, req.Method);
        Assert.Equal("/api/placements/pl_1/set_active_profile", req.Url);
        Assert.Equal("""{"profile_id":"prof_2"}""", req.Body);
    }
}

public class ApiKeysResourceTests
{
    [Fact]
    public async Task ListAsync_ReturnsKeys()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"data": [{"id": "k1", "name": "CI", "profile_group_id": "pg_1", "sandbox": false, "created_at": "2026-09-09T00:00:00Z"}]}""");

        var result = await client.ApiKeys.ListAsync();

        Assert.Equal("CI", result.Data[0].Name);
        Assert.Null(result.Data[0].Token);
        Assert.Equal("/api/api_keys", handler.Requests[0].Url);
    }

    [Fact]
    public async Task CreateAsync_ReturnsToken()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"id": "k1", "name": "CI", "profile_group_id": "pg_1", "sandbox": false, "token": "secret", "created_at": "2026-09-09T00:00:00Z"}""");

        var key = await client.ApiKeys.CreateAsync("CI", "pg_1", idempotencyKey: "idem");

        Assert.Equal("secret", key.Token);
        var req = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.Equal("""{"name":"CI","profile_group_id":"pg_1"}""", req.Body);
        Assert.Equal("idem", req.Headers["Idempotency-Key"]);
    }

    [Fact]
    public async Task DeleteAsync_DeletesKey()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"deleted": true}""");

        var result = await client.ApiKeys.DeleteAsync("k1");

        Assert.True(result.Deleted);
        Assert.Equal(HttpMethod.Delete, handler.Requests[0].Method);
        Assert.Equal("/api/api_keys/k1", handler.Requests[0].Url);
    }
}
