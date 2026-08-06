// Backfill a profile's older posts and follow the sync run to completion.
using PostProxy;
using PostProxy.Exceptions;
using PostProxy.Models;

var client = PostProxyClient.Builder(Environment.GetEnvironmentVariable("POSTPROXY_API_KEY")!)
    .ProfileGroupId(Environment.GetEnvironmentVariable("POSTPROXY_PROFILE_GROUP_ID")!)
    .Build();

var profileId = "your-profile-id";

// Start a backfill. It walks the profile's feed backwards from the newest post
// in batches of 25 and stops at `from` — or earlier, if the platform stops
// returning history. Runs in the background.
PostSync sync;
try
{
    sync = await client.Profiles.BackfillPostsAsync(profileId, "2025-01-01");
}
catch (ConflictException e)
{
    // Only one backfill runs per profile at a time; the running one already
    // covers any window a second request could ask for.
    var runningId = e.Response!["profile_sync_id"].ToString()!;
    Console.WriteLine($"Backfill already running: {runningId}");
    sync = await client.Profiles.GetPostSyncAsync(profileId, runningId);
}

Console.WriteLine($"Backfill {sync.Id} — status: {sync.Status}");

// Poll until it finishes.
while (sync.Status is PostSyncStatus.Pending or PostSyncStatus.Running)
{
    await Task.Delay(TimeSpan.FromSeconds(5));
    sync = await client.Profiles.GetPostSyncAsync(profileId, sync.Id);
    Console.WriteLine(
        $"  {sync.Status}: {sync.PostsImported} imported of {sync.PostsSeen} seen, " +
        $"reached back to {sync.OldestPostedAt?.ToString("O") ?? "—"}");
}

if (sync.Status == PostSyncStatus.Failed)
    Console.WriteLine($"Backfill failed: {sync.Error}");
else
    Console.WriteLine($"Done. Imported {sync.PostsImported} posts");

// Every pull is recorded — the sync fired on connect, the recurring poll, and
// each backfill. Runs are kept for 30 days.
var runs = await client.Profiles.ListPostSyncsAsync(profileId, perPage: 10);
Console.WriteLine($"\nRecent post syncs ({runs.Total}):");
foreach (var run in runs.Data)
{
    Console.WriteLine(
        $"  {run.CreatedAt:O} {run.Trigger} → {run.Status} ({run.PostsImported}/{run.PostsSeen} new)");
}
