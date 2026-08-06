using PostProxy.Models;

namespace PostProxy.Tests;

public class CommentsResourceTests
{
    [Fact]
    public async Task ListAsync_ReturnsComments()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "total": 1,
            "page": 0,
            "per_page": 20,
            "data": [
                {
                    "id": "cmt_abc123",
                    "external_id": "17858893269123456",
                    "body": "Great post!",
                    "status": "synced",
                    "author_username": "someuser",
                    "like_count": 3,
                    "is_hidden": false,
                    "created_at": "2026-03-25T10:01:00Z",
                    "replies": [
                        {
                            "id": "cmt_def456",
                            "body": "Thanks!",
                            "status": "synced",
                            "author_username": "author",
                            "like_count": 1,
                            "is_hidden": false,
                            "created_at": "2026-03-25T10:05:00Z",
                            "replies": []
                        }
                    ]
                }
            ]
        }
        """);

        var result = await client.Comments.ListAsync("post1", "prof1");

        Assert.Equal(1, result.Total);
        Assert.Single(result.Data);
        Assert.Equal("cmt_abc123", result.Data[0].Id);
        Assert.Equal("Great post!", result.Data[0].Body);
        Assert.Single(result.Data[0].Replies!);
        Assert.Equal("cmt_def456", result.Data[0].Replies![0].Id);

        var req = handler.Requests[0];
        Assert.Equal(HttpMethod.Get, req.Method);
        Assert.Contains("/api/posts/post1/comments", req.Url);
        Assert.Contains("profile_id=prof1", req.Url);
    }

    [Fact]
    public async Task ListAsync_WithPagination()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "total": 42,
            "page": 2,
            "per_page": 10,
            "data": []
        }
        """);

        var result = await client.Comments.ListAsync("post1", "prof1", page: 2, perPage: 10);

        Assert.Equal(42, result.Total);

        var url = handler.Requests[0].Url;
        Assert.Contains("page=2", url);
        Assert.Contains("per_page=10", url);
    }

    [Fact]
    public async Task GetAsync_ReturnsComment()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "id": "cmt_abc123",
            "body": "Great post!",
            "status": "synced",
            "author_username": "someuser",
            "like_count": 3,
            "is_hidden": false,
            "created_at": "2026-03-25T10:01:00Z",
            "replies": []
        }
        """);

        var comment = await client.Comments.GetAsync("post1", "cmt_abc123", "prof1");

        Assert.Equal("cmt_abc123", comment.Id);
        Assert.Equal("Great post!", comment.Body);
        Assert.Equal(3, comment.LikeCount);
        Assert.Contains("/api/posts/post1/comments/cmt_abc123", handler.Requests[0].Url);
    }

    [Fact]
    public async Task CreateAsync_SendsCorrectBody()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "id": "cmt_new",
            "body": "Nice!",
            "status": "pending",
            "like_count": 0,
            "is_hidden": false,
            "created_at": "2026-03-25T12:00:00Z",
            "replies": []
        }
        """);

        var comment = await client.Comments.CreateAsync("post1", "prof1", "Nice!");

        Assert.Equal("cmt_new", comment.Id);
        Assert.Equal("pending", comment.Status);

        var req = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.Contains("\"text\":\"Nice!\"", req.Body);
        Assert.DoesNotContain("parent_id", req.Body);
    }

    [Fact]
    public async Task CreateAsync_WithParentId()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "id": "cmt_reply",
            "body": "Thanks!",
            "status": "pending",
            "like_count": 0,
            "is_hidden": false,
            "created_at": "2026-03-25T12:00:00Z",
            "replies": []
        }
        """);

        await client.Comments.CreateAsync("post1", "prof1", "Thanks!", parentId: "cmt_abc123");

        var req = handler.Requests[0];
        Assert.Contains("\"text\":\"Thanks!\"", req.Body);
        Assert.Contains("\"parent_id\":\"cmt_abc123\"", req.Body);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsAccepted()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"accepted": true}""");

        var result = await client.Comments.DeleteAsync("post1", "cmt_abc123", "prof1");

        Assert.True(result.Accepted);
        Assert.Equal(HttpMethod.Delete, handler.Requests[0].Method);
    }

    [Fact]
    public async Task HideAsync_PostsToCorrectUrl()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"accepted": true}""");

        var result = await client.Comments.HideAsync("post1", "cmt_abc123", "prof1");

        Assert.True(result.Accepted);
        Assert.Contains("/comments/cmt_abc123/hide", handler.Requests[0].Url);
    }

    [Fact]
    public async Task UnhideAsync_PostsToCorrectUrl()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"accepted": true}""");

        var result = await client.Comments.UnhideAsync("post1", "cmt_abc123", "prof1");

        Assert.True(result.Accepted);
        Assert.Contains("/comments/cmt_abc123/unhide", handler.Requests[0].Url);
    }

    [Fact]
    public async Task LikeAsync_PostsToCorrectUrl()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"accepted": true}""");

        var result = await client.Comments.LikeAsync("post1", "cmt_abc123", "prof1");

        Assert.True(result.Accepted);
        Assert.Contains("/comments/cmt_abc123/like", handler.Requests[0].Url);
    }

    [Fact]
    public async Task UnlikeAsync_PostsToCorrectUrl()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"accepted": true}""");

        var result = await client.Comments.UnlikeAsync("post1", "cmt_abc123", "prof1");

        Assert.True(result.Accepted);
        Assert.Contains("/comments/cmt_abc123/unlike", handler.Requests[0].Url);
    }

    [Fact]
    public async Task GetAsync_ParsesAttachmentsAndMetadata()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "id": "cmt_abc123",
            "body": "Check this out",
            "status": "synced",
            "author_username": "someuser",
            "like_count": 0,
            "is_hidden": false,
            "metadata": {"is_verified_user": true, "follower_count": 1200},
            "attachments": [
                {"id": "att_1", "type": "image", "url": "https://cdn.example.com/a.jpg", "status": "processed", "external_id": "ext_1"}
            ],
            "created_at": "2026-03-25T10:01:00Z",
            "replies": []
        }
        """);

        var comment = await client.Comments.GetAsync("post1", "cmt_abc123", "prof1");

        Assert.NotNull(comment.Attachments);
        Assert.Single(comment.Attachments!);
        Assert.Equal("att_1", comment.Attachments![0].Id);
        Assert.Equal("image", comment.Attachments![0].Type);
        Assert.Equal(MediaStatus.Processed, comment.Attachments![0].Status);
        Assert.NotNull(comment.Metadata);
        Assert.True(comment.Metadata!["is_verified_user"].GetBoolean());
        Assert.Equal(1200, comment.Metadata!["follower_count"].GetInt32());
    }

    [Fact]
    public async Task PrivateReplyAsync_ReturnsMessage()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "id": "msg_222",
            "chat_id": "chat_xyz789",
            "direction": "outbound",
            "body": "DM-ing you the details.",
            "status": "pending",
            "external_comment_id": "17858893269123456",
            "created_at": "2026-05-31T15:30:05.000Z"
        }
        """);

        var msg = await client.Comments.PrivateReplyAsync("post1", "cmt_abc123", "prof1", "DM-ing you the details.");

        Assert.Equal("msg_222", msg.Id);
        Assert.Equal("17858893269123456", msg.ExternalCommentId);

        var req = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.Contains("/api/posts/post1/comments/cmt_abc123/private_reply", req.Url);
        Assert.Contains("profile_id=prof1", req.Url);
        Assert.Contains("\"text\":\"DM-ing you the details.\"", req.Body);
    }

    [Fact]
    public async Task ListAsync_SendsDateFilters()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"total": 0, "page": 0, "per_page": 20, "data": []}""");

        await client.Comments.ListAsync("post-1", "prof-1", from: "2026-03-25", to: "2026-03-26T12:00:00Z");

        var url = handler.Requests[0].Url;
        Assert.Contains("from=2026-03-25", url);
        Assert.Contains("to=2026-03-26T12%3A00%3A00Z", url);
    }

    [Fact]
    public async Task ListAllAsync_ReturnsFlatComments()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "total": 2,
            "page": 0,
            "per_page": 50,
            "data": [
                {
                    "post_id": "abc123xyz", "profile_id": "prof456", "platform": "instagram",
                    "id": "cmt_abc123", "body": "Great post!", "parent_external_id": null,
                    "like_count": 3, "is_hidden": false, "created_at": "2026-03-25T10:01:00Z"
                },
                {
                    "post_id": "abc123xyz", "profile_id": "prof456", "platform": "instagram",
                    "id": "cmt_def456", "body": "Thanks!", "parent_external_id": "17858893269123456",
                    "like_count": 1, "is_hidden": false, "created_at": "2026-03-25T10:05:00Z"
                }
            ]
        }
        """);

        var result = await client.Comments.ListAllAsync(
            postIds: ["abc123xyz", "def456uvw"],
            profiles: ["instagram", "prof456"],
            from: "2026-03-25",
            perPage: 50);

        Assert.Equal(2, result.Total);
        Assert.Equal("abc123xyz", result.Data[0].PostId);
        Assert.Equal("prof456", result.Data[0].ProfileId);
        Assert.Equal(Platform.Instagram, result.Data[0].Platform);
        // Flat: the reply is its own entry, linked by parent_external_id.
        Assert.Equal("17858893269123456", result.Data[1].ParentExternalId);

        var url = handler.Requests[0].Url;
        Assert.Contains("/api/comments", url);
        Assert.Contains("post_ids=abc123xyz%2Cdef456uvw", url);
        Assert.Contains("profiles=instagram%2Cprof456", url);
        Assert.Contains("per_page=50", url);
    }

    [Fact]
    public async Task ListAllAsync_WithoutFilters()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"total": 0, "page": 0, "per_page": 20, "data": []}""");

        await client.Comments.ListAllAsync();

        var url = handler.Requests[0].Url;
        Assert.Contains("/api/comments", url);
        Assert.DoesNotContain("profiles=", url);
        Assert.DoesNotContain("post_ids=", url);
    }

    [Fact]
    public async Task CreateAsync_SendsIdempotencyKey()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"id": "cmt_abc123", "body": "Nice", "created_at": "2026-03-25T10:01:00Z"}""");

        await client.Comments.CreateAsync("post-1", "prof-1", "Nice", idempotencyKey: "key-42");

        Assert.Equal("key-42", handler.Requests[0].Headers["Idempotency-Key"]);
    }
}
