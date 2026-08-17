using PostProxy.Models;
using PostProxy.Parameters;

namespace PostProxy.Tests;

public class MessagesResourceTests
{
    private const string MockInbound = """
    {
        "id": "msg_111",
        "chat_id": "chat_xyz789",
        "external_id": "mid.abc123",
        "direction": "inbound",
        "body": "Hey, do you ship internationally?",
        "status": "received",
        "tag": null,
        "error_message": null,
        "platform_data": null,
        "external_posted_at": "2026-05-31T14:02:00.000Z",
        "external_delivered_at": null,
        "external_read_at": null,
        "external_edited_at": null,
        "reactions": [
            {"sender_external_id": "psid_123", "emoji": "❤️", "reaction": "love", "at": "2026-05-31T14:04:00.000Z"}
        ],
        "attachments": [],
        "is_unsupported": false,
        "created_at": "2026-05-31T14:02:01.000Z"
    }
    """;

    private const string MockOutbound = """
    {
        "id": "msg_222",
        "chat_id": "chat_xyz789",
        "external_id": null,
        "direction": "outbound",
        "body": "Yes, we ship worldwide!",
        "status": "pending",
        "attachments": [],
        "reactions": [],
        "is_unsupported": false,
        "created_at": "2026-05-31T15:30:05.000Z"
    }
    """;

    [Fact]
    public async Task ListAsync_ReturnsMessages()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse($$"""
        {
            "total": 1,
            "page": 0,
            "per_page": 20,
            "data": [{{MockInbound}}]
        }
        """);

        var result = await client.Messages.ListAsync("chat_xyz789", new ListMessagesParams { Direction = MessageDirection.Inbound });

        Assert.Equal(1, result.Total);
        var msg = result.Data[0];
        Assert.Equal(MessageDirection.Inbound, msg.Direction);
        Assert.Equal(MessageStatus.Received, msg.Status);
        Assert.Single(msg.Reactions);
        Assert.Equal("love", msg.Reactions[0].ReactionType);

        var req = handler.Requests[0];
        Assert.Contains("/api/chats/chat_xyz789/messages", req.Url);
        Assert.Contains("direction=inbound", req.Url);
    }

    [Fact]
    public async Task SendAsync_TextMessage()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(MockOutbound);

        var msg = await client.Messages.SendAsync("chat_xyz789", "Yes, we ship worldwide!");

        Assert.Equal("msg_222", msg.Id);
        Assert.Equal(MessageStatus.Pending, msg.Status);

        var req = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.Contains("/api/chats/chat_xyz789/messages", req.Url);
        Assert.Contains("\"body\":\"Yes, we ship worldwide!\"", req.Body);
    }

    [Fact]
    public async Task SendAsync_WithTag()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(MockOutbound);

        await client.Messages.SendAsync("chat_xyz789", new SendMessageParams { Body = "Following up.", Tag = "HUMAN_AGENT" });

        Assert.Contains("\"tag\":\"HUMAN_AGENT\"", handler.Requests[0].Body);
    }

    [Fact]
    public async Task SendAsync_WithMediaUrls()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(MockOutbound);

        await client.Messages.SendAsync("chat_xyz789", new SendMessageParams
        {
            Media = new[] { "https://cdn.example.com/photo.png" },
        });

        Assert.Contains("\"media\":[\"https://cdn.example.com/photo.png\"]", handler.Requests[0].Body);
    }

    [Fact]
    public async Task SendAsync_WithQuickReplies()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
          "id": "msg_222", "chat_id": "chat_xyz789", "direction": "outbound",
          "body": "What can I help with?", "status": "pending",
          "quick_replies": [{ "content_type": "text", "title": "Track order", "payload": "TRACK" }],
          "reactions": [], "attachments": [], "is_unsupported": false,
          "created_at": "2026-05-31T15:30:05Z"
        }
        """);

        var msg = await client.Messages.SendAsync("chat_xyz789", new SendMessageParams
        {
            Body = "What can I help with?",
            QuickReplies = new[]
            {
                new QuickReply { Title = "Track order", Payload = "TRACK" },
                new QuickReply { Title = "Talk to support", Payload = "HELP" },
            },
        });

        var body = handler.Requests[0].Body;
        Assert.Contains("\"quick_replies\":[{\"title\":\"Track order\",\"payload\":\"TRACK\"}", body);
        // Unset ContentType is dropped by WhenWritingNull rather than sent as null.
        Assert.DoesNotContain("\"content_type\":null", body);

        Assert.Equal("text", msg.QuickReplies![0].ContentType);
    }

    [Fact]
    public async Task SendAsync_WithButtonsAndCard()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
          "id": "msg_222", "chat_id": "chat_xyz789", "direction": "outbound",
          "body": "Your order shipped", "status": "pending",
          "buttons": [{ "type": "web_url", "title": "Track", "url": "https://shop.example.com" }],
          "card": { "subtitle": "Arriving Friday" },
          "reactions": [], "attachments": [], "is_unsupported": false,
          "created_at": "2026-05-31T15:30:05Z"
        }
        """);

        var msg = await client.Messages.SendAsync("chat_xyz789", new SendMessageParams
        {
            Body = "Your order shipped",
            Buttons = new[]
            {
                MessageButton.WebUrl("Track", "https://shop.example.com"),
                MessageButton.Postback("Cancel", "CANCEL:123"),
            },
            Card = new MessageCard
            {
                Subtitle = "Arriving Friday",
                DefaultAction = CardDefaultAction.WebUrl("https://shop.example.com"),
            },
        });

        var body = handler.Requests[0].Body;
        Assert.Contains("\"type\":\"web_url\",\"title\":\"Track\",\"url\":\"https://shop.example.com\"", body);
        Assert.Contains("\"type\":\"postback\",\"title\":\"Cancel\",\"payload\":\"CANCEL:123\"", body);
        Assert.Contains("\"card\":{\"subtitle\":\"Arriving Friday\"", body);
        Assert.Contains("\"default_action\":{\"type\":\"web_url\",\"url\":\"https://shop.example.com\"}", body);

        Assert.Equal("https://shop.example.com", msg.Buttons![0].Url);
        Assert.Equal("Arriving Friday", msg.Card!.Subtitle);
    }

    [Fact]
    public async Task GetAsync_ReadsTappedAction()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
          "id": "msg_333", "chat_id": "chat_xyz789", "direction": "inbound",
          "body": "Track order", "status": "received",
          "tapped_action": { "kind": "quick_reply", "payload": "TRACK", "title": "Track order" },
          "reactions": [], "attachments": [], "is_unsupported": false,
          "created_at": "2026-05-31T14:02:01Z"
        }
        """);

        var msg = await client.Messages.GetAsync("msg_333");

        Assert.NotNull(msg.TappedAction);
        Assert.Equal(TappedActionKind.QuickReply, msg.TappedAction!.Kind);
        Assert.Equal("TRACK", msg.TappedAction.Payload);
        Assert.Equal("Track order", msg.TappedAction.Title);
    }

    [Fact]
    public async Task SendAsync_WithMediaFiles_UsesMultipart()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(MockOutbound);

        var tempFile = Path.Combine(Path.GetTempPath(), $"pp_test_{Guid.NewGuid():N}.png");
        await File.WriteAllBytesAsync(tempFile, new byte[] { 0x89, 0x50, 0x4E, 0x47 });
        try
        {
            await client.Messages.SendAsync("chat_xyz789", new SendMessageParams
            {
                Body = "Photo!",
                MediaFiles = new[] { tempFile },
            });

            var req = handler.Requests[0];
            Assert.Equal(HttpMethod.Post, req.Method);
            Assert.Contains("Photo!", req.Body);
            Assert.Contains("media[]", req.Body);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task GetAsync_ReturnsMessage()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(MockInbound);

        var msg = await client.Messages.GetAsync("msg_111");

        Assert.Equal("msg_111", msg.Id);
        Assert.Contains("/api/messages/msg_111", handler.Requests[0].Url);
    }

    [Fact]
    public async Task EditAsync_SendsBody()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "id": "msg_222",
            "chat_id": "chat_xyz789",
            "direction": "outbound",
            "body": "Updated",
            "status": "published",
            "created_at": "2026-05-31T15:30:05.000Z"
        }
        """);

        var msg = await client.Messages.EditAsync("msg_222", new EditMessageParams { Body = "Updated" });

        Assert.Equal("Updated", msg.Body);
        var req = handler.Requests[0];
        Assert.Equal(HttpMethod.Patch, req.Method);
        Assert.Contains("/api/messages/msg_222", req.Url);
        Assert.Contains("\"body\":\"Updated\"", req.Body);
    }

    [Fact]
    public async Task ReactAsync_SendsReaction()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(MockInbound);

        var msg = await client.Messages.ReactAsync("msg_111", new ReactParams { Reaction = "love", Emoji = "❤️" });

        Assert.Equal("msg_111", msg.Id);
        var req = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.Contains("/api/messages/msg_111/react", req.Url);
        Assert.Contains("\"reaction\":\"love\"", req.Body);
    }

    [Fact]
    public async Task ReactAsync_DefaultsToEmptyBody()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(MockInbound);

        await client.Messages.ReactAsync("msg_111");

        var req = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.Contains("/api/messages/msg_111/react", req.Url);
    }

    [Fact]
    public async Task UnreactAsync_DeletesReaction()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(MockInbound);

        var msg = await client.Messages.UnreactAsync("msg_111");

        Assert.Equal("msg_111", msg.Id);
        var req = handler.Requests[0];
        Assert.Equal(HttpMethod.Delete, req.Method);
        Assert.Contains("/api/messages/msg_111/unreact", req.Url);
    }
}
