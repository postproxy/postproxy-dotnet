using System.Net;
using PostProxy.Exceptions;
using PostProxy.Models;

namespace PostProxy.Tests;

public class ProfilesResourceTests
{
    [Fact]
    public async Task ListAsync_ReturnsProfiles()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "data": [
                {"id": "prof-1", "name": "My Page", "status": "active", "platform": "facebook", "post_count": 42}
            ]
        }
        """);

        var result = await client.Profiles.ListAsync();

        Assert.Single(result.Data);
        Assert.Equal("prof-1", result.Data[0].Id);
        Assert.Equal(ProfileStatus.Active, result.Data[0].Status);
        Assert.Equal(Platform.Facebook, result.Data[0].Platform);
        Assert.Equal(42, result.Data[0].PostCount);
    }

    [Fact]
    public async Task ListAsync_WithProfileGroupId_AddsQueryParam()
    {
        var (client, handler) = TestHelpers.CreateMockClient("pg-default");
        handler.EnqueueResponse("""{"data": []}""");

        await client.Profiles.ListAsync();

        Assert.Contains("profile_group_id=pg-default", handler.Requests[0].Url);
    }

    [Fact]
    public async Task GetAsync_ReturnsProfile()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"id": "prof-1", "name": "My Page", "platform": "instagram"}""");

        var profile = await client.Profiles.GetAsync("prof-1");

        Assert.Equal("prof-1", profile.Id);
        Assert.Equal(Platform.Instagram, profile.Platform);
    }

    [Fact]
    public async Task PlacementsAsync_ReturnsPlacements()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"data": [{"id": "pl-1", "name": "Feed"}]}""");

        var result = await client.Profiles.PlacementsAsync("prof-1");

        Assert.Single(result.Data);
        Assert.Equal("pl-1", result.Data[0].Id);
        Assert.Contains("/api/profiles/prof-1/placements", handler.Requests[0].Url);
    }

    [Fact]
    public async Task AssignPlacementToGroupAsync_SendsPatchWithBody()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"id": "pl-1", "name": "Feed", "metadata": {}, "profile_group_id": "pg-2"}""");

        var result = await client.Profiles.AssignPlacementToGroupAsync("prof-1", "pl-1", "pg-2");

        Assert.Equal("pg-2", result.ProfileGroupId);
        Assert.Equal(HttpMethod.Patch, handler.Requests[0].Method);
        Assert.Contains("/api/profiles/prof-1/assign_placement_to_group", handler.Requests[0].Url);
        Assert.Contains("\"placement_id\":\"pl-1\"", handler.Requests[0].Body);
        Assert.Contains("\"target_profile_group_id\":\"pg-2\"", handler.Requests[0].Body);
    }

    [Fact]
    public async Task GetIceBreakersAsync_ReturnsIceBreakers()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"ice_breakers": [{"question": "What do you do?", "payload": "services"}]}""");

        var result = await client.Profiles.GetIceBreakersAsync("prof-1");

        Assert.Single(result.IceBreakers);
        Assert.Equal("What do you do?", result.IceBreakers[0].Question);
        Assert.Contains("/api/profiles/prof-1/ice_breakers", handler.Requests[0].Url);
    }

    [Fact]
    public async Task SetIceBreakersAsync_SendsPostWithBody()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"success": true}""");

        var result = await client.Profiles.SetIceBreakersAsync("prof-1",
            [new IceBreaker { Question = "What do you do?", Payload = "services" }]);

        Assert.True(result.Success);
        Assert.Equal(HttpMethod.Post, handler.Requests[0].Method);
        Assert.Contains("/api/profiles/prof-1/ice_breakers", handler.Requests[0].Url);
        Assert.Contains("\"question\":\"What do you do?\"", handler.Requests[0].Body);
        Assert.Contains("\"payload\":\"services\"", handler.Requests[0].Body);
    }

    [Fact]
    public async Task DeleteIceBreakersAsync_SendsDelete()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"success": true}""");

        var result = await client.Profiles.DeleteIceBreakersAsync("prof-1");

        Assert.True(result.Success);
        Assert.Equal(HttpMethod.Delete, handler.Requests[0].Method);
        Assert.Contains("/api/profiles/prof-1/ice_breakers", handler.Requests[0].Url);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsSuccessResponse()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"success": true}""");

        var result = await client.Profiles.DeleteAsync("prof-1");

        Assert.True(result.Success);
        Assert.Equal(HttpMethod.Delete, handler.Requests[0].Method);
    }

    [Fact]
    public async Task GetAsync_ThrowsAuthenticationException()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(HttpStatusCode.Unauthorized, """{"error": "Invalid API key"}""");

        var ex = await Assert.ThrowsAsync<AuthenticationException>(() =>
            client.Profiles.GetAsync("prof-1"));

        Assert.Equal(401, ex.StatusCode);
    }
}
