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
}
