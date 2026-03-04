using System.Net;
using System.Text.Json;
using PostProxy.Exceptions;
using PostProxy.Models;
using PostProxy.Parameters;

namespace PostProxy.Tests;

public class PostsResourceTests
{
    [Fact]
    public async Task ListAsync_ReturnsPagedPosts()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "data": [{"id": "post-1", "body": "Hello", "status": "draft"}],
            "total": 1,
            "page": 0,
            "per_page": 10
        }
        """);

        var result = await client.Posts.ListAsync();

        Assert.Single(result.Data);
        Assert.Equal("post-1", result.Data[0].Id);
        Assert.Equal(PostStatus.Draft, result.Data[0].Status);
        Assert.Equal("/api/posts", handler.Requests[0].Url);
    }

    [Fact]
    public async Task ListAsync_WithParams_BuildsQueryString()
    {
        var (client, handler) = TestHelpers.CreateMockClient("pg-1");
        handler.EnqueueResponse("""{"data": [], "total": 0, "page": 0, "per_page": 10}""");

        await client.Posts.ListAsync(new ListPostsParams
        {
            Page = 1,
            PerPage = 5,
            Status = PostStatus.Pending,
        });

        var url = handler.Requests[0].Url;
        Assert.Contains("profile_group_id=pg-1", url);
        Assert.Contains("page=1", url);
        Assert.Contains("per_page=5", url);
        Assert.Contains("status=pending", url);
    }

    [Fact]
    public async Task GetAsync_ReturnsPost()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"id": "post-1", "body": "Hello", "status": "processed"}""");

        var post = await client.Posts.GetAsync("post-1");

        Assert.Equal("post-1", post.Id);
        Assert.Equal("Hello", post.Body);
        Assert.Equal(PostStatus.Processed, post.Status);
        Assert.Contains("/api/posts/post-1", handler.Requests[0].Url);
    }

    [Fact]
    public async Task CreateAsync_SendsCorrectBody()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"id": "post-new", "body": "New post", "status": "pending"}""");

        var post = await client.Posts.CreateAsync(new CreatePostParams
        {
            Body = "New post",
            Profiles = ["profile-1"],
            Draft = true,
        });

        Assert.Equal("post-new", post.Id);
        var request = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Contains("\"body\":\"New post\"", request.Body);
        Assert.Contains("\"draft\":true", request.Body);
    }

    [Fact]
    public async Task CreateAsync_WithPlatformParams_SendsCorrectBody()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"id": "post-ig", "body": "Reel", "status": "pending"}""");

        await client.Posts.CreateAsync(new CreatePostParams
        {
            Body = "Reel",
            Profiles = ["ig-1"],
            Platforms = new PlatformParams
            {
                Instagram = new InstagramParams
                {
                    Format = InstagramFormat.Reel,
                    Collaborators = ["friend"],
                },
            },
        });

        var body = handler.Requests[0].Body!;
        Assert.Contains("\"instagram\"", body);
        Assert.Contains("\"reel\"", body);
    }

    [Fact]
    public async Task CreateAsync_ThrowsOnEmptyBody()
    {
        var (client, _) = TestHelpers.CreateMockClient();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Posts.CreateAsync(new CreatePostParams
            {
                Body = "",
                Profiles = ["p1"],
            }));
    }

    [Fact]
    public async Task PublishDraftAsync_PostsCorrectUrl()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"id": "post-1", "body": "Hello", "status": "pending"}""");

        await client.Posts.PublishDraftAsync("post-1");

        Assert.Equal(HttpMethod.Post, handler.Requests[0].Method);
        Assert.Contains("/api/posts/post-1/publish", handler.Requests[0].Url);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsDeleteResponse()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"deleted": true}""");

        var result = await client.Posts.DeleteAsync("post-1");

        Assert.True(result.Deleted);
        Assert.Equal(HttpMethod.Delete, handler.Requests[0].Method);
    }

    [Fact]
    public async Task GetAsync_ThrowsNotFoundException()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(HttpStatusCode.NotFound, """{"error": "Post not found"}""");

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            client.Posts.GetAsync("nonexistent"));

        Assert.Equal(404, ex.StatusCode);
        Assert.Equal("Post not found", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_ThrowsValidationException()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(HttpStatusCode.UnprocessableEntity, """{"error": "Body too long"}""");

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            client.Posts.CreateAsync(new CreatePostParams
            {
                Body = "test",
                Profiles = ["p1"],
            }));

        Assert.Equal(422, ex.StatusCode);
    }

    [Fact]
    public async Task StatsAsync_ReturnsStats()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "data": {
                "abc123": {
                    "platforms": [
                        {
                            "profile_id": "prof_abc",
                            "platform": "instagram",
                            "records": [
                                {
                                    "stats": { "impressions": 1200, "likes": 85 },
                                    "recorded_at": "2026-02-20T12:00:00Z"
                                },
                                {
                                    "stats": { "impressions": 1523, "likes": 102 },
                                    "recorded_at": "2026-02-21T04:00:00Z"
                                }
                            ]
                        }
                    ]
                }
            }
        }
        """);

        var result = await client.Posts.StatsAsync(new PostStatsParams
        {
            PostIds = ["abc123"],
        });

        Assert.True(result.Data.ContainsKey("abc123"));
        var postStats = result.Data["abc123"];
        Assert.Single(postStats.Platforms);
        Assert.Equal(Platform.Instagram, postStats.Platforms[0].Platform);
        Assert.Equal("prof_abc", postStats.Platforms[0].ProfileId);
        Assert.Equal(2, postStats.Platforms[0].Records.Count);
        Assert.Equal(1200, postStats.Platforms[0].Records[0].Stats["impressions"]);
        Assert.Equal(85, postStats.Platforms[0].Records[0].Stats["likes"]);
    }

    [Fact]
    public async Task StatsAsync_WithFilters_BuildsQueryString()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"data": {}}""");

        await client.Posts.StatsAsync(new PostStatsParams
        {
            PostIds = ["abc123", "def456"],
            Profiles = ["instagram", "twitter"],
            From = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero),
            To = new DateTimeOffset(2026, 2, 24, 0, 0, 0, TimeSpan.Zero),
        });

        var url = handler.Requests[0].Url;
        Assert.Contains("post_ids=abc123%2Cdef456", url);
        Assert.Contains("profiles=instagram%2Ctwitter", url);
        Assert.Contains("from=", url);
        Assert.Contains("to=", url);
        Assert.Equal(HttpMethod.Get, handler.Requests[0].Method);
    }

    [Fact]
    public async Task StatsAsync_ThrowsOnEmptyPostIds()
    {
        var (client, _) = TestHelpers.CreateMockClient();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Posts.StatsAsync(new PostStatsParams
            {
                PostIds = [],
            }));
    }

    [Fact]
    public async Task CreateAsync_WithThread_SendsThreadInBody()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "id": "post-thread",
            "body": "Main post",
            "status": "pending",
            "thread": [
                {"id": "t-1", "body": "Reply 1", "media": []},
                {"id": "t-2", "body": "Reply 2", "media": []}
            ]
        }
        """);

        var post = await client.Posts.CreateAsync(new CreatePostParams
        {
            Body = "Main post",
            Profiles = ["profile-1"],
            Thread =
            [
                new ThreadChildInput { Body = "Reply 1" },
                new ThreadChildInput { Body = "Reply 2", Media = ["https://example.com/img.jpg"] },
            ],
        });

        Assert.Equal("post-thread", post.Id);
        Assert.NotNull(post.Thread);
        Assert.Equal(2, post.Thread.Count);
        Assert.Equal("Reply 1", post.Thread[0].Body);

        var body = handler.Requests[0].Body!;
        Assert.Contains("\"thread\"", body);
        Assert.Contains("Reply 1", body);
        Assert.Contains("https://example.com/img.jpg", body);
    }

    [Fact]
    public async Task GetAsync_WithMediaAndThread_Deserializes()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "id": "post-1",
            "body": "Hello",
            "status": "media_processing_failed",
            "media": [
                {"id": "m-1", "status": "processed", "content_type": "image/jpeg", "url": "https://cdn.example.com/img.jpg"}
            ],
            "thread": [
                {"id": "t-1", "body": "Reply", "media": []}
            ]
        }
        """);

        var post = await client.Posts.GetAsync("post-1");

        Assert.Equal(PostStatus.MediaProcessingFailed, post.Status);
        Assert.NotNull(post.Media);
        Assert.Single(post.Media);
        Assert.Equal("m-1", post.Media[0].Id);
        Assert.Equal(MediaStatus.Processed, post.Media[0].Status);
        Assert.NotNull(post.Thread);
        Assert.Single(post.Thread);
        Assert.Equal("Reply", post.Thread[0].Body);
    }
}
