using System.Net;
using System.Text.Json;

namespace PostProxy.Tests;

public class ConnectTests
{
    [Fact]
    public async Task ConnectBluesky_Posts_Identifier_And_AppPassword()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(HttpStatusCode.OK,
            """
            {"success":true,"profile":{"id":"pf_bsky_1","network":"bluesky","name":"Jane","external_username":"jane.bsky.social"}}
            """);

        var result = await client.ProfileGroups.ConnectBlueskyAsync(
            "pg-1", "jane.bsky.social", "xxxx");

        Assert.True(result.Success);
        Assert.Equal("pf_bsky_1", result.Profile.Id);

        var body = JsonDocument.Parse(handler.Requests[0].Body!);
        Assert.Equal("bluesky", body.RootElement.GetProperty("platform").GetString());
        Assert.Equal("jane.bsky.social", body.RootElement.GetProperty("identifier").GetString());
        Assert.Equal("xxxx", body.RootElement.GetProperty("app_password").GetString());
    }

    [Fact]
    public async Task ConnectTelegram_Posts_BotToken()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(HttpStatusCode.OK,
            """
            {"success":true,"profile":{"id":"pf_tg_1","network":"telegram","name":"My Bot","external_username":"my_bot"},"next_step":"Add bot as admin"}
            """);

        var result = await client.ProfileGroups.ConnectTelegramAsync("pg-1", "123:ABC");

        Assert.Contains("admin", result.NextStep);

        var body = JsonDocument.Parse(handler.Requests[0].Body!);
        Assert.Equal("telegram", body.RootElement.GetProperty("platform").GetString());
        Assert.Equal("123:ABC", body.RootElement.GetProperty("bot_token").GetString());
    }
}
