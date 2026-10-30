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

    private const string PostSyncJson = """
    {
        "id": "sync456def",
        "profile_id": "prof-1",
        "kind": "posts",
        "trigger": "backfill",
        "status": "running",
        "started_at": "2026-08-06T09:15:02.000Z",
        "completed_at": null,
        "posts_seen": 150,
        "posts_imported": 143,
        "backfill_from": "2025-01-01T00:00:00.000Z",
        "oldest_posted_at": "2025-11-04T18:22:00.000Z",
        "error": null,
        "created_at": "2026-08-06T09:15:00.000Z"
    }
    """;

    [Fact]
    public async Task BackfillPostsAsync_StartsARun()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(HttpStatusCode.Accepted, PostSyncJson);

        var sync = await client.Profiles.BackfillPostsAsync("prof-1", "2025-01-01");

        Assert.Equal("sync456def", sync.Id);
        Assert.Equal(PostSyncTrigger.Backfill, sync.Trigger);
        Assert.Equal(PostSyncStatus.Running, sync.Status);
        Assert.Equal(143, sync.PostsImported);
        Assert.NotNull(sync.OldestPostedAt);

        var request = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Contains("/api/profiles/prof-1/backfill_posts", request.Url);
        Assert.Contains("\"from\":\"2025-01-01\"", request.Body);
    }

    [Fact]
    public async Task BackfillPostsAsync_SendsIdempotencyKey()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(HttpStatusCode.Accepted, PostSyncJson);

        await client.Profiles.BackfillPostsAsync("prof-1", "2025-01-01", idempotencyKey: "key-1");

        Assert.Equal("key-1", handler.Requests[0].Headers["Idempotency-Key"]);
    }

    [Fact]
    public async Task BackfillPostsAsync_ThrowsConflictWhenAlreadyRunning()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(HttpStatusCode.Conflict, """
        {"error": "A posts backfill is already running for this profile", "profile_sync_id": "sync456def"}
        """);

        var ex = await Assert.ThrowsAsync<ConflictException>(
            () => client.Profiles.BackfillPostsAsync("prof-1", "2025-01-01"));

        Assert.Equal(409, ex.StatusCode);
        Assert.NotNull(ex.Response);
        Assert.Equal("sync456def", ex.Response!["profile_sync_id"].ToString());
    }

    [Fact]
    public async Task ListPostSyncsAsync_SendsFilters()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse($$"""
        {"total": 1, "page": 0, "per_page": 25, "data": [{{PostSyncJson}}]}
        """);

        var result = await client.Profiles.ListPostSyncsAsync(
            "prof-1", PostSyncTrigger.Backfill, PostSyncStatus.Running, perPage: 25);

        Assert.Equal(1, result.Total);
        Assert.Equal(143, result.Data[0].PostsImported);

        var url = handler.Requests[0].Url;
        Assert.Contains("/api/profiles/prof-1/post_syncs", url);
        Assert.Contains("trigger=backfill", url);
        Assert.Contains("status=running", url);
        Assert.Contains("per_page=25", url);
    }

    [Fact]
    public async Task GetPostSyncAsync_FetchesASingleRun()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(PostSyncJson.Replace("\"running\"", "\"completed\""));

        var sync = await client.Profiles.GetPostSyncAsync("prof-1", "sync456def");

        Assert.Equal(PostSyncStatus.Completed, sync.Status);
        Assert.Contains("/api/profiles/prof-1/post_syncs/sync456def", handler.Requests[0].Url);
    }

    [Fact]
    public async Task GetAsync_ParsesNewProfileFields()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "id": "prof-1", "name": "Acme", "username": "acme", "status": "inactive",
            "inactive_reason": "network_taken", "platform": "whatsapp", "platform_id": "waba_1",
            "avatar_url": "https://img.test/a.png", "platform_url": "https://wa.me/1555",
            "placements_sync_status": "synced"
        }
        """);

        var profile = await client.Profiles.GetAsync("prof-1");

        Assert.Equal("acme", profile.Username);
        Assert.Equal(ProfileInactiveReason.NetworkTaken, profile.InactiveReason);
        Assert.Equal(Platform.WhatsApp, profile.Platform);
        Assert.Equal("waba_1", profile.PlatformId);
        Assert.Equal("https://img.test/a.png", profile.AvatarUrl);
        Assert.Equal("https://wa.me/1555", profile.PlatformUrl);
        Assert.Equal("synced", profile.PlacementsSyncStatus);
    }

    [Fact]
    public async Task GetDailyStatsAsync_WithAttribution()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {"data": {"profile_id": "p1", "attribution": "publish", "records": [{"date": "2026-09-01", "impressions": 10, "platform_stats": null}], "metrics": {}}}
        """);

        var result = await client.Profiles.GetDailyStatsAsync("p1", placementId: "pl1", from: "2026-09-01", to: "2026-09-30", attribution: DailyStatsAttribution.Publish);

        Assert.Equal(10, result.Data.Records[0].Metrics["impressions"].GetInt32());
        Assert.Equal(DailyStatsAttribution.Publish, result.Data.Attribution);
        var url = handler.Requests[0].Url;
        Assert.StartsWith("/api/profiles/p1/daily_stats?", url);
        Assert.Contains("attribution=publish", url);
        Assert.Contains("placement_id=pl1", url);
    }

    [Fact]
    public async Task GetFollowerHistoryAsync_BuildsQuery()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"data": {"records": [{"date": "2026-09-01", "followers_total": 120}]}}""");

        var result = await client.Profiles.GetFollowerHistoryAsync("p1", from: "2026-09-01");

        Assert.Equal(120, result.Data.Records[0].FollowersTotal);
        Assert.Equal("/api/profiles/p1/follower_history?from=2026-09-01", handler.Requests[0].Url);
    }

    [Fact]
    public async Task GetParticipantStatusAsync_ReturnsStatus()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"is_verified_user": true, "fetched_at": "2026-10-01T00:00:00Z"}""");

        var result = await client.Profiles.GetParticipantStatusAsync("p1", "igsid_1");

        Assert.True(result.IsVerifiedUser);
        Assert.Equal("/api/profiles/p1/participants/igsid_1", handler.Requests[0].Url);
    }

    [Fact]
    public async Task ResolveMentionAsync_SendsUrl()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"urn": "urn:li:organization:1", "type": "organization", "name": "Acme", "vanity_name": "acme"}""");

        var result = await client.Profiles.ResolveMentionAsync("p1", "https://www.linkedin.com/company/acme");

        Assert.Equal("urn:li:organization:1", result.Urn);
        Assert.Contains("url=https%3A%2F%2Fwww.linkedin.com%2Fcompany%2Facme", handler.Requests[0].Url);
    }

    [Fact]
    public async Task GetInstagramAvailableAudioAsync_BuildsQuery()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"data": [{"id": "a1"}]}""");

        var result = await client.Profiles.GetInstagramAvailableAudioAsync("p1", InstagramAudioType.OriginalSound, "summer");

        Assert.Equal("a1", result.Data[0]["id"].GetString());
        Assert.Equal("/api/profiles/p1/instagram/available_audio?audio_type=original_sound&search_query=summer", handler.Requests[0].Url);
    }

    [Fact]
    public async Task FacebookPage_ReadAndUpdate()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"about": "Hi"}""");
        handler.EnqueueResponse("""{"success": true}""");

        var page = await client.Profiles.GetFacebookPageAsync("p1", "page_1", fields: "about,website");
        await client.Profiles.UpdateFacebookPageAsync("p1", "page_1", new Dictionary<string, object?> { ["about"] = "New about" });

        Assert.Equal("Hi", page.GetProperty("about").GetString());
        Assert.Equal("/api/profiles/p1/facebook/page?page_id=page_1&fields=about%2Cwebsite", handler.Requests[0].Url);
        Assert.Equal(HttpMethod.Patch, handler.Requests[1].Method);
        Assert.Equal("/api/profiles/p1/facebook/update_page", handler.Requests[1].Url);
        Assert.Equal("""{"about":"New about","page_id":"page_1"}""", handler.Requests[1].Body);
    }

    [Fact]
    public async Task AssignToGroupAsync_PatchesTargetGroup()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"id": "p1", "profile_group_id": "pg_2"}""");

        var profile = await client.Profiles.AssignToGroupAsync("p1", "pg_2");

        Assert.Equal("pg_2", profile.ProfileGroupId);
        Assert.Equal(HttpMethod.Patch, handler.Requests[0].Method);
        Assert.Equal("/api/profiles/p1/assign_to_group", handler.Requests[0].Url);
        Assert.Equal("""{"target_profile_group_id":"pg_2"}""", handler.Requests[0].Body);
    }

    [Fact]
    public async Task ChatBackfillAndSyncs()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        const string sync = """{"id": "cs1", "trigger": "backfill", "status": "pending", "chats_seen": 0, "messages_imported": 0, "created_at": "2026-10-01T00:00:00Z"}""";
        handler.EnqueueResponse(sync);
        handler.EnqueueResponse($$"""{"total": 1, "page": 1, "per_page": 5, "data": [{{sync}}]}""");
        handler.EnqueueResponse(sync);

        var started = await client.Profiles.BackfillChatsAsync("p1", "2026-01-01T00:00:00Z");
        var runs = await client.Profiles.ListChatSyncsAsync("p1", status: PostSyncStatus.Running, perPage: 5);
        await client.Profiles.GetChatSyncAsync("p1", "cs1");

        Assert.Equal(0, started.ChatsSeen);
        Assert.Equal(PostSyncTrigger.Backfill, runs.Data[0].Trigger);
        Assert.Equal(HttpMethod.Post, handler.Requests[0].Method);
        Assert.Equal("/api/profiles/p1/backfill_chats", handler.Requests[0].Url);
        Assert.Equal("""{"from":"2026-01-01T00:00:00Z"}""", handler.Requests[0].Body);
        Assert.Equal("/api/profiles/p1/chat_syncs?status=running&per_page=5", handler.Requests[1].Url);
        Assert.Equal("/api/profiles/p1/chat_syncs/cs1", handler.Requests[2].Url);
    }

    [Fact]
    public async Task InstagramCollaborationInvites_ListAcceptDecline()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "collaboration_invites": [
                {"media_id": "1789", "media_owner_username": "partner_brand", "caption": "Collab", "media_url": "https://x"}
            ],
            "next_cursor": null
        }
        """);
        handler.EnqueueResponse("""{"accepted": true, "media_id": "1789"}""");
        handler.EnqueueResponse("""{"declined": true, "media_id": "1790"}""");

        var list = await client.Profiles.ListInstagramCollaborationInvitesAsync("p1", after: "cur_1");
        var accepted = await client.Profiles.AcceptInstagramCollaborationInviteAsync("p1", "1789");
        var declined = await client.Profiles.DeclineInstagramCollaborationInviteAsync("p1", "1790");

        Assert.Equal("1789", list.CollaborationInvites[0].MediaId);
        Assert.Equal("partner_brand", list.CollaborationInvites[0].MediaOwnerUsername);
        Assert.Null(list.NextCursor);
        Assert.True(accepted.Accepted);
        Assert.True(declined.Declined);
        Assert.Equal("1790", declined.MediaId);

        var reqs = handler.Requests;
        Assert.Equal("/api/profiles/p1/instagram/collaboration_invites?after=cur_1", reqs[0].Url);
        Assert.Equal(HttpMethod.Post, reqs[1].Method);
        Assert.Equal("/api/profiles/p1/instagram/accept_collaboration_invite", reqs[1].Url);
        Assert.Equal("""{"media_id":"1789"}""", reqs[1].Body);
        Assert.Equal("/api/profiles/p1/instagram/decline_collaboration_invite", reqs[2].Url);
        Assert.Equal("""{"media_id":"1790"}""", reqs[2].Body);
    }

    [Fact]
    public async Task PinterestBoards_CreateAndDelete()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"id": "1022", "name": "Summer Recipes", "avatar_url": null, "metadata": {}}""");
        handler.EnqueueResponse("""{"deleted": true, "board_id": "1022"}""");

        var board = await client.Profiles.CreatePinterestBoardAsync("p1", "Summer Recipes", description: "Light dinners", privacy: PinterestBoardPrivacy.Protected);
        var deleted = await client.Profiles.DeletePinterestBoardAsync("p1", board.Id);

        Assert.Equal("1022", board.Id);
        Assert.True(deleted.Deleted);
        Assert.Equal("1022", deleted.BoardId);

        var reqs = handler.Requests;
        Assert.Equal(HttpMethod.Post, reqs[0].Method);
        Assert.Equal("/api/profiles/p1/pinterest/create_board", reqs[0].Url);
        Assert.Equal("""{"name":"Summer Recipes","description":"Light dinners","privacy":"PROTECTED"}""", reqs[0].Body);
        Assert.Equal(HttpMethod.Delete, reqs[1].Method);
        Assert.Equal("/api/profiles/p1/pinterest/delete_board?board_id=1022", reqs[1].Url);
    }

    [Fact]
    public async Task YouTubePlaylists_ListCreateUpdateDelete()
    {
        const string playlist = """
        {
            "id": "PL1",
            "title": "Tutorials",
            "description": "",
            "privacy_status": "public",
            "item_count": 3,
            "thumbnail_url": null,
            "published_at": "2026-03-14T09:21:00.000Z"
        }
        """;
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse($$"""{"data": [{{playlist}}], "next_page_token": null}""");
        handler.EnqueueResponse(playlist);
        handler.EnqueueResponse(playlist);
        handler.EnqueueResponse("""{"deleted": true, "playlist_id": "PL1"}""");

        var list = await client.Profiles.ListYouTubePlaylistsAsync("p1", pageToken: "tok");
        var created = await client.Profiles.CreateYouTubePlaylistAsync("p1", "Tutorials", privacyStatus: YouTubePrivacy.Unlisted);
        await client.Profiles.UpdateYouTubePlaylistAsync("p1", "PL1", description: "");
        var deleted = await client.Profiles.DeleteYouTubePlaylistAsync("p1", "PL1");

        Assert.Equal("PL1", list.Data[0].Id);
        Assert.Equal(YouTubePrivacy.Public, list.Data[0].PrivacyStatus);
        Assert.Equal(3, list.Data[0].ItemCount);
        Assert.Null(list.NextPageToken);
        Assert.Equal("Tutorials", created.Title);
        Assert.True(deleted.Deleted);

        var reqs = handler.Requests;
        Assert.Equal("/api/profiles/p1/youtube/playlists?page_token=tok", reqs[0].Url);
        Assert.Equal(HttpMethod.Post, reqs[1].Method);
        Assert.Equal("/api/profiles/p1/youtube/create_playlist", reqs[1].Url);
        Assert.Equal("""{"title":"Tutorials","privacy_status":"unlisted"}""", reqs[1].Body);
        Assert.Equal(HttpMethod.Patch, reqs[2].Method);
        Assert.Equal("/api/profiles/p1/youtube/update_playlist", reqs[2].Url);
        Assert.Equal("""{"playlist_id":"PL1","description":""}""", reqs[2].Body);
        Assert.Equal(HttpMethod.Delete, reqs[3].Method);
        Assert.Equal("/api/profiles/p1/youtube/delete_playlist?playlist_id=PL1", reqs[3].Url);
    }
}
