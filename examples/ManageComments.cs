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
    if (comment.Replies is not null)
    {
        foreach (var reply in comment.Replies)
        {
            Console.WriteLine($"    {reply.AuthorUsername}: {reply.Body}");
        }
    }
}

// Create a comment
var newComment = await client.Comments.CreateAsync(postId, profileId, "Thanks for the feedback!");
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

// Delete
await client.Comments.DeleteAsync(postId, newComment.Id, profileId);
Console.WriteLine("Comment deleted");
