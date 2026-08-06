using PostProxy;

var client = PostProxyClient.Builder(Environment.GetEnvironmentVariable("POSTPROXY_API_KEY")!)
    .ProfileGroupId(Environment.GetEnvironmentVariable("POSTPROXY_PROFILE_GROUP_ID")!)
    .Build();

var postId = "your-post-id";
var profileId = "your-profile-id";

// List comments on a post
var comments = await client.Comments.ListAsync(postId, profileId);
Console.WriteLine($"Total comments: {comments.Total}");
foreach (var comment in comments.Data)
{
    Console.WriteLine($"  {comment.AuthorUsername}: {comment.Body}");
    if (comment.Attachments is not null)
    {
        foreach (var attachment in comment.Attachments)
            Console.WriteLine($"    attachment: {attachment.Type} {attachment.Url}");
    }
    if (comment.Metadata is not null && comment.Metadata.TryGetValue("follower_count", out var followers))
        Console.WriteLine($"    author followers: {followers}");
    if (comment.Replies is not null)
    {
        foreach (var reply in comment.Replies)
        {
            Console.WriteLine($"    {reply.AuthorUsername}: {reply.Body}");
        }
    }
}

// Filter the per-post list by when PostProxy received the comment
var recent = await client.Comments.ListAsync(postId, profileId, from: "2026-03-25", to: "2026-03-26");
Console.WriteLine($"Comments received 2026-03-25..26: {recent.Total}");

// List comments across every post in the profile group. Flat: replies come back
// as their own entries linked by ParentExternalId.
var across = await client.Comments.ListAllAsync(profiles: ["instagram"], from: "2026-03-25", perPage: 50);
Console.WriteLine($"Comments across posts: {across.Total}");
foreach (var c in across.Data)
{
    var kind = c.ParentExternalId is null ? "comment" : "reply";
    Console.WriteLine($"  [{c.Platform}] {kind} on post {c.PostId} — {c.AuthorUsername}: {c.Body}");
}

// Create a comment. An idempotency key makes the write safe to retry after a
// dropped connection — the retry replays the original response instead of
// posting a second comment.
var newComment = await client.Comments.CreateAsync(
    postId, profileId, "Thanks for the feedback!", idempotencyKey: Guid.NewGuid().ToString());
Console.WriteLine($"Created: {newComment.Id} (status: {newComment.Status})");

// Reply to a comment
var replyComment = await client.Comments.CreateAsync(postId, profileId, "Glad you liked it!", parentId: newComment.Id);
Console.WriteLine($"Reply: {replyComment.Id}");

// Hide / unhide
await client.Comments.HideAsync(postId, newComment.Id, profileId);
Console.WriteLine("Comment hidden");

await client.Comments.UnhideAsync(postId, newComment.Id, profileId);
Console.WriteLine("Comment unhidden");

// Like / unlike
await client.Comments.LikeAsync(postId, newComment.Id, profileId);
Console.WriteLine("Comment liked");

await client.Comments.UnlikeAsync(postId, newComment.Id, profileId);
Console.WriteLine("Comment unliked");

// Privately reply to a comment (returns a direct Message, not a Comment)
var privateReply = await client.Comments.PrivateReplyAsync(postId, newComment.Id, profileId, "DM-ing you the details!");
Console.WriteLine($"Private reply sent as message: {privateReply.Id}");

// Delete
await client.Comments.DeleteAsync(postId, newComment.Id, profileId);
Console.WriteLine("Comment deleted");
