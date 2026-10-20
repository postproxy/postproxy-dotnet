using PostProxy.Models;
using PostProxy.Parameters;

namespace PostProxy.Tests;

public class ChatsResourceTests
{
    private const string MockChat = """
    {
        "id": "chat_xyz789",
        "profile_id": "prof_abc123",
        "platform": "instagram",
        "participant_external_id": "igsid_8675309",
        "participant_username": "jane_doe",
        "participant_name": "Jane Doe",
        "participant_avatar_url": "https://storage.postproxy.dev/x.jpg",
        "external_conversation_id": null,
        "last_inbound_at": "2026-05-31T14:02:00.000Z",
        "last_outbound_at": "2026-05-31T15:10:00.000Z",
        "last_message_at": "2026-05-31T15:10:00.000Z",
        "metadata": {"is_verified_user": false, "follower_count": 482},
        "created_at": "2026-04-12T08:00:00.000Z"
    }
    """;

    [Fact]
    public async Task ListAsync_ReturnsChats()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse($$"""
        {
            "total": 1,
            "page": 0,
            "per_page": 20,
            "data": [{{MockChat}}]
        }
        """);

        var result = await client.Chats.ListAsync("prof_abc123", new ListChatsParams { PerPage = 20 });

        Assert.Equal(1, result.Total);
        Assert.Single(result.Data);
        var chat = result.Data[0];
        Assert.Equal("chat_xyz789", chat.Id);
        Assert.Equal("jane_doe", chat.ParticipantUsername);
        Assert.Equal(Platform.Instagram, chat.Platform);
        Assert.Equal(482, chat.Metadata!["follower_count"].GetInt32());

        var req = handler.Requests[0];
        Assert.Equal(HttpMethod.Get, req.Method);
        Assert.Contains("/api/profiles/prof_abc123/chats", req.Url);
        Assert.Contains("per_page=20", req.Url);
    }

    [Fact]
    public async Task CreateAsync_SendsCorrectBody()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(MockChat);

        var chat = await client.Chats.CreateAsync("prof_abc123", new CreateChatParams
        {
            ParticipantExternalId = "igsid_8675309",
            ParticipantUsername = "jane_doe",
        });

        Assert.Equal("chat_xyz789", chat.Id);

        var req = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.Contains("/api/profiles/prof_abc123/chats", req.Url);
        Assert.Contains("\"participant_external_id\":\"igsid_8675309\"", req.Body);
        Assert.Contains("\"participant_username\":\"jane_doe\"", req.Body);
    }

    [Fact]
    public async Task GetAsync_ReturnsChat()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(MockChat);

        var chat = await client.Chats.GetAsync("chat_xyz789");

        Assert.Equal("chat_xyz789", chat.Id);
        Assert.Contains("/api/chats/chat_xyz789", handler.Requests[0].Url);
    }

    [Fact]
    public async Task ArchiveAsync_ReturnsArchivedChat()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "id": "chat_xyz789",
            "profile_id": "prof_abc123",
            "platform": "bluesky",
            "participant_external_id": "did:plc:abc",
            "archived": true,
            "created_at": "2026-04-12T08:00:00.000Z"
        }
        """);

        var chat = await client.Chats.ArchiveAsync("chat_xyz789");

        Assert.True(chat.Archived);
        var req = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.Contains("/api/chats/chat_xyz789/archive", req.Url);
    }

    [Fact]
    public async Task UnarchiveAsync_ReturnsUnarchivedChat()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "id": "chat_xyz789",
            "profile_id": "prof_abc123",
            "platform": "bluesky",
            "participant_external_id": "did:plc:abc",
            "archived": false,
            "created_at": "2026-04-12T08:00:00.000Z"
        }
        """);

        var chat = await client.Chats.UnarchiveAsync("chat_xyz789");

        Assert.False(chat.Archived);
        var req = handler.Requests[0];
        Assert.Equal(HttpMethod.Delete, req.Method);
        Assert.Contains("/api/chats/chat_xyz789/archive", req.Url);
    }

    [Fact]
    public async Task CreateAsync_WithPlacementId_ForWhatsApp()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(MockChat);

        await client.Chats.CreateAsync("prof_wa", new CreateChatParams
        {
            ParticipantExternalId = "15551234567",
            PlacementId = "pn_1",
        });

        Assert.Equal("""{"participant_external_id":"15551234567","placement_id":"pn_1"}""", handler.Requests[0].Body);
    }

    [Fact]
    public async Task GetAsync_ParsesWhatsAppFields()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "id": "chat_wa",
            "profile_id": "prof_wa",
            "platform": "whatsapp",
            "external_placement_id": "pn_1",
            "group": false,
            "participant_external_id": "15551234567",
            "within_messaging_window": true,
            "created_at": "2026-10-01T00:00:00Z"
        }
        """);

        var chat = await client.Chats.GetAsync("chat_wa");

        Assert.Equal(Platform.WhatsApp, chat.Platform);
        Assert.Equal("pn_1", chat.ExternalPlacementId);
        Assert.False(chat.Group);
        Assert.True(chat.WithinMessagingWindow);
    }

    [Fact]
    public async Task MarkReadAsync_PostsToCorrectUrl()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(MockChat);

        await client.Chats.MarkReadAsync("chat_xyz789");

        Assert.Equal(HttpMethod.Post, handler.Requests[0].Method);
        Assert.Contains("/api/chats/chat_xyz789/mark_read", handler.Requests[0].Url);
    }

    [Fact]
    public async Task StartAndStopTyping()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"accepted": true}""");
        handler.EnqueueResponse("""{"accepted": true}""");

        var started = await client.Chats.StartTypingAsync("chat_xyz789");
        await client.Chats.StopTypingAsync("chat_xyz789");

        Assert.True(started.Accepted);
        Assert.Contains("/api/chats/chat_xyz789/start_typing", handler.Requests[0].Url);
        Assert.Contains("/api/chats/chat_xyz789/stop_typing", handler.Requests[1].Url);
    }
}
