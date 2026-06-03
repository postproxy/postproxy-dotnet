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

    private static object MockMessage() => new
    {
        id = "msg_111",
        chat_id = "chat_xyz789",
        external_id = "mid.abc123",
        direction = "inbound",
        body = "Hi there",
        status = "received",
        reactions = Array.Empty<object>(),
        attachments = Array.Empty<object>(),
        is_unsupported = false,
        created_at = "2026-05-31T14:02:01.000Z",
    };

    [Theory]
    [InlineData("message.received", WebhookEventType.MessageReceived)]
    [InlineData("message.sent", WebhookEventType.MessageSent)]
    [InlineData("message.delivered", WebhookEventType.MessageDelivered)]
    [InlineData("message.read", WebhookEventType.MessageRead)]
    [InlineData("message.edited", WebhookEventType.MessageEdited)]
    [InlineData("message.deleted", WebhookEventType.MessageDeleted)]
    [InlineData("message.failed_waiting_for_retry", WebhookEventType.MessageFailedWaitingForRetry)]
    [InlineData("message.failed", WebhookEventType.MessageFailed)]
    public void ParsesMessageEvents(string type, WebhookEventType expected)
    {
        var ev = WebhookEvents.Parse(Envelope(type, new { message = MockMessage() }));

        Assert.Equal(expected, ev.Type);
        var data = WebhookEvents.AsMessageEvent(ev);
        Assert.Equal("msg_111", data.Message.Id);
        Assert.Equal(MessageDirection.Inbound, data.Message.Direction);
    }

    [Fact]
    public void ParsesReactionReceived()
    {
        var ev = WebhookEvents.Parse(Envelope("reaction.received", new
        {
            message = MockMessage(),
            sender_external_id = "psid_123",
            action = "react",
            reaction = "love",
            emoji = "❤️",
            occurred_at = "2026-05-31T14:04:00.000Z",
        }));

        Assert.Equal(WebhookEventType.ReactionReceived, ev.Type);
        var data = WebhookEvents.AsReactionEvent(ev);
        Assert.Equal("psid_123", data.SenderExternalId);
        Assert.Equal("react", data.Action);
        Assert.Equal("love", data.Reaction);
        Assert.Equal("msg_111", data.Message.Id);
    }

    [Fact]
    public void ParsesProfileCommentCreated()
    {
        var ev = WebhookEvents.Parse(Envelope("profile_comment.created", new
        {
            id = "pc_1",
            profile_id = "prof_abc123",
            platform = "google_business",
            placement_id = "loc_1",
            external_id = "ext_9",
            body = "Great service!",
            status = "published",
            author_username = "happy_customer",
            created_at = "2026-05-31T14:02:01.000Z",
        }));

        Assert.Equal(WebhookEventType.ProfileCommentCreated, ev.Type);
        var data = WebhookEvents.AsProfileCommentCreated(ev);
        Assert.Equal("pc_1", data.Id);
        Assert.Equal("prof_abc123", data.ProfileId);
        Assert.Equal(Platform.GoogleBusiness, data.Platform);
        Assert.Equal("Great service!", data.Body);
    }

    [Fact]
    public void RejectsInvalidJson()
    {
        Assert.Throws<WebhookParseException>(() => WebhookEvents.Parse("not json{"));
    }
}
