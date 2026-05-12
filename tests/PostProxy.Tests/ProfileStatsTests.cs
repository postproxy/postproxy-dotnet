using System.Net;
using System.Text.Json;

namespace PostProxy.Tests;

public class ProfileStatsTests
{
    [Fact]
    public async Task GetProfileStats_With_Placement()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(HttpStatusCode.OK,
            """
            {"data":{"profile_id":"pf1","platform":"linkedin","placement_id":"org_1","records":[{"stats":{"followerCount":100},"recorded_at":"2026-05-12T00:00:00Z"}]}}
            """);

        var result = await client.Profiles.GetProfileStatsAsync(
            "pf1",
            placementId: "org_1",
            from: "2026-04-01T00:00:00Z");

        Assert.Equal("pf1", result.Data.ProfileId);
        Assert.Equal(100, result.Data.Records[0].Stats["followerCount"].GetInt32());

        var url = handler.Requests[0].Url;
        Assert.Contains("/api/profiles/pf1/stats", url);
        Assert.Contains("placement_id=org_1", url);
    }

    [Fact]
    public async Task GetProfileStats_Without_Placement()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(HttpStatusCode.OK,
            """
            {"data":{"profile_id":"bsky1","platform":"bluesky","placement_id":null,"records":[]}}
            """);

        var result = await client.Profiles.GetProfileStatsAsync("bsky1");
        Assert.Null(result.Data.PlacementId);
        Assert.DoesNotContain("placement_id", handler.Requests[0].Url);
    }
}
