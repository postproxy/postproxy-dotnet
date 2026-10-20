# Postproxy .NET SDK

.NET client for the [Postproxy API](https://postproxy.dev). Uses C# records for models, `System.Net.Http.HttpClient` for HTTP, and `System.Text.Json` for JSON. No external dependencies beyond Microsoft.Extensions for DI support.

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

#### Idempotency

Every write (`POST`/`PUT`/`PATCH`/`DELETE`) accepts an idempotency key, sent as the
`Idempotency-Key` header. If the connection drops before you see the response, retry with
the same key and you get the original response back instead of a second post:

```csharp
var key = Guid.NewGuid().ToString();

var post = await client.Posts.CreateAsync(new CreatePostParams
{
    Body = "Hello",
    Profiles = ["profile-id"],
    IdempotencyKey = key,
});

// Retrying the same call with the same key replays the original response.
```

Methods that take a parameters record expose it as `IdempotencyKey`; the rest take an
`idempotencyKey` argument:

```csharp
await client.Comments.CreateAsync("post-id", "profile-id", "Nice!", idempotencyKey: key);
await client.Profiles.BackfillPostsAsync("profile-id", "2025-01-01", idempotencyKey: key);
```

Generate a fresh key per logical operation — a GUID is ideal. Keys are scoped to your
account and may be up to 255 characters. The SDK never generates keys or retries for you.

| Situation | Result |
|---|---|
| First request with the key | Runs normally |
| Retry after a success | Original status and body replayed |
| Retry while the first is still running | `ConflictException` (409) — wait and retry |
| Same key, different request body | `ValidationException` (422) |
| Retry after an error response | Runs normally — errors are not replayed |

Only successful (`2xx`) responses are stored, so a request that failed validation or hit a
quota leaves the key free — fix the payload and retry with the same key. Stored responses
are kept for **24 hours**. Requests without a key are unaffected.

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
    Media = ["https://example.com/image.jpg", "https://example.com/second.jpg"],
});

// Publish even if an identical post was just submitted (skips the 409;
// sends X-Skip-Duplicate-Check)
var post = await client.Posts.CreateAsync(new CreatePostParams
{
    Body = "Same text, on purpose",
    Profiles = ["profile-id"],
    SkipDuplicateCheck = true,
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

// Custom cover from a local file — uploaded with the post (the request switches
// to multipart even without MediaFiles). Instagram, TikTok, YouTube, Pinterest.
// A base64 data URL ("data:image/jpeg;base64,...") is sent as-is instead.
var post = await client.Posts.CreateAsync(new CreatePostParams
{
    Body = "Behind the scenes",
    Profiles = ["tt-profile"],
    Media = ["https://example.com/bts.mp4"],
    Platforms = new PlatformParams
    {
        TikTok = new TikTokParams { Format = TikTokFormat.Video, CoverFile = "./cover.jpg" },
    },
});

// Alt text — one string for every item, or one per media item in order
var post = await client.Posts.CreateAsync(new CreatePostParams
{
    Body = "Two photos",
    Profiles = ["ig-profile", "li-profile"],
    Media = ["https://example.com/a.jpg", "https://example.com/b.jpg"],
    Platforms = new PlatformParams
    {
        Instagram = new InstagramParams { AltText = new[] { "A red mug", "A blue mug" }, MadeWithAi = false },
        LinkedIn = new LinkedInParams { AltText = "Two mugs" },
    },
});

// LinkedIn poll
var post = await client.Posts.CreateAsync(new CreatePostParams
{
    Body = "Settle this for us",
    Profiles = ["li-profile"],
    Platforms = new PlatformParams
    {
        LinkedIn = new LinkedInParams
        {
            Format = LinkedInFormat.Poll,
            PollQuestion = "Tabs or spaces?",
            PollOptions = ["Tabs", "Spaces"],
            PollDurationDays = 7,
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

// Update a draft or scheduled post
var post = await client.Posts.UpdateAsync("post-id", new UpdatePostParams
{
    Body = "Updated content!",
});

// Edit a published post. Only Body, YouTube.Title, Pinterest.Title and
// Pinterest.DestinationLink can change; the edit is pushed to the platforms in
// the background (platform_post.edited / platform_post.edit_failed webhooks).
var post = await client.Posts.UpdateAsync("published-post-id", new UpdatePostParams
{
    Body = "Launch day! (now with the right link)",
    Platforms = new PlatformParams
    {
        Pinterest = new PinterestParams { DestinationLink = "https://example.com/launch" },
    },
});
foreach (var p in post.Platforms ?? [])
    Console.WriteLine($"{p.Platform} ({p.ProfileName}): {p.Status} {p.Permalink}");

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

`post.processed`, `post.imported`, `platform_post.published`, `platform_post.failed`, `platform_post.failed_waiting_for_retry`, `platform_post.edited`, `platform_post.edit_failed`, `platform_post.insights`, `profile.connected`, `profile.disconnected`, `profile.stats`, `profile.placements_synced`, `placement.orphaned`, `placement.restored`, `media.failed`, `comment.created`, `comment.failed`, `comment.failed_waiting_for_retry`, `comment.delivery_unconfirmed`, `comment.edited`, `comment.edit_failed`, `profile_comment.created`, `profile_comment.failed`, `profile_comment.failed_waiting_for_retry`, `profile_comment.deleted`, `message.received`, `message.sent`, `message.delivered`, `message.read`, `message.edited`, `message.deleted`, `message.failed_waiting_for_retry`, `message.failed`, `reaction.received`, `referral.received`, `message_template.status_updated`, `message_template.category_updated`, `placement.name_status_updated`, `placement.quality_updated`, `whatsapp.automatic_event`.

`WebhookEvents.Parse` validates the envelope; `As*` helpers decode `Data` into a typed payload:

| Events | Payload | Helper |
|---|---|---|
| `platform_post.*` (incl. `edited`, `edit_failed`) | `PlatformPostData` | `AsPlatformPost` |
| `profile.connected`, `profile.disconnected` | `ProfileEventData` | `AsProfileEvent` |
| `profile.placements_synced` | `PlacementsSyncedData` | `AsPlacementsSynced` |
| `placement.*` | `PlacementEventData` (event-specific fields in `Extra`) | `AsPlacementEvent` |
| `comment.*` | `CommentCreatedData` | `AsCommentCreated` |
| `profile_comment.*` | `ProfileCommentCreatedData` | `AsProfileCommentCreated` |
| the eight `message.*` events | `MessageEventData` | `AsMessageEvent` |
| `reaction.received` | `ReactionEventData` | `AsReactionEvent` |
| `referral.received` | `ReferralEventData` | `AsReferralEvent` |
| `message_template.*` | `MessageTemplateEventData` | `AsMessageTemplateEvent` |
| `whatsapp.automatic_event` | `WhatsAppAutomaticEventData` | `AsWhatsAppAutomaticEvent` |

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
    case WebhookEventType.MessageReceived:
        var m = WebhookEvents.AsMessageEvent(ev);
        Console.WriteLine($"New message: {m.Message.Body}");
        break;
    case WebhookEventType.ReactionReceived:
        var r = WebhookEvents.AsReactionEvent(ev);
        Console.WriteLine($"{r.SenderExternalId} {r.Action}: {r.Emoji}");
        break;
    case WebhookEventType.ProfileCommentCreated:
        var pc = WebhookEvents.AsProfileCommentCreated(ev);
        Console.WriteLine($"Review by {pc.AuthorUsername}: {pc.Body}");
        break;
    case WebhookEventType.PlatformPostEditFailed:
        var ef = WebhookEvents.AsPlatformPost(ev);
        Console.WriteLine($"Edit failed on {ef.Platform}: {ef.Error}");
        break;
    case WebhookEventType.MessageTemplateStatusUpdated:
        var t = WebhookEvents.AsMessageTemplateEvent(ev);
        Console.WriteLine($"Template {t.Name}: {t.Status}");
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

// Filter by when PostProxy received the comment (created_at, not posted_at).
// A bare date means that date's start of day. Applies to top-level comments —
// one in range brings its full Replies list with it.
var recent = await client.Comments.ListAsync(
    "post-id", "profile-id", from: "2026-03-25", to: "2026-03-26T12:00:00Z");

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

// Edit a comment you published (Facebook, YouTube). Runs in the background —
// the outcome arrives as comment.edited / comment.edit_failed.
await client.Comments.EditAsync("post-id", "comment-id", "profile-id", "Fixed a typo");

// Pin / unpin a comment (TikTok)
await client.Comments.PinAsync("post-id", "comment-id", "profile-id");
await client.Comments.UnpinAsync("post-id", "comment-id", "profile-id");

// Comments may carry media attachments and author metadata
foreach (var attachment in comment.Attachments ?? [])
    Console.WriteLine($"{attachment.Type}: {attachment.Url} ({attachment.Status})");
if (comment.Metadata is not null)
    Console.WriteLine($"author signals: {comment.Metadata.Count} fields");

// Privately reply to a comment — returns a direct Message (not a Comment).
// One private reply per comment, within 7 days of it.
var dm = await client.Comments.PrivateReplyAsync("post-id", "comment-id", "profile-id", "DM-ing you the details!");
Console.WriteLine($"Sent DM: {dm.Id}");

// Private replies take QuickReplies, Buttons, Card and Elements as in Messages.SendAsync
await client.Comments.PrivateReplyAsync("post-id", "comment-id", "profile-id", "Here you go:",
    buttons: [MessageButton.WebUrl("Shop", "https://shop.example.com")]);
```

#### Comments across posts

`Comments.ListAllAsync` returns comments spanning every post in the profile group in one
request — the comments counterpart to `Posts.StatsAsync`. Every filter is optional.

**This list is flat.** Unlike the per-post list, replies are not nested: every comment,
top-level or reply, is its own entry linked to its parent by `ParentExternalId`, so `Total`
counts every comment and paging is exact. Entries are `BulkComment`, which adds `PostId`,
`ProfileId`, and `Platform`.

```csharp
var all = await client.Comments.ListAllAsync(
    profiles: ["instagram", "prof-abc"],  // profile IDs or network names, mixed
    postIds: ["post-1", "post-2"],        // omit for every post in scope
    from: "2026-03-25",
    perPage: 50);                         // max 100
// placementId: "page-id" narrows to one page / organization

foreach (var c in all.Data)
{
    // Each entry says where it came from, so you can act on it with the
    // post-scoped methods above.
    Console.WriteLine($"{c.Platform} {c.PostId} {c.ProfileId}: {c.Body}");

    if (c.ParentExternalId is not null)
        Console.WriteLine($"  ↳ reply to {c.ParentExternalId}");
}

// Reply to one of them
var first = all.Data[0];
await client.Comments.CreateAsync(first.PostId, first.ProfileId, "Thanks!", parentId: first.Id);
```

Unknown or out-of-scope IDs in `postIds` and `profiles` are ignored rather than erroring.
Results are ordered newest first by receipt time.

### Direct Messages

Manage private conversations (Chats and Messages) across Facebook, Instagram, Telegram, Bluesky, and WhatsApp.

```csharp
using PostProxy.Models;
using PostProxy.Parameters;

// List chats for a profile (paginated)
var chats = await client.Chats.ListAsync("profile-id", new ListChatsParams { PerPage = 20 });
foreach (var chat in chats.Data)
    Console.WriteLine($"{chat.Id}: {chat.ParticipantName ?? chat.ParticipantExternalId}");

// Create (or look up) a chat with a participant
var newChat = await client.Chats.CreateAsync("profile-id", new CreateChatParams
{
    ParticipantExternalId = "igsid_8675309",
    ParticipantUsername = "jane_doe",
});

// Get a single chat
var chat = await client.Chats.GetAsync("chat-id");

// Archive / unarchive a chat (Bluesky)
await client.Chats.ArchiveAsync("chat-id");
await client.Chats.UnarchiveAsync("chat-id");

// Read receipt and typing indicator (typing: Facebook, Instagram, WhatsApp)
await client.Chats.MarkReadAsync("chat-id");
await client.Chats.StartTypingAsync("chat-id");
await client.Chats.StopTypingAsync("chat-id");

// List messages in a chat (filter by direction/status)
var messages = await client.Messages.ListAsync("chat-id", new ListMessagesParams
{
    Direction = MessageDirection.Inbound,
});
foreach (var msg in messages.Data)
{
    Console.WriteLine($"[{msg.Direction}] {msg.Body}");
    foreach (var reaction in msg.Reactions)
        Console.WriteLine($"  {reaction.Emoji} ({reaction.ReactionType})");
}

// Send a text message
var sent = await client.Messages.SendAsync("chat-id", "Yes, we ship worldwide!");

// Send media (URLs and/or local files), with optional tag / reply / reply markup
await client.Messages.SendAsync("chat-id", new SendMessageParams
{
    Body = "Here's the photo.",
    Media = new[] { "https://cdn.example.com/photo.jpg" },
    MediaFiles = new[] { "/path/to/photo.png" },
    Tag = "HUMAN_AGENT",
});

// Get / edit a message
var message = await client.Messages.GetAsync("message-id");
await client.Messages.EditAsync("message-id", new EditMessageParams { Body = "Updated text" });

// React / unreact (defaults to "love" server-side when no reaction given)
await client.Messages.ReactAsync("message-id", new ReactParams { Reaction = "love", Emoji = "❤️" });
await client.Messages.UnreactAsync("message-id");
```

#### Quick replies and buttons (Facebook & Instagram)

Meta's two interactive primitives. **Quick replies** are chips above the participant's
composer that disappear once tapped; **buttons** are attached to the message and stay in
the thread. Telegram's equivalent is `ReplyMarkup` — passing `QuickReplies` or `Buttons` on
a Telegram or Bluesky chat returns `422`.

```csharp
// Quick replies — up to 13. Title ≤ 20 chars, Payload ≤ 1000.
await client.Messages.SendAsync("chat-id", new SendMessageParams
{
    Body = "What can I help with?",
    QuickReplies = new[]
    {
        new QuickReply { Title = "Track order", Payload = "TRACK" },
        new QuickReply { Title = "Talk to support", Payload = "HELP" },
    },
});

// Buttons — up to 3, each either web_url or postback. Card is optional and
// requires Buttons.
await client.Messages.SendAsync("chat-id", new SendMessageParams
{
    Body = "Your order shipped",
    Buttons = new[]
    {
        MessageButton.WebUrl("Track", "https://shop.example.com/o/123"),
        MessageButton.Postback("Cancel", "CANCEL:123"),
    },
    Card = new MessageCard
    {
        Subtitle = "Arriving Friday",
        ImageUrl = "https://cdn.example.com/shoe.png",
        DefaultAction = CardDefaultAction.WebUrl("https://shop.example.com/o/123"),
    },
});
```

Buttons are delivered as a Meta generic template and your `Body` becomes the template's
element title — so **`Body` is capped at 80 characters when buttons are present**. That is
Meta's limit, not Postproxy's, and a longer body is rejected with a `422` naming the
length. Buttons cannot be combined with media. Instagram is stricter than Messenger: it
delivers quick replies only on a plain-text message, so `QuickReplies` with media or with
`Buttons` returns `422` on Instagram while both are accepted on Facebook.

Validation happens server-side and names the offending index — `buttons[1].url must be an
https:// URL` — surfacing as the SDK's usual exception for a `422`.

> The new parameters are sent on the JSON path only. To combine quick replies with an
> attachment, pass `Media` as a hosted URL rather than uploading via `MediaFiles`.

A tap comes back as an **inbound message** carrying `TappedAction`:

```csharp
var inbound = await client.Messages.ListAsync("chat-id",
    new ListMessagesParams { Direction = MessageDirection.Inbound });
foreach (var msg in inbound.Data)
{
    if (msg.TappedAction is not null)
        // TappedActionKind.QuickReply / Postback / CallbackQuery
        Console.WriteLine($"{msg.TappedAction.Kind}: {msg.TappedAction.Payload}");
}
```

Subscribe to `message.received` to react to taps as they happen — the same field is on the
webhook payload. `TappedAction` is derived rather than stored, so it also resolves for taps
recorded before Postproxy exposed it, including Instagram ice-breaker taps and Telegram
callback queries (`TappedActionKind.CallbackQuery`). A tap also opens the 24h window.

#### Carousels (Facebook & Instagram)

`Elements` sends a horizontally scrolling carousel of up to 10 cards. Each card needs a
`Title` (80 characters or fewer) plus at least one other field, and takes up to 3 buttons.
It can't be combined with `Body`, `Buttons` or `Card`.

```csharp
await client.Messages.SendAsync("chat-id", new SendMessageParams
{
    Elements =
    [
        new MessageElement
        {
            Title = "Red sneaker",
            Subtitle = "$89 — sizes 38-46",
            ImageUrl = "https://cdn.example.com/red.png",
            Buttons = [MessageButton.WebUrl("Buy", "https://shop.example.com/red")],
        },
        new MessageElement
        {
            Title = "Blue sneaker",
            ImageUrl = "https://cdn.example.com/blue.png",
            Buttons = [MessageButton.Postback("Notify me", "NOTIFY:blue")],
        },
    ],
});
```

#### WhatsApp

A WhatsApp chat runs through one of the profile's phone numbers, so `CreateChatParams`
needs a `PlacementId` (the phone number id from `Profiles.PlacementsAsync`). Outside the
24-hour customer service window (`Chat.WithinMessagingWindow`) only a template gets
through. Send exactly one of `Body`, `Media`, `Template`, `Interactive`, `Location` or
`Contacts`.

```csharp
var numbers = await client.Profiles.PlacementsAsync("whatsapp-profile-id");
var chat = await client.Chats.CreateAsync("whatsapp-profile-id", new CreateChatParams
{
    ParticipantExternalId = "15551234567",
    PlacementId = numbers.Data[0].Id,
});

// Template — by Id, or by Name (+ Language when it exists in several)
await client.Messages.SendAsync(chat.Id, new SendMessageParams
{
    Template = new MessageTemplateInput
    {
        Name = "order_shipped",
        Language = "en_US",
        Variables = ["Jane", "#1234"],   // header, then body, then URL buttons
    },
});

// Inside the window: location, contacts, interactive (Meta's objects, sent as-is)
await client.Messages.SendAsync(chat.Id, new SendMessageParams
{
    Location = new MessageLocation { Latitude = 52.52, Longitude = 13.405, Name = "Pickup point" },
});
await client.Messages.SendAsync(chat.Id, new SendMessageParams
{
    Interactive = new { type = "button", body = new { text = "Pick one" }, action = new { buttons = new[] { new { type = "reply", reply = new { id = "a", title = "A" } } } } },
});

// Send options: Category ("utility"), LinkPreview, VoiceNote, Filename
await client.Messages.SendAsync(chat.Id, new SendMessageParams
{
    Media = ["https://files.example.com/invoice.pdf"],
    Filename = "invoice.pdf",
});
```

Inbound WhatsApp messages carry `Location`, `Contacts`, `Interactive` and, for template
sends, `Template`. Download an inbound attachment with `client.WhatsApp.DownloadMediaAsync`
(see [WhatsApp numbers](#whatsapp-numbers)).

### WhatsApp templates

Templates live on the WhatsApp Business account (the profile) and are addressed by their
PostProxy id, Meta's id, or their name — pass `language` with a name that exists in
several languages.

```csharp
// Create — Meta reviews it before it can be sent
var template = await client.WhatsAppTemplates.CreateAsync("profile-id", new CreateWhatsAppTemplateParams
{
    Name = "order_shipped",
    Category = "UTILITY",
    Language = "en_US",
    Components = [new { type = "BODY", text = "Hi {{1}}, your order {{2}} has shipped." }],
});
Console.WriteLine(template.Status); // PENDING — follow message_template.status_updated

// List (Refresh re-reads from Meta first), get, update, delete
var approved = await client.WhatsAppTemplates.ListAsync("profile-id", status: "APPROVED", refresh: true);
var one = await client.WhatsAppTemplates.GetAsync("profile-id", "order_shipped", language: "en_US");
await client.WhatsAppTemplates.UpdateAsync("profile-id", one.Id,
    new UpdateWhatsAppTemplateParams { MessageSendTtlSeconds = 3600 });
await client.WhatsAppTemplates.DeleteAsync("profile-id", "order_shipped"); // every language

// Meta's ready-made template library
var library = await client.WhatsAppTemplates.LibraryAsync("profile-id", name: "delivery");
```

### WhatsApp numbers

`client.WhatsApp` administers a profile's phone numbers. `phoneNumberId` is the placement id
from `Profiles.PlacementsAsync`; dataset and conversion calls are account-level and don't
take one. Responses are Meta's payloads, returned as `JsonElement`.

```csharp
var wa = client.WhatsApp;

// Registration and verification
await wa.GetNumberInfoAsync("profile-id", "pn-id");
await wa.RequestVerificationCodeAsync("profile-id", "pn-id", method: "SMS", language: "en");
await wa.VerifyPhoneNumberAsync("profile-id", "pn-id", "123456");
await wa.RegisterPhoneNumberAsync("profile-id", "pn-id", pin: "000000");

// Business profile, display name, username
await wa.UpdateBusinessProfileAsync("profile-id", "pn-id", about: "Fast replies", websites: ["https://example.com"]);
await wa.UpdateBusinessProfilePhotoAsync("profile-id", "pn-id", url: "https://example.com/logo.png");
await wa.RequestDisplayNameChangeAsync("profile-id", "pn-id", "Acme Coffee"); // placement.name_status_updated
await wa.SetUsernameAsync("profile-id", "pn-id", "acmecoffee");

// Blocked users
await wa.BlockUsersAsync("profile-id", "pn-id", ["15557654321"]);
var blocked = await wa.ListBlockedUsersAsync("profile-id", "pn-id", limit: 50);

// Groups
var group = await wa.CreateGroupAsync("profile-id", "pn-id", "VIP customers", joinApprovalMode: "approval_required");
var groupId = group.GetProperty("id").ToString();
await wa.CreateGroupInviteLinkAsync("profile-id", "pn-id", groupId);
await wa.ApproveGroupJoinRequestsAsync("profile-id", "pn-id", groupId, ["15551234567"]);

// Conversions for click-to-WhatsApp ads
await wa.SendConversionEventAsync("profile-id", new WhatsAppConversionEvent
{
    EventName = "Purchase",
    EventId = "order-1234",
    ChatId = "chat-id",
    Currency = "USD",
    Value = 24.5m,
});

// Download an inbound media file — raw bytes plus content type
var media = await wa.DownloadMediaAsync("profile-id", "pn-id", attachment.ExternalId!);
await File.WriteAllBytesAsync("photo.jpg", media.Data);
```

### Google Business locations

`client.GoogleBusiness` manages a location's own data — hours, attributes, services, menus,
action links and photos — separate from posting to it. `locationId` is the placement id from
`Profiles.PlacementsAsync`. Google's payloads are camelCased and passed through untouched in
both directions (responses are `JsonElement`), so you can PATCH back exactly what you GET.
Updates take a `fields` list naming the top-level fields to change (Google's update mask).

```csharp
var gbp = client.GoogleBusiness;

var location = await gbp.GetLocationAsync("profile-id", "location-id", readMask: "title,websiteUri");
await gbp.UpdateLocationAsync("profile-id", "location-id", ["websiteUri"],
    new Dictionary<string, object?> { ["websiteUri"] = "https://example.com" });

await gbp.UpdateHoursAsync("profile-id", "location-id",
    fields: ["regularHours"],
    regularHours: new
    {
        periods = new[] { new { openDay = "MONDAY", openTime = new { hours = 9 }, closeDay = "MONDAY", closeTime = new { hours = 17 } } },
    });

// Attributes, services, food menus
var available = await gbp.ListAvailableAttributesAsync("profile-id", "location-id", showAll: true);
await gbp.UpdateAttributesAsync("profile-id", "location-id",
    [new { name = "attributes/has_wifi", values = new[] { true } }], attributeMask: ["attributes/has_wifi"]);

// Booking / ordering links
await gbp.CreatePlaceActionLinkAsync("profile-id", "location-id", "https://book.example.com", "APPOINTMENT", isPreferred: true);

// Photos
await gbp.CreateMediaAsync("profile-id", "location-id", "https://example.com/storefront.jpg",
    category: "EXTERIOR", mediaFormat: "PHOTO");
var media = await gbp.ListMediaAsync("profile-id", "location-id");
```

All 19 methods: `GetLocationAsync`, `UpdateLocationAsync`, `ListAvailableLocationCategoriesAsync`,
`UpdateHoursAsync`, `GetAttributesAsync`, `ListAvailableAttributesAsync`, `UpdateAttributesAsync`,
`GetServiceListAsync`, `UpdateServiceListAsync`, `GetFoodMenusAsync`, `UpdateFoodMenusAsync`,
`ListPlaceActionLinksAsync`, `ListAvailablePlaceActionTypesAsync`, `CreatePlaceActionLinkAsync`,
`UpdatePlaceActionLinkAsync`, `DeletePlaceActionLinkAsync`, `ListMediaAsync`, `CreateMediaAsync`,
`DeleteMediaAsync`.

### Profile comments (Google Business reviews)

Profile-level comments expose Google Business reviews and replies. Reviews are user-generated — the SDK lets you list/get them and reply to or delete your own replies. Reviews sync twice daily.

```csharp
// List reviews for a profile (paginated)
var reviews = await client.ProfileComments.ListAsync("profile-id");
foreach (var review in reviews.Data)
{
    Console.WriteLine($"{review.AuthorUsername}: {review.Body}");
    foreach (var reply in review.Replies ?? [])
        Console.WriteLine($"  reply: {reply.Body}");
}

// Filter by placement (location)
var reviews = await client.ProfileComments.ListAsync(
    "profile-id",
    placementId: "accounts/123/locations/456");

// Get a single review
var review = await client.ProfileComments.GetAsync("profile-id", "review-id");

// Reply to a review (parentId is the review id)
var reply = await client.ProfileComments.CreateAsync("profile-id", "review-id", "Thanks for visiting!");

// Delete your reply
await client.ProfileComments.DeleteAsync("profile-id", "reply-id");
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

// Move a placement (e.g. a Facebook Page or Telegram channel) to another group
var placement = await client.Profiles.AssignPlacementToGroupAsync(
    "profile-id", "placement-external-id", "pg-other");
Console.WriteLine(placement.ProfileGroupId); // "pg-other"

// Move the whole profile (with its placements) to another group
await client.Profiles.AssignToGroupAsync("profile-id", "pg-other");

// Instagram: whether a DM participant is verified and follows you
var status = await client.Profiles.GetParticipantStatusAsync("profile-id", "igsid");

// LinkedIn: resolve a company page to the URN used to @-mention it
var mention = await client.Profiles.ResolveMentionAsync("profile-id", "https://www.linkedin.com/company/acme");
Console.WriteLine(mention.Urn);

// Instagram reel music (profiles connected via Facebook login) — use with InstagramParams.AudioId
var audio = await client.Profiles.GetInstagramAvailableAudioAsync("profile-id", InstagramAudioType.Music, "summer");

// Facebook Page about / contact fields
var page = await client.Profiles.GetFacebookPageAsync("profile-id", "page-id", fields: "about,website");
await client.Profiles.UpdateFacebookPageAsync("profile-id", "page-id",
    new Dictionary<string, object?> { ["about"] = "Fresh coffee since 1999" });

// Ice breakers (Instagram DMs): FAQ prompts shown when a user opens a chat
var iceBreakers = await client.Profiles.GetIceBreakersAsync("profile-id");
foreach (var ib in iceBreakers.IceBreakers)
    Console.WriteLine(ib.Question);

await client.Profiles.SetIceBreakersAsync("profile-id",
[
    new IceBreaker { Question = "What services do you offer?", Payload = "services" },
    new IceBreaker { Question = "What are your hours?", Payload = "hours" },
]); // 1-4 items

await client.Profiles.DeleteIceBreakersAsync("profile-id");

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

// Per-day series. Publish attributes post metrics to the day the post was
// published instead of the day they were received.
var daily = await client.Profiles.GetDailyStatsAsync(
    "profile-id", from: "2026-09-01", to: "2026-09-30", attribution: DailyStatsAttribution.Publish);
foreach (var day in daily.Data.Records)
    Console.WriteLine($"{day.Date}: {day.Metrics["impressions"]}");

// Followers gained / lost per day
var followers = await client.Profiles.GetFollowerHistoryAsync("profile-id", from: "2026-09-01");
```

Every stats record (post stats and profile stats alike) carries `RawStats` alongside the
normalized `Stats`, exposing each metric under its **original platform name**:

```csharp
var stats = await client.Posts.StatsAsync(new PostStatsParams { PostIds = ["post-id"] });
var record = stats.Data["post-id"].Platforms[0].Records[0];

Console.WriteLine(record.Stats["impressions"]);              // normalized
Console.WriteLine(record.RawStats?["views"]);                // Instagram's own name
Console.WriteLine(record.RawStats?["impression_count"]);     // Twitter/X's own name
```

LinkedIn post stats now normalize `likes`, `comments`, `shares`, and `clicks` alongside
`impressions` — previously only `impressions` was normalized.

#### Post syncs & backfill

Postproxy mirrors posts published natively on a platform into your account. Every one of
those pulls is recorded as a **post sync**: the one fired when the profile connects, the
recurring poll, and any backfill you start.

```csharp
// Start a backfill — walks the feed backwards from the newest post in batches
// of 25 until it reaches `from` or the platform stops returning posts.
var sync = await client.Profiles.BackfillPostsAsync("profile-id", "2025-01-01");
Console.WriteLine($"{sync.Id} {sync.Status}"); // "sync456def Pending"

// Poll it to completion — finished when Status is Completed or Failed
var run = await client.Profiles.GetPostSyncAsync("profile-id", sync.Id);
Console.WriteLine($"{run.PostsImported} of {run.PostsSeen}, back to {run.OldestPostedAt}");

// List recent runs (kept for 30 days), newest first
var runs = await client.Profiles.ListPostSyncsAsync(
    "profile-id",
    trigger: PostSyncTrigger.Backfill,   // Connect | Scheduled | Backfill
    status: PostSyncStatus.Completed,    // Pending | Running | Completed | Failed
    perPage: 25);
```

| `PostSync` property | Description |
|---|---|
| `Id` | Sync identifier |
| `ProfileId` | Profile this run belongs to |
| `Kind` | Always `posts` today |
| `Trigger` | `Connect`, `Scheduled`, or `Backfill` |
| `Status` | `Pending`, `Running`, `Completed`, or `Failed` |
| `StartedAt` / `CompletedAt` | `DateTimeOffset?` |
| `PostsSeen` | Posts the platform returned across the run |
| `PostsImported` | Posts that were **new** and got created |
| `BackfillFrom` | The date floor requested; null for `Connect`/`Scheduled` |
| `OldestPostedAt` | Publish date of the oldest post the run reached |
| `Error` | Platform error message when `Status` is `Failed` |
| `CreatedAt` | `DateTimeOffset` |

**How far back a backfill reaches depends on the platform's API**, not on Postproxy: where
history is pageable we follow it, otherwise the run ends early with whatever it got and
still reports `PostSyncStatus.Completed`.

Only one backfill runs per profile at a time — starting a second throws `ConflictException`
carrying the running one's id:

```csharp
try
{
    await client.Profiles.BackfillPostsAsync("profile-id", "2025-01-01");
}
catch (ConflictException e)
{
    var runningId = e.Response!["profile_sync_id"].ToString();
    // Poll the run that's already going.
}
```

DM history has the same shape: `BackfillChatsAsync(id, from)` starts a chat backfill, and
`ListChatSyncsAsync` / `GetChatSyncAsync` return `ChatSync` runs (`ChatsSeen`,
`MessagesImported`, `OldestUpdatedAt`).

```csharp
var chatSync = await client.Profiles.BackfillChatsAsync("profile-id", "2026-01-01T00:00:00Z");
var chatRuns = await client.Profiles.ListChatSyncsAsync("profile-id", status: PostSyncStatus.Running);
```

Posts you already have are skipped, so overlapping backfills are safe. Imported posts
behave exactly like ones the poll picks up (`source: "imported"`, `post.imported` webhook),
but a backfill's follow-up work is queued at a lower priority so a deep run can't slow down
publishing.

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

// Pick the flow: Instagram via Facebook login (needed for reel music), or
// WhatsApp via the Cloud API (Api) / the WhatsApp Business app (BusinessApp).
// refreshConnection: true re-authorizes an existing profile.
var wa = await client.ProfileGroups.InitializeConnectionAsync(
    "pg-id", Platform.WhatsApp, "https://yourapp.com/callback",
    onboarding: ConnectionOnboarding.BusinessApp);

// BlueSky — app password (synchronous, no OAuth)
var bsky = await client.ProfileGroups.ConnectBlueskyAsync(
    "pg-id", "yourname.bsky.social", "xxxx-xxxx-xxxx-xxxx");
Console.WriteLine(bsky.Profile.Id);

// Placements across the whole account (account-wide API key). When a placement
// is reachable through several profiles, choose which one is active.
var all = await client.Placements.ListAsync(perPage: 50);
await client.Placements.SetActiveProfileAsync(all.Data[0].Id, "profile-id");

// Profile-group-scoped API keys (account-wide API key). Token is shown only once.
var key = await client.ApiKeys.CreateAsync("CI", "pg-id");
Console.WriteLine(key.Token);
var keys = await client.ApiKeys.ListAsync();
await client.ApiKeys.DeleteAsync(key.Id);

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
| `ForbiddenException` | 403 — account canceled or past due, or the platform refused the action |
| `NotFoundException` | 404 |
| `ConflictException` | 409 — duplicate submission (`Response["duplicate_post_id"]`), a backfill already running (`Response["profile_sync_id"]`), or an in-flight `Idempotency-Key` |
| `ValidationException` | 422 |
| `RateLimitException` | 429 — `RetryAfter` holds the seconds from the `Retry-After` header (or null) |

```csharp
catch (RateLimitException e) when (e.RetryAfter is int seconds)
{
    await Task.Delay(TimeSpan.FromSeconds(seconds));
}
```

## Types

All list methods return a response object with a `Data` list:

```csharp
var profiles = (await client.Profiles.ListAsync()).Data;
var posts = await client.Posts.ListAsync(); // PaginatedResponse also has Total, Page, PerPage
```

Key types:

| Type | Fields |
|---|---|
| `Post` | Id, Body, Status, ScheduledAt, CreatedAt, Draft, Source, Media, Thread, Platforms, QueueId, QueuePriority |
| `Profile` | Id, Name, Username, Status, InactiveReason, Platform, PlatformId, ProfileGroupId, ExpiresAt, PostCount, AvatarUrl, PlatformUrl, PlacementsSyncStatus |
| `Placement` | Id, Name, AvatarUrl, Metadata |
| `AccountPlacement` | Id, Platform, ExternalId, Name, AvatarUrl, Status, ActiveProfile, LinkedProfiles — returned by `Placements.ListAsync` |
| `ApiKey` | Id, Name, ProfileGroupId, Sandbox, Token (create only), CreatedAt |
| `ProfileGroup` | Id, Name, ProfilesCount |
| `Media` | Id, Type, Url, Status |
| `ThreadChild` | Id, Body, Media, Platforms |
| `ThreadChildInput` | Body, Media |
| `Webhook` | Id, Url, Events, Secret, Enabled, Description, CreatedAt |
| `WebhookDelivery` | Id, EventId, EventType, ResponseStatus, AttemptNumber, Success, AttemptedAt, CreatedAt |
| `PlatformResult` | Platform, ProfileId, ProfileName, Status, Permalink, Params, Error, ErrorDetails, AttemptedAt, Insights, Attachments |
| `StatsResponse` | Data (dictionary keyed by post ID) |
| `PostStats` | Platforms |
| `PlatformStats` | ProfileId, Platform, Records |
| `StatsRecord` | Stats (dictionary of metric name to value), RawStats (metrics under their platform-native names), RecordedAt |
| `Queue` | Id, Name, Description, Timezone, Enabled, Jitter, ProfileGroupId, Timeslots, PostsCount |
| `Timeslot` | Id, Day, Time |
| `NextSlotResponse` | NextSlot |
| `ListResponse<T>` | Data |
| `Comment` | Id, ExternalId, Body, Status, AuthorUsername, AuthorAvatarUrl, AuthorExternalId, ParentExternalId, LikeCount, IsHidden, Permalink, PlatformData, ExternalEditedAt, PostedAt, CreatedAt, Replies |
| `BulkComment` | Every `Comment` property except `Replies`, plus PostId, ProfileId, Platform — returned by `Comments.ListAllAsync` |
| `PostSync` | Id, ProfileId, PlacementId, Kind, Trigger, Status, StartedAt, CompletedAt, PostsSeen, PostsImported, BackfillFrom, OldestPostedAt, Error, CreatedAt |
| `ChatSync` | Id, ProfileId, PlacementId, Kind, Trigger, Status, StartedAt, CompletedAt, ChatsSeen, MessagesImported, BackfillFrom, OldestUpdatedAt, Error, CreatedAt |
| `Chat` | Id, ProfileId, Platform, ExternalPlacementId, Group, Participant*, LastInboundAt, LastOutboundAt, LastMessageAt, Metadata, Archived, WithinMessagingWindow, CreatedAt |
| `Message` | … plus Elements, StoryReply, Interactive, Location, Contacts, Template, ErrorDetails |
| `DailyStatsResponse` / `FollowerHistoryResponse` | Data with Records — returned by `GetDailyStatsAsync` / `GetFollowerHistoryAsync` |
| `WhatsAppTemplate` | Id, ExternalId, ProfileId, Name, Language, Category, Status, ParameterFormat, Components, MessageSendTtlSeconds, RejectedReason, QualityScore, VariableCount, CreatedAt, UpdatedAt |
| `AcceptedResponse` | Accepted |
| `PaginatedResponse<T>` | Data, Total, Page, PerPage |

### Platform parameter types

| Type | Platform |
|---|---|
| `FacebookParams` | Format (`Post`, `Story`), FirstComment, PageId, Link, AltText, MadeWithAi, Countries |
| `InstagramParams` | Format (`Post`, `Reel`, `Story`), FirstComment, Collaborators, CoverUrl, CoverFile, AudioName, AudioId, AudioVolume, VideoVolume, StripAudio, TrialStrategy (`Manual`, `SsPerformance`), ThumbOffset, UserTags, AltText, MadeWithAi |
| `InstagramUserTag` | Username, X, Y, MediaIndex |
| `TikTokParams` | Format (`Video`, `Image`), PrivacyStatus, PhotoCoverIndex, AutoAddMusic, MadeWithAi, DisableComment, DisableDuet, DisableStitch, BrandContentToggle, BrandOrganicToggle, UploadAsDraft, CoverUrl, CoverFile, ThumbOffset |
| `LinkedInParams` | Format (`Post`, `Poll`), OrganizationId, ReshareUrn, AltText, Countries, PollQuestion, PollOptions (2-4, max 30 chars), PollDurationDays (1, 3, 7, 14) |
| `YouTubeParams` | Format (`Post`), Title, PrivacyStatus, CoverUrl, CoverFile, MadeForKids, Tags, CategoryId, ContainsSyntheticMedia |
| `PinterestParams` | Format (`Pin`), Title, BoardId, DestinationLink, CoverUrl, CoverFile, ThumbOffset, AltText |
| `ThreadsParams` | Format (`Post`), AltText, TopicTag |
| `TwitterParams` | Format (`Post`, `Poll`), AltText, PollOptions (2-4 choices, max 25 chars each; required for `Poll`), PollDurationMinutes (5-10080; required for `Poll`) |
| `BlueskyParams` | Format (`Post`), AltText |
| `TelegramParams` | Format (`Post`), ChatId (required), ParseMode (`Html`, `MarkdownV2`), DisableLinkPreview, DisableNotification |

`AltText` takes a `string` (one text for every media item) or a `string[]` (one per item, in order). `CoverFile` takes a local path, uploaded with the post, or a base64 data URL sent as-is.

Supported platforms: `Facebook`, `Instagram`, `TikTok`, `LinkedIn`, `YouTube`, `Twitter`, `Threads`, `Pinterest`, `Bluesky`, `Telegram`, `GoogleBusiness`, `WhatsApp`. Telegram requires a `ChatId` per post — list channels with `client.Profiles.PlacementsAsync(profileId)`.

#### Google Business

Google Business posts use the `GoogleBusiness` property on `PlatformParams`, a `Dictionary<string, object>`. The `location_id` is the location resource path returned by `client.Profiles.PlacementsAsync()`. Supported formats: `standard`, `event`, `offer`. CTA actions: `LEARN_MORE`, `BOOK`, `ORDER`, `SHOP`, `SIGN_UP`, `CALL`. Media is limited to one image (≤5 MB).

```csharp
var platforms = new PlatformParams
{
    GoogleBusiness = new Dictionary<string, object>
    {
        ["format"] = "standard",
        ["location_id"] = "accounts/123/locations/456",
        ["cta_action_type"] = "LEARN_MORE",
        ["cta_url"] = "https://example.com",
    },
};
```

#### Instagram user tags

Tag public Instagram accounts in a post — feed post, reel, or story:

```csharp
var platforms = new PlatformParams
{
    Instagram = new InstagramParams
    {
        Format = InstagramFormat.Post,
        UserTags =
        [
            new InstagramUserTag { Username = "natgeo", X = 0.5, Y = 0.4 },               // slide 0
            new InstagramUserTag { Username = "nasa", X = 0.2, Y = 0.8, MediaIndex = 1 }, // slide 1
            new InstagramUserTag { Username = "spacex", MediaIndex = 2 },                 // video — username only
        ],
    },
};
```

- **Images require `X` and `Y`** — floats `0.0`–`1.0` measured from the top-left corner.
- **Reels and video slides** are tagged by username only; coordinates are ignored and dropped.
- **Stories** accept coordinates but don't need them.
- `MediaIndex` picks the carousel slide (0-based, defaults to `0`, video slides included).
- A leading `@` on a username is stripped for you.

Coordinates outside `0.0`–`1.0`, a `MediaIndex` past the last media item, or an image tag
missing `X`/`Y` are rejected with a `ValidationException` naming the offending entry.
Accounts that are private or have tagging turned off are silently skipped by Instagram at
publish time.

Wrap them in `PlatformParams` when passing to `Posts.CreateAsync()`.

## Examples

Run examples from the repo root:

```bash
dotnet run --project examples -p:Example=CreatePost
dotnet run --project examples -p:Example=InitializeConnection
dotnet run --project examples -p:Example=PostStats
dotnet run --project examples -p:Example=ManageQueues
dotnet run --project examples -p:Example=EditPublishedPost
dotnet run --project examples -p:Example=DmCarousel
dotnet run --project examples -p:Example=GoogleBusiness
dotnet run --project examples -p:Example=WhatsAppTemplates
dotnet run --project examples -p:Example=WhatsAppGroups
```

Replace the API key and profile group ID in the example files before running.

## Development

```bash
dotnet build
dotnet test
```

## License

MIT
