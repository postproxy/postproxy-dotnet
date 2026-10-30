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

    [Theory]
    [InlineData("platform_post.edited", WebhookEventType.PlatformPostEdited)]
    [InlineData("platform_post.edit_failed", WebhookEventType.PlatformPostEditFailed)]
    [InlineData("profile.placements_synced", WebhookEventType.ProfilePlacementsSynced)]
    [InlineData("placement.orphaned", WebhookEventType.PlacementOrphaned)]
    [InlineData("placement.restored", WebhookEventType.PlacementRestored)]
    [InlineData("comment.failed", WebhookEventType.CommentFailed)]
    [InlineData("comment.failed_waiting_for_retry", WebhookEventType.CommentFailedWaitingForRetry)]
    [InlineData("comment.delivery_unconfirmed", WebhookEventType.CommentDeliveryUnconfirmed)]
    [InlineData("comment.edited", WebhookEventType.CommentEdited)]
    [InlineData("comment.edit_failed", WebhookEventType.CommentEditFailed)]
    [InlineData("profile_comment.failed", WebhookEventType.ProfileCommentFailed)]
    [InlineData("profile_comment.failed_waiting_for_retry", WebhookEventType.ProfileCommentFailedWaitingForRetry)]
    [InlineData("profile_comment.deleted", WebhookEventType.ProfileCommentDeleted)]
    [InlineData("referral.received", WebhookEventType.ReferralReceived)]
    [InlineData("message_template.status_updated", WebhookEventType.MessageTemplateStatusUpdated)]
    [InlineData("message_template.category_updated", WebhookEventType.MessageTemplateCategoryUpdated)]
    [InlineData("placement.name_status_updated", WebhookEventType.PlacementNameStatusUpdated)]
    [InlineData("placement.quality_updated", WebhookEventType.PlacementQualityUpdated)]
    [InlineData("whatsapp.automatic_event", WebhookEventType.WhatsAppAutomaticEvent)]
    public void ParsesEventTypesAddedIn114(string type, WebhookEventType expected)
    {
        var ev = WebhookEvents.Parse(Envelope(type, new { id = "x" }));

        Assert.Equal(expected, ev.Type);
    }

    [Theory]
    [InlineData("profile_comment.edited", WebhookEventType.ProfileCommentEdited)]
    [InlineData("chat.thread_control_changed", WebhookEventType.ChatThreadControlChanged)]
    [InlineData("chat.thread_control_requested", WebhookEventType.ChatThreadControlRequested)]
    public void ParsesEventTypesAddedIn115(string type, WebhookEventType expected)
    {
        var ev = WebhookEvents.Parse(Envelope(type, new { id = "x" }));

        Assert.Equal(expected, ev.Type);
    }

    [Fact]
    public void ParsesThreadControlChanged()
    {
        var ev = WebhookEvents.Parse(Envelope("chat.thread_control_changed", new
        {
            chat = new { id = "chat_1", profile_id = "prof_fb", platform = "facebook", group = false, participant_external_id = "psid_1" },
            previous_owner_app_id = (string?)null,
            new_owner_app_id = "263902037430900",
            metadata = "escalated",
            occurred_at = "2026-10-28T12:00:00Z",
        }));

        var data = WebhookEvents.AsThreadControlChanged(ev);
        Assert.Equal("chat_1", data.Chat.Id);
        Assert.Null(data.PreviousOwnerAppId);
        Assert.Equal("263902037430900", data.NewOwnerAppId);
        Assert.Equal("escalated", data.Metadata);
        Assert.NotNull(data.OccurredAt);
    }

    [Fact]
    public void ParsesThreadControlRequested()
    {
        var ev = WebhookEvents.Parse(Envelope("chat.thread_control_requested", new
        {
            chat = new { id = "chat_1", platform = "instagram", group = false },
            requested_owner_app_id = "1217981644879628",
            metadata = "please",
        }));

        var data = WebhookEvents.AsThreadControlRequested(ev);
        Assert.Equal(Platform.Instagram, data.Chat.Platform);
        Assert.Equal("1217981644879628", data.RequestedOwnerAppId);
        Assert.Equal("please", data.Metadata);
    }

    [Fact]
    public void ParsesProfileDisconnectedWithError()
    {
        var ev = WebhookEvents.Parse(Envelope("profile.disconnected", new
        {
            id = "prof_1",
            name = "Acme",
            platform = "twitter",
            profile_group_id = "pg_1",
            status = "inactive",
            uid = "u1",
            origin = "automated",
            error = "Twitter access revoked",
            error_details = new { platform_error_code = "89", platform_error_message = "Invalid or expired token" },
        }));

        var data = WebhookEvents.AsProfileEvent(ev);
        Assert.Equal("automated", data.Origin);
        Assert.Equal("Twitter access revoked", data.Error);
        Assert.Equal("89", data.ErrorDetails!.PlatformErrorCode);
        Assert.Equal("Invalid or expired token", data.ErrorDetails.PlatformErrorMessage);
    }

    [Fact]
    public void ParsesMessageTemplateStatusUpdated()
    {
        var ev = WebhookEvents.Parse(Envelope("message_template.status_updated", new
        {
            id = "tpl_1",
            external_id = "998877",
            profile_id = "prof_wa",
            platform = "whatsapp",
            name = "order_update",
            language = "en_US",
            category = "UTILITY",
            status = "APPROVED",
            rejected_reason = (string?)null,
            quality_score = (string?)null,
        }));

        var data = WebhookEvents.AsMessageTemplateEvent(ev);
        Assert.Equal("APPROVED", data.Status);
        Assert.Equal(Platform.WhatsApp, data.Platform);
    }

    [Fact]
    public void ParsesPlacementQualityUpdated_KeepsExtraFields()
    {
        var ev = WebhookEvents.Parse(Envelope("placement.quality_updated", new
        {
            id = "pn_1",
            name = "Acme",
            platform = "whatsapp",
            profile_group_id = "pg_1",
            profile_id = "prof_wa",
            profile_name = "Acme WABA",
            metadata = new { },
            @event = "UPGRADE",
            current_limit = "TIER_10K",
        }));

        var data = WebhookEvents.AsPlacementEvent(ev);
        Assert.Equal("pn_1", data.Id);
        Assert.Equal("UPGRADE", data.Extra!["event"].GetString());
        Assert.Equal("TIER_10K", data.Extra["current_limit"].GetString());
    }

    [Fact]
    public void ParsesPlacementsSynced()
    {
        var ev = WebhookEvents.Parse(Envelope("profile.placements_synced", new
        {
            id = "prof_1",
            name = "Acme",
            platform = "facebook",
            profile_group_id = "pg_1",
            status = "active",
            uid = "u1",
            platform_id = "fb_1",
            username = (string?)null,
            placements_sync_status = "synced",
            placements = new[] { new { id = "page_1", name = "Acme Page", platform = "facebook", profile_id = "prof_1" } },
        }));

        var data = WebhookEvents.AsPlacementsSynced(ev);
        Assert.Equal("synced", data.PlacementsSyncStatus);
        Assert.Equal("page_1", data.Placements[0].Id);
        Assert.Equal("fb_1", data.PlatformId);
    }

    [Fact]
    public void ParsesReferralReceived()
    {
        var ev = WebhookEvents.Parse(Envelope("referral.received", new
        {
            chat = new { id = "chat_1", profile_id = "prof_wa", platform = "whatsapp", group = false, participant_external_id = "1555" },
            source = "ADS",
            ctwa_clid = "clid_1",
        }));

        var data = WebhookEvents.AsReferralEvent(ev);
        Assert.Equal("chat_1", data.Chat.Id);
        Assert.Equal("clid_1", data.CtwaClid);
    }

    [Fact]
    public void ParsesWhatsAppAutomaticEvent()
    {
        var ev = WebhookEvents.Parse(Envelope("whatsapp.automatic_event", new
        {
            profile_id = "prof_wa",
            chat_id = "chat_1",
            event_name = "Purchase",
            custom_data = new { currency = "USD", value = 24.5 },
        }));

        var data = WebhookEvents.AsWhatsAppAutomaticEvent(ev);
        Assert.Equal("Purchase", data.EventName);
        Assert.Equal("USD", data.CustomData!["currency"].GetString());
    }

    [Fact]
    public void ParsesCommentEditFailed()
    {
        var ev = WebhookEvents.Parse(Envelope("comment.edit_failed", new
        {
            id = "c_1",
            post_id = "p_1",
            platform = "facebook",
            profile_id = "prof_1",
            error = "Comment no longer exists",
            created_at = "2026-10-01T00:00:00Z",
        }));

        var data = WebhookEvents.AsCommentCreated(ev);
        Assert.Equal("prof_1", data.ProfileId);
        Assert.Equal("Comment no longer exists", data.Error);
    }
}
