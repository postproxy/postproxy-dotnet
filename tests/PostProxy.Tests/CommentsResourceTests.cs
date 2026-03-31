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
}
