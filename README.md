# PostProxy .NET SDK

.NET client for the [PostProxy API](https://postproxy.dev). Uses C# records for models, `System.Net.Http.HttpClient` for HTTP, and `System.Text.Json` for JSON. No external dependencies beyond Microsoft.Extensions for DI support.

## Installation

### Package Manager

```
dotnet add package PostProxy
```

### PackageReference

```xml
<PackageReference Include="PostProxy" Version="1.0.0" />
```

Requires .NET 8+.

## Quick start

```csharp
using PostProxy;
using PostProxy.Parameters;

var client = PostProxyClient.Builder("your-api-key")
    .ProfileGroupId("pg-abc")
    .Build();

// List profiles
var profiles = await client.Profiles.ListAsync();

// Create a post
var post = await client.Posts.CreateAsync(new CreatePostParams
{
    Body = "Hello from PostProxy!",
    Profiles = [profiles.Data[0].Id],
});
Console.WriteLine($"{post.Id} {post.Status}");
```

## Usage

### Client

```csharp
using PostProxy;

// Basic
var client = PostProxyClient.Builder("your-api-key").Build();

// With a default profile group (applied to all requests)
var client = PostProxyClient.Builder("your-api-key")
    .ProfileGroupId("pg-abc")
    .Build();

// With a custom base URL
var client = PostProxyClient.Builder("your-api-key")
    .BaseUrl("https://custom.postproxy.dev")
    .Build();
```

### Dependency injection

```csharp
services.AddPostProxy(options =>
{
    options.ApiKey = "your-api-key";
    options.ProfileGroupId = "pg-abc";
});
```

Then inject `PostProxyClient` wherever needed.

### Posts

```csharp
using PostProxy.Models;
using PostProxy.Parameters;

// List posts (paginated)
var page = await client.Posts.ListAsync(new ListPostsParams
{
    Page = 0, PerPage = 10, Status = PostStatus.Draft,
});
Console.WriteLine($"{page.Total} {page.Data.Count}");

// Filter by platform and schedule
var page = await client.Posts.ListAsync(new ListPostsParams
{
    Platforms = [Platform.Instagram, Platform.TikTok],
    ScheduledAfter = DateTimeOffset.Parse("2025-06-01T00:00:00Z"),
});

// Get a single post
var post = await client.Posts.GetAsync("post-id");

// Create a post
var post = await client.Posts.CreateAsync(new CreatePostParams
{
    Body = "Check out our new product!",
    Profiles = ["profile-id-1", "profile-id-2"],
});

// Create a draft
var post = await client.Posts.CreateAsync(new CreatePostParams
{
    Body = "Draft content",
    Profiles = ["profile-id"],
    Draft = true,
});

// Create with media URLs
var post = await client.Posts.CreateAsync(new CreatePostParams
{
    Body = "Photo post",
    Profiles = ["profile-id"],
    Media = ["https://example.com/image.jpg"],
});

// Create with local file uploads
var post = await client.Posts.CreateAsync(new CreatePostParams
{
    Body = "Posted with a local file!",
    Profiles = ["profile-id"],
    MediaFiles = ["./photo.jpg", "./video.mp4"],
});

// Create with platform-specific params
var post = await client.Posts.CreateAsync(new CreatePostParams
{
    Body = "Cross-platform post",
    Profiles = ["ig-profile", "tt-profile"],
    Platforms = new PlatformParams
    {
        Instagram = new InstagramParams
        {
            Format = InstagramFormat.Reel,
            Collaborators = ["@friend"],
        },
        TikTok = new TikTokParams
        {
            PrivacyStatus = TikTokPrivacy.PublicToEveryone,
        },
    },
});

// Schedule a post
var post = await client.Posts.CreateAsync(new CreatePostParams
{
    Body = "Scheduled post",
    Profiles = ["profile-id"],
    ScheduledAt = DateTimeOffset.Parse("2025-12-25T09:00:00Z"),
});

// Create a thread post
var post = await client.Posts.CreateAsync(new CreatePostParams
{
    Body = "Thread starts here",
    Profiles = ["profile-id"],
    Thread =
    [
        new ThreadChildInput { Body = "Second post in the thread" },
        new ThreadChildInput { Body = "Third with media", Media = ["https://example.com/img.jpg"] },
    ],
});
foreach (var child in post.Thread!)
    Console.WriteLine($"{child.Id}: {child.Body}");

// Update a post (only drafts or scheduled posts)
var post = await client.Posts.UpdateAsync("post-id", new UpdatePostParams
{
    Body = "Updated content!",
});

// Update platform params only
var post = await client.Posts.UpdateAsync("post-id", new UpdatePostParams
{
    Platforms = new PlatformParams
    {
        YouTube = new YouTubeParams { PrivacyStatus = "unlisted" },
    },
});

// Replace profiles and media
var post = await client.Posts.UpdateAsync("post-id", new UpdatePostParams
{
    Profiles = ["twitter", "threads"],
    Media = ["https://example.com/new-image.jpg"],
});

// Replace thread children
var post = await client.Posts.UpdateAsync("post-id", new UpdatePostParams
{
    Thread =
    [
        new ThreadChildInput { Body = "Updated first reply" },
        new ThreadChildInput { Body = "Updated second reply", Media = ["https://example.com/img.jpg"] },
    ],
});

// Remove all media
var post = await client.Posts.UpdateAsync("post-id", new UpdatePostParams
{
    Media = [],
});

// Publish a draft
var post = await client.Posts.PublishDraftAsync("post-id");

// Delete a post
var result = await client.Posts.DeleteAsync("post-id");
Console.WriteLine(result.Deleted); // true

// Delete a post and also remove it from social platforms
var result2 = await client.Posts.DeleteAsync("post-id", deleteOnPlatform: true, profileGroupId: null);

// Delete from platforms only (keeps DB record). Defaults to all platforms.
var r1 = await client.Posts.DeleteOnPlatformAsync("post-id");
// Target a single network
var r2 = await client.Posts.DeleteOnPlatformAsync("post-id", new DeleteOnPlatformParams { Network = "twitter" });
// Target a specific profile
var r3 = await client.Posts.DeleteOnPlatformAsync("post-id", new DeleteOnPlatformParams { ProfileId = "prof-abc" });
// Target a specific post profile (covers entire thread for that profile)
var r4 = await client.Posts.DeleteOnPlatformAsync("post-id", new DeleteOnPlatformParams { PostProfileId = "pp-abc" });

// Get post stats
var stats = await client.Posts.StatsAsync(new PostStatsParams
{
    PostIds = ["post-id-1", "post-id-2"],
});

// Filter by profiles/networks and time range
var stats = await client.Posts.StatsAsync(new PostStatsParams
{
    PostIds = ["post-id-1"],
    Profiles = ["instagram", "twitter"],
    From = DateTimeOffset.UtcNow.AddDays(-7),
    To = DateTimeOffset.UtcNow,
});

// Access stats data
foreach (var (postId, postStats) in stats.Data)
{
    foreach (var platform in postStats.Platforms)
    {
        Console.WriteLine($"{platform.Platform}: {platform.Records.Count} snapshots");
        var latest = platform.Records.Last();
        Console.WriteLine($"  impressions: {latest.Stats["impressions"]}");
    }
}
```

### Queues

```csharp
using PostProxy.Models;

// List all queues
var queues = await client.Queues.ListAsync();

// Get a queue
var queue = await client.Queues.GetAsync("queue-id");

// Get next available slot
var nextSlot = await client.Queues.NextSlotAsync("queue-id");
Console.WriteLine(nextSlot.NextSlot);

// Create a queue with timeslots
var queue = await client.Queues.CreateAsync(
    "Morning Posts",
    "pg-abc",
    description: "Weekday morning content",
    timezone: "America/New_York",
    jitter: 10,
    timeslots: new object[]
    {
        new Dictionary<string, object> { ["day"] = 1, ["time"] = "09:00" },
        new Dictionary<string, object> { ["day"] = 2, ["time"] = "09:00" },
        new Dictionary<string, object> { ["day"] = 3, ["time"] = "09:00" },
    });

// Update a queue
var queue = await client.Queues.UpdateAsync("queue-id",
    jitter: 15,
    timeslots: new object[]
    {
        new Dictionary<string, object> { ["day"] = 6, ["time"] = "10:00" },                    // add new timeslot
        new Dictionary<string, object> { ["id"] = 1, ["_destroy"] = true },                    // remove existing timeslot
    });

// Pause/unpause a queue
await client.Queues.UpdateAsync("queue-id", enabled: false);

// Delete a queue
var result = await client.Queues.DeleteAsync("queue-id");

// Add a post to a queue
var post = await client.Posts.CreateAsync(new CreatePostParams
{
    Body = "This post will be scheduled by the queue",
    Profiles = ["profile-id"],
    QueueId = "queue-id",
    QueuePriority = "high",
});
```

### Webhooks

```csharp
// List webhooks
var webhooks = await client.Webhooks.ListAsync();

// Get a webhook
var webhook = await client.Webhooks.GetAsync("wh-id");

// Create a webhook
var webhook = await client.Webhooks.CreateAsync(new CreateWebhookParams
{
    Url = "https://example.com/webhook",
    Events = ["post.published", "post.failed"],
    Description = "My webhook",
});
Console.WriteLine(webhook.Secret);

// Update a webhook
var webhook = await client.Webhooks.UpdateAsync("wh-id", new UpdateWebhookParams
{
    Events = ["post.published"],
    Enabled = false,
});

// Delete a webhook
await client.Webhooks.DeleteAsync("wh-id");

// List deliveries
var deliveries = await client.Webhooks.DeliveriesAsync("wh-id");
foreach (var d in deliveries.Data)
    Console.WriteLine($"{d.EventType}: {d.Success}");
```

#### Signature verification

Verify incoming webhook signatures using HMAC-SHA256:

```csharp
using PostProxy;

var isValid = WebhookSignature.Verify(
    payload: requestBody,
    signatureHeader: request.Headers["X-PostProxy-Signature"],
    secret: "whsec_..."
);
```

#### Event types and typed payloads

Subscribe to any of these events (or pass `["*"]` for all):

`post.processed`, `post.imported`, `platform_post.published`, `platform_post.failed`, `platform_post.failed_waiting_for_retry`, `platform_post.insights`, `profile.connected`, `profile.disconnected`, `profile.stats`, `media.failed`, `comment.created`.

`WebhookEvents.Parse` validates the envelope; `As*` helpers decode `Data` into a typed payload.

```csharp
using PostProxy.Models;
using PostProxy.Webhooks;

var ev = WebhookEvents.Parse(requestBody);
switch (ev.Type)
{
    case WebhookEventType.ProfileStats:
        var stats = WebhookEvents.AsProfileStats(ev);
        Console.WriteLine($"{stats.ProfileId}: {stats.Stats["followerCount"]}");
        break;
    case WebhookEventType.PlatformPostPublished:
        var pp = WebhookEvents.AsPlatformPost(ev);
        Console.WriteLine($"Published: {pp.PlatformId}");
        break;
    case WebhookEventType.CommentCreated:
        var c = WebhookEvents.AsCommentCreated(ev);
        Console.WriteLine($"{c.AuthorUsername}: {c.Body}");
        break;
}
```

### Comments

```csharp
// List comments on a post (paginated)
var comments = await client.Comments.ListAsync("post-id", "profile-id");
foreach (var comment in comments.Data)
{
    Console.WriteLine($"{comment.AuthorUsername}: {comment.Body}");
    foreach (var reply in comment.Replies ?? [])
        Console.WriteLine($"  {reply.AuthorUsername}: {reply.Body}");
}

// List with pagination
var comments = await client.Comments.ListAsync("post-id", "profile-id", page: 2, perPage: 10);

// Get a single comment
var comment = await client.Comments.GetAsync("post-id", "comment-id", "profile-id");

// Create a comment
var comment = await client.Comments.CreateAsync("post-id", "profile-id", "Great post!");

// Reply to a comment
var reply = await client.Comments.CreateAsync("post-id", "profile-id", "Thanks!", parentId: "comment-id");

// Delete a comment
var result = await client.Comments.DeleteAsync("post-id", "comment-id", "profile-id");
Console.WriteLine(result.Accepted); // true

// Hide / unhide a comment
await client.Comments.HideAsync("post-id", "comment-id", "profile-id");
await client.Comments.UnhideAsync("post-id", "comment-id", "profile-id");

// Like / unlike a comment
await client.Comments.LikeAsync("post-id", "comment-id", "profile-id");
await client.Comments.UnlikeAsync("post-id", "comment-id", "profile-id");
```

### Profiles

```csharp
// List all profiles
var profiles = await client.Profiles.ListAsync();

// List profiles in a specific group (overrides client default)
var profiles = await client.Profiles.ListAsync("pg-other");

// Get a single profile
var profile = await client.Profiles.GetAsync("profile-id");
Console.WriteLine($"{profile.Name} {profile.Platform} {profile.Status}");

// Get available placements for a profile
var placements = await client.Profiles.PlacementsAsync("profile-id");
foreach (var p in placements.Data)
    Console.WriteLine($"{p.Id} {p.Name}");

// Delete a profile
var result = await client.Profiles.DeleteAsync("profile-id");
Console.WriteLine(result.Success); // true

// Profile stats timeseries — placement_id required for facebook, linkedin, telegram
var stats = await client.Profiles.GetProfileStatsAsync(
    "prof_li_001",
    placementId: "108520199",
    from: "2026-04-01T00:00:00Z");
foreach (var r in stats.Data.Records)
{
    Console.WriteLine($"{r.RecordedAt}: {r.Stats["followerCount"]}");
}

// Bluesky — no placements
var bsky = await client.Profiles.GetProfileStatsAsync("prof_bsky_001");
var last = bsky.Data.Records[^1];
Console.WriteLine(last.Stats["followersCount"]);
```

### Profile Groups

```csharp
using PostProxy.Models;

// List all groups
var groups = await client.ProfileGroups.ListAsync();

// Get a single group
var group = await client.ProfileGroups.GetAsync("pg-id");
Console.WriteLine($"{group.Name} {group.ProfilesCount}");

// Create a group
var group = await client.ProfileGroups.CreateAsync("My New Group");

// Delete a group (must have no profiles)
var result = await client.ProfileGroups.DeleteAsync("pg-id");
Console.WriteLine(result.Deleted); // true

// Initialize a social platform OAuth connection
var conn = await client.ProfileGroups.InitializeConnectionAsync(
    "pg-id",
    Platform.Instagram,
    "https://yourapp.com/callback");
Console.WriteLine(conn.Url); // Redirect the user to this URL

// BlueSky — app password (synchronous, no OAuth)
var bsky = await client.ProfileGroups.ConnectBlueskyAsync(
    "pg-id", "yourname.bsky.social", "xxxx-xxxx-xxxx-xxxx");
Console.WriteLine(bsky.Profile.Id);

// Telegram — bring-your-own-bot. Channels populate asynchronously; poll
// placements until non-empty.
var tg = await client.ProfileGroups.ConnectTelegramAsync(
    "pg-id", "123456789:ABCdef-GhIJklMnOpQrStUvWxYz");
Console.WriteLine(tg.NextStep);

ListResponse<Placement> placements;
do
{
    placements = await client.Profiles.PlacementsAsync(tg.Profile.Id);
    if (placements.Data.Count == 0) await Task.Delay(3000);
} while (placements.Data.Count == 0);
```

## Error handling

All errors extend `PostProxyException`, which includes the HTTP status code and raw response:

```csharp
using PostProxy.Exceptions;

try
{
    await client.Posts.GetAsync("nonexistent");
}
catch (NotFoundException e)
{
    Console.WriteLine(e.StatusCode);  // 404
    Console.WriteLine(e.Response);    // {error: Not found}
}
catch (PostProxyException e)
{
    Console.WriteLine($"API error {e.StatusCode}: {e.Message}");
}
```

Exception hierarchy:

| Exception | HTTP Status |
|---|---|
| `PostProxyException` | Base class |
| `AuthenticationException` | 401 |
| `BadRequestException` | 400 |
| `NotFoundException` | 404 |
| `ValidationException` | 422 |

## Types

All list methods return a response object with a `Data` list:

```csharp
var profiles = (await client.Profiles.ListAsync()).Data;
var posts = await client.Posts.ListAsync(); // PaginatedResponse also has Total, Page, PerPage
```

Key types:

| Type | Fields |
|---|---|
| `Post` | Id, Body, Status, ScheduledAt, CreatedAt, Media, Thread, Platforms, QueueId, QueuePriority |
| `Profile` | Id, Name, Status, Platform, ProfileGroupId, ExpiresAt, PostCount |
| `ProfileGroup` | Id, Name, ProfilesCount |
| `Media` | Id, Type, Url, Status |
| `ThreadChild` | Id, Body, Media |
| `ThreadChildInput` | Body, Media |
| `Webhook` | Id, Url, Events, Secret, Enabled, Description, CreatedAt |
| `WebhookDelivery` | Id, EventId, EventType, ResponseStatus, AttemptNumber, Success, AttemptedAt, CreatedAt |
| `PlatformResult` | Platform, Status, Params, Error, AttemptedAt, Insights |
| `StatsResponse` | Data (dictionary keyed by post ID) |
| `PostStats` | Platforms |
| `PlatformStats` | ProfileId, Platform, Records |
| `StatsRecord` | Stats (dictionary of metric name to value), RecordedAt |
| `Queue` | Id, Name, Description, Timezone, Enabled, Jitter, ProfileGroupId, Timeslots, PostsCount |
| `Timeslot` | Id, Day, Time |
| `NextSlotResponse` | NextSlot |
| `ListResponse<T>` | Data |
| `Comment` | Id, ExternalId, Body, Status, AuthorUsername, AuthorAvatarUrl, AuthorExternalId, ParentExternalId, LikeCount, IsHidden, Permalink, PlatformData, PostedAt, CreatedAt, Replies |
| `AcceptedResponse` | Accepted |
| `PaginatedResponse<T>` | Data, Total, Page, PerPage |

### Platform parameter types

| Type | Platform |
|---|---|
| `FacebookParams` | Format (`Post`, `Story`), FirstComment, PageId |
| `InstagramParams` | Format (`Post`, `Reel`, `Story`), FirstComment, Collaborators, CoverUrl, AudioName, TrialStrategy, ThumbOffset |
| `TikTokParams` | Format (`Video`, `Image`), PrivacyStatus, PhotoCoverIndex, AutoAddMusic, MadeWithAi, DisableComment, DisableDuet, DisableStitch, BrandContentToggle, BrandOrganicToggle |
| `LinkedInParams` | Format (`Post`), OrganizationId |
| `YouTubeParams` | Format (`Post`), Title, PrivacyStatus, CoverUrl, MadeForKids, Tags, CategoryId, ContainsSyntheticMedia |
| `PinterestParams` | Format (`Pin`), Title, BoardId, DestinationLink, CoverUrl, ThumbOffset |
| `ThreadsParams` | Format (`Post`) |
| `TwitterParams` | Format (`Post`) |
| `BlueskyParams` | Format (`Post`) |
| `TelegramParams` | Format (`Post`), ChatId (required), ParseMode (`Html`, `MarkdownV2`), DisableLinkPreview, DisableNotification |

Supported platforms: `Facebook`, `Instagram`, `TikTok`, `LinkedIn`, `YouTube`, `Twitter`, `Threads`, `Pinterest`, `Bluesky`, `Telegram`. Telegram requires a `ChatId` per post — list channels with `client.Profiles.PlacementsAsync(profileId)`.

Wrap them in `PlatformParams` when passing to `Posts.CreateAsync()`.

## Examples

Run examples from the repo root:

```bash
dotnet run --project examples -p:Example=CreatePost
dotnet run --project examples -p:Example=InitializeConnection
dotnet run --project examples -p:Example=PostStats
dotnet run --project examples -p:Example=ManageQueues
```

Replace the API key and profile group ID in the example files before running.

## Development

```bash
dotnet build
dotnet test
```

## License

MIT
