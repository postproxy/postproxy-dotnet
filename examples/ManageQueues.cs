using PostProxy;
using PostProxy.Parameters;

var client = PostProxyClient.Builder("your-api-key")
    .ProfileGroupId("your-profile-group-id")
    .Build();

// List all queues
var queues = await client.Queues.ListAsync();
Console.WriteLine($"Queues: {queues.Data.Count}");

// Create a queue with timeslots
var queue = await client.Queues.CreateAsync(
    "Morning Posts",
    "your-profile-group-id",
    description: "Weekday morning content",
    timezone: "America/New_York",
    jitter: 10,
    timeslots: new object[]
    {
        new Dictionary<string, object> { ["day"] = 1, ["time"] = "09:00" },
        new Dictionary<string, object> { ["day"] = 2, ["time"] = "09:00" },
        new Dictionary<string, object> { ["day"] = 3, ["time"] = "09:00" },
    });
Console.WriteLine($"Created queue: {queue.Id} {queue.Name}");
Console.WriteLine($"Timeslots: {queue.Timeslots?.Count}");

// Get a queue
var fetched = await client.Queues.GetAsync(queue.Id);
Console.WriteLine($"Fetched: {fetched.Name} enabled={fetched.Enabled}");

// Get next available slot
var nextSlot = await client.Queues.NextSlotAsync(queue.Id);
Console.WriteLine($"Next slot: {nextSlot.NextSlot}");

// Update the queue — change jitter and add a timeslot
var updated = await client.Queues.UpdateAsync(queue.Id,
    jitter: 15,
    timeslots: new object[]
    {
        new Dictionary<string, object> { ["day"] = 4, ["time"] = "09:00" },
    });
Console.WriteLine($"Updated jitter: {updated.Jitter}");

// Pause the queue
var paused = await client.Queues.UpdateAsync(queue.Id, enabled: false);
Console.WriteLine($"Paused: enabled={paused.Enabled}");

// Add a post to the queue
var profiles = await client.Profiles.ListAsync();
var profileId = profiles.Data[0].Id;

var post = await client.Posts.CreateAsync(new CreatePostParams
{
    Body = "This post will be scheduled by the queue",
    Profiles = [profileId],
    QueueId = queue.Id,
    QueuePriority = "high",
});
Console.WriteLine($"Queued post: {post.Id}");

// Delete the queue
var deleted = await client.Queues.DeleteAsync(queue.Id);
Console.WriteLine($"Deleted: {deleted.Deleted}");
