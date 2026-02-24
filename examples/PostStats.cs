using PostProxy;
using PostProxy.Parameters;

var client = PostProxyClient.Builder("your-api-key")
    .ProfileGroupId("your-profile-group-id")
    .Build();

// Get stats for specific posts
var stats = await client.Posts.StatsAsync(new PostStatsParams
{
    PostIds = ["post-id-1", "post-id-2"],
});

foreach (var (postId, postStats) in stats.Data)
{
    Console.WriteLine($"Post: {postId}");
    foreach (var platform in postStats.Platforms)
    {
        Console.WriteLine($"  {platform.Platform} ({platform.ProfileId})");
        foreach (var record in platform.Records)
        {
            Console.WriteLine($"    {record.RecordedAt:g}");
            foreach (var (metric, value) in record.Stats)
                Console.WriteLine($"      {metric}: {value}");
        }
    }
}

// Filter by profiles/networks and time range
var filtered = await client.Posts.StatsAsync(new PostStatsParams
{
    PostIds = ["post-id-1"],
    Profiles = ["instagram", "twitter"],
    From = DateTimeOffset.UtcNow.AddDays(-7),
    To = DateTimeOffset.UtcNow,
});

foreach (var (postId, postStats) in filtered.Data)
{
    foreach (var platform in postStats.Platforms)
    {
        var latest = platform.Records.LastOrDefault();
        if (latest is not null)
            Console.WriteLine($"{postId} on {platform.Platform}: {string.Join(", ", latest.Stats.Select(s => $"{s.Key}={s.Value}"))}");
    }
}
