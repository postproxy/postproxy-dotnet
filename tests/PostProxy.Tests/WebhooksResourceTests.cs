using System.Net;
using PostProxy.Models;

namespace PostProxy.Tests;

public class WebhooksResourceTests
{
    [Fact]
    public async Task ListAsync_ReturnsWebhooks()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "data": [
                {
                    "id": "wh-1",
                    "url": "https://example.com/webhook",
                    "events": ["post.published", "post.failed"],
                    "enabled": true,
                    "description": "Test webhook",
                    "secret": "whsec_test123",
                    "created_at": "2025-01-01T00:00:00Z",
                    "updated_at": "2025-01-01T00:00:00Z"
                }
            ]
        }
        """);

        var result = await client.Webhooks.ListAsync();

        Assert.Single(result.Data);
        Assert.Equal("wh-1", result.Data[0].Id);
        Assert.Equal("https://example.com/webhook", result.Data[0].Url);
        Assert.Equal(HttpMethod.Get, handler.Requests[0].Method);
        Assert.Contains("/api/webhooks", handler.Requests[0].Url);
    }

    [Fact]
    public async Task GetAsync_ReturnsWebhook()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "id": "wh-1",
            "url": "https://example.com/webhook",
            "events": ["post.published", "post.failed"],
            "enabled": true,
            "secret": "whsec_test123"
        }
        """);

        var webhook = await client.Webhooks.GetAsync("wh-1");

        Assert.Equal("wh-1", webhook.Id);
        Assert.Equal(new[] { "post.published", "post.failed" }, webhook.Events);
        Assert.True(webhook.Enabled);
        Assert.Equal("whsec_test123", webhook.Secret);
        Assert.Contains("/api/webhooks/wh-1", handler.Requests[0].Url);
    }

    [Fact]
    public async Task CreateAsync_SendsCorrectBody()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "id": "wh-new",
            "url": "https://example.com/webhook",
            "events": ["post.published"],
            "enabled": true,
            "secret": "whsec_abc"
        }
        """);

        var webhook = await client.Webhooks.CreateAsync(
            "https://example.com/webhook",
            ["post.published"],
            description: "Test webhook");

        Assert.Equal("wh-new", webhook.Id);

        var request = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Contains("\"url\":\"https://example.com/webhook\"", request.Body);
        Assert.Contains("\"post.published\"", request.Body);
        Assert.Contains("\"description\":\"Test webhook\"", request.Body);
    }

    [Fact]
    public async Task UpdateAsync_SendsPatchRequest()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "id": "wh-1",
            "url": "https://example.com/webhook",
            "events": ["post.published"],
            "enabled": false
        }
        """);

        var webhook = await client.Webhooks.UpdateAsync("wh-1", enabled: false);

        Assert.False(webhook.Enabled);

        var request = handler.Requests[0];
        Assert.Equal(HttpMethod.Patch, request.Method);
        Assert.Contains("\"enabled\":false", request.Body);
        Assert.Contains("/api/webhooks/wh-1", request.Url);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsDeleteResponse()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"deleted": true}""");

        var result = await client.Webhooks.DeleteAsync("wh-1");

        Assert.True(result.Deleted);
        Assert.Equal(HttpMethod.Delete, handler.Requests[0].Method);
    }

    [Fact]
    public async Task DeliveriesAsync_ReturnsPaginatedDeliveries()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "data": [
                {
                    "id": "del-1",
                    "event_id": "evt-1",
                    "event_type": "post.published",
                    "response_status": 200,
                    "attempt_number": 1,
                    "success": true,
                    "attempted_at": "2025-01-01T00:00:00Z",
                    "created_at": "2025-01-01T00:00:00Z"
                }
            ],
            "total": 1,
            "page": 1,
            "per_page": 10
        }
        """);

        var result = await client.Webhooks.DeliveriesAsync("wh-1", page: 1, perPage: 10);

        Assert.Single(result.Data);
        Assert.Equal("del-1", result.Data[0].Id);
        Assert.Equal("post.published", result.Data[0].EventType);
        Assert.True(result.Data[0].Success);
        Assert.Equal(1, result.Total);

        var url = handler.Requests[0].Url;
        Assert.Contains("/api/webhooks/wh-1/deliveries", url);
        Assert.Contains("page=1", url);
        Assert.Contains("per_page=10", url);
    }

    [Fact]
    public void VerifySignature_ValidSignature()
    {
        var payload = """{"event":"post.published","data":{"id":"post-1"}}""";
        var secret = "whsec_test123";
        var signature = "t=1234567890,v1=c8e99efbb07ac8e3152c02dd8d83e8ddb803ae8fb001d9e1ab42fb0b1f405ef2";

        Assert.True(WebhookSignature.Verify(payload, signature, secret));
    }

    [Fact]
    public void VerifySignature_InvalidSignature()
    {
        var payload = """{"event":"post.published","data":{"id":"post-1"}}""";
        var secret = "whsec_test123";
        var signature = "t=1234567890,v1=invalidsignature";

        Assert.False(WebhookSignature.Verify(payload, signature, secret));
    }

    [Fact]
    public void VerifySignature_WrongSecret()
    {
        var payload = """{"event":"post.published","data":{"id":"post-1"}}""";
        var secret = "wrong_secret";
        var signature = "t=1234567890,v1=c8e99efbb07ac8e3152c02dd8d83e8ddb803ae8fb001d9e1ab42fb0b1f405ef2";

        Assert.False(WebhookSignature.Verify(payload, signature, secret));
    }

    [Fact]
    public async Task PostWithMediaAndThread_Deserializes()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "id": "post-1",
            "body": "Hello",
            "status": "media_processing_failed",
            "media": [
                {
                    "id": "m-1",
                    "status": "processed",
                    "content_type": "image/jpeg",
                    "url": "https://cdn.example.com/img.jpg"
                }
            ],
            "thread": [
                {"id": "t-1", "body": "Thread reply", "media": []}
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
        Assert.Equal("Thread reply", post.Thread[0].Body);
    }
}
