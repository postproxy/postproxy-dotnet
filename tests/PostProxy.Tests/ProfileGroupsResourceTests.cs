using PostProxy.Models;

namespace PostProxy.Tests;

public class ProfileGroupsResourceTests
{
    [Fact]
    public async Task ListAsync_ReturnsGroups()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"data": [{"id": "pg-1", "name": "Default", "profiles_count": 3}]}""");

        var result = await client.ProfileGroups.ListAsync();

        Assert.Single(result.Data);
        Assert.Equal("pg-1", result.Data[0].Id);
        Assert.Equal("Default", result.Data[0].Name);
        Assert.Equal(3, result.Data[0].ProfilesCount);
    }

    [Fact]
    public async Task GetAsync_ReturnsGroup()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"id": "pg-1", "name": "Default", "profiles_count": 3}""");

        var group = await client.ProfileGroups.GetAsync("pg-1");

        Assert.Equal("pg-1", group.Id);
        Assert.Contains("/api/profile_groups/pg-1", handler.Requests[0].Url);
    }

    [Fact]
    public async Task CreateAsync_SendsCorrectBody()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"id": "pg-new", "name": "New Group", "profiles_count": 0}""");

        var group = await client.ProfileGroups.CreateAsync("New Group");

        Assert.Equal("pg-new", group.Id);
        var body = handler.Requests[0].Body!;
        Assert.Contains("\"name\":\"New Group\"", body);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsDeleteResponse()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"deleted": true}""");

        var result = await client.ProfileGroups.DeleteAsync("pg-1");

        Assert.True(result.Deleted);
        Assert.Equal(HttpMethod.Delete, handler.Requests[0].Method);
    }

    [Fact]
    public async Task InitializeConnectionAsync_SendsCorrectBody()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"url": "https://facebook.com/oauth", "success": true}""");

        var result = await client.ProfileGroups.InitializeConnectionAsync(
            "pg-1", Platform.Facebook, "https://myapp.com/callback");

        Assert.True(result.Success);
        Assert.Equal("https://facebook.com/oauth", result.Url);
        var body = handler.Requests[0].Body!;
        Assert.Contains("\"facebook\"", body);
        Assert.Contains("https://myapp.com/callback", body);
    }
}
