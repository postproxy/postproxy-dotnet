using PostProxy.Models;

namespace PostProxy.Tests;

public class QueuesResourceTests
{
    [Fact]
    public async Task ListAsync_ReturnsQueues()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "data": [
                {
                    "id": "q1abc",
                    "name": "Morning Posts",
                    "description": "Daily morning content",
                    "timezone": "America/New_York",
                    "enabled": true,
                    "jitter": 10,
                    "profile_group_id": "pg123",
                    "timeslots": [
                        {"id": 1, "day": 1, "time": "09:00"},
                        {"id": 2, "day": 3, "time": "09:00"}
                    ],
                    "posts_count": 5
                }
            ]
        }
        """);

        var result = await client.Queues.ListAsync();

        Assert.Single(result.Data);
        Assert.Equal("q1abc", result.Data[0].Id);
        Assert.Equal("Morning Posts", result.Data[0].Name);
        Assert.Equal(2, result.Data[0].Timeslots!.Count);
        Assert.Equal(HttpMethod.Get, handler.Requests[0].Method);
        Assert.Contains("/api/post_queues", handler.Requests[0].Url);
    }

    [Fact]
    public async Task GetAsync_ReturnsQueue()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "id": "q1abc",
            "name": "Morning Posts",
            "description": "Daily morning content",
            "timezone": "America/New_York",
            "enabled": true,
            "jitter": 10,
            "profile_group_id": "pg123",
            "timeslots": [
                {"id": 1, "day": 1, "time": "09:00"}
            ],
            "posts_count": 5
        }
        """);

        var queue = await client.Queues.GetAsync("q1abc");

        Assert.Equal("q1abc", queue.Id);
        Assert.Equal("Morning Posts", queue.Name);
        Assert.True(queue.Enabled);
        Assert.Equal(10, queue.Jitter);
        Assert.Contains("/api/post_queues/q1abc", handler.Requests[0].Url);
    }

    [Fact]
    public async Task NextSlotAsync_ReturnsNextSlot()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"next_slot": "2026-03-11T14:00:00Z"}""");

        var result = await client.Queues.NextSlotAsync("q1abc");

        Assert.Equal("2026-03-11T14:00:00Z", result.NextSlot);
        Assert.Contains("/api/post_queues/q1abc/next_slot", handler.Requests[0].Url);
    }

    [Fact]
    public async Task CreateAsync_SendsCorrectBody()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "id": "q1abc",
            "name": "Morning Posts",
            "timezone": "America/New_York",
            "enabled": true,
            "jitter": 10,
            "profile_group_id": "pg123",
            "timeslots": [
                {"id": 1, "day": 1, "time": "09:00"}
            ],
            "posts_count": 0
        }
        """);

        var queue = await client.Queues.CreateAsync(
            "Morning Posts",
            "pg123",
            timezone: "America/New_York",
            jitter: 10,
            timeslots: new object[]
            {
                new Dictionary<string, object> { ["day"] = 1, ["time"] = "09:00" },
            });

        Assert.Equal("q1abc", queue.Id);

        var request = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Contains("\"profile_group_id\":\"pg123\"", request.Body);
        Assert.Contains("\"post_queue\"", request.Body);
        Assert.Contains("\"name\":\"Morning Posts\"", request.Body);
        Assert.Contains("\"timezone\":\"America/New_York\"", request.Body);
    }

    [Fact]
    public async Task UpdateAsync_SendsPatchRequest()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""
        {
            "id": "q1abc",
            "name": "Morning Posts",
            "enabled": false,
            "jitter": 15,
            "profile_group_id": "pg123",
            "timeslots": [],
            "posts_count": 0
        }
        """);

        var queue = await client.Queues.UpdateAsync("q1abc", enabled: false, jitter: 15);

        Assert.False(queue.Enabled);
        Assert.Equal(15, queue.Jitter);

        var request = handler.Requests[0];
        Assert.Equal(HttpMethod.Patch, request.Method);
        Assert.Contains("\"post_queue\"", request.Body);
        Assert.Contains("\"enabled\":false", request.Body);
        Assert.Contains("\"jitter\":15", request.Body);
        Assert.Contains("/api/post_queues/q1abc", request.Url);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsDeleteResponse()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"deleted": true}""");

        var result = await client.Queues.DeleteAsync("q1abc");

        Assert.True(result.Deleted);
        Assert.Equal(HttpMethod.Delete, handler.Requests[0].Method);
        Assert.Contains("/api/post_queues/q1abc", handler.Requests[0].Url);
    }
}
