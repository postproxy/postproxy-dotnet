using System.Text.Json;
using PostProxy.Models;
using PostProxy.Webhooks;

namespace PostProxy.Tests;

public class WebhookEventsTests
{
    private static string Envelope(string type, object data)
    {
        return JsonSerializer.Serialize(new
        {
            id = "evt_1",
            type,
            created_at = "2026-05-12T00:00:00Z",
            data,
        });
    }

    [Fact]
    public void ParsesPostProcessed()
    {
        var ev = WebhookEvents.Parse(Envelope("post.processed", new
        {
            id = "p1",
            body = "hi",
            status = "processed",
            scheduled_at = (string?)null,
            created_at = "2026-05-12T00:00:00Z",
            platforms = new[] { new { id = "pf1", platform = "twitter", name = "X" } },
        }));

        Assert.Equal(WebhookEventType.PostProcessed, ev.Type);
        var data = WebhookEvents.AsPostProcessed(ev);
        Assert.Equal("hi", data.Body);
        Assert.Single(data.Platforms);
    }

    [Theory]
    [InlineData("platform_post.published")]
    [InlineData("platform_post.failed")]
    [InlineData("platform_post.failed_waiting_for_retry")]
    [InlineData("platform_post.insights")]
    public void ParsesPlatformPostVariants(string type)
    {
        var ev = WebhookEvents.Parse(Envelope(type, new
        {
            id = "pp1",
            post_id = "p1",
            platform = "twitter",
            profile_id = "pf1",
            profile_name = "X",
            status = "published",
            error = (string?)null,
            error_details = (object?)null,
            platform_id = "tw_999",
        }));

        var data = WebhookEvents.AsPlatformPost(ev);
        Assert.Equal("pp1", data.Id);
    }

    [Fact]
    public void ParsesProfileStats()
    {
        var ev = WebhookEvents.Parse(Envelope("profile.stats", new
        {
            profile_id = "pf1",
            platform = "linkedin",
            placement_id = "org_1",
            stats = new { followerCount = 4567 },
            recorded_at = "2026-05-12T00:00:00Z",
        }));

        var data = WebhookEvents.AsProfileStats(ev);
        Assert.Equal("org_1", data.PlacementId);
        Assert.Equal(4567, data.Stats["followerCount"].GetInt32());
    }

    [Fact]
    public void ParsesCommentCreated()
    {
        var ev = WebhookEvents.Parse(Envelope("comment.created", new
        {
            id = "c1",
            post_id = "p1",
            platform_post_id = "pp1",
            platform = "instagram",
            body = "great",
            status = "published",
            author_name = "Jane",
            author_username = "jane",
            like_count = 0,
            reply_count = 0,
            is_hidden = false,
            created_at = "2026-05-12T00:00:00Z",
        }));

        var data = WebhookEvents.AsCommentCreated(ev);
        Assert.Equal("Jane", data.AuthorName);
    }

    [Fact]
    public void RejectsInvalidJson()
    {
        Assert.Throws<WebhookParseException>(() => WebhookEvents.Parse("not json{"));
    }
}
