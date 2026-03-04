using PostProxy;
using PostProxy.Models;
using PostProxy.Parameters;

var client = PostProxyClient.Builder("your-api-key")
    .ProfileGroupId("your-profile-group-id")
    .Build();

// List profiles
var profiles = await client.Profiles.ListAsync();
Console.WriteLine($"Profiles: {string.Join(", ", profiles.Data.Select(p => p.Name))}");


// Create a post with media URLs
var postWithMedia = await client.Posts.CreateAsync(new CreatePostParams
{
    Body = "Check out this image!",
    Profiles = ["instagram"],
    Media = ["https://example.com/image.jpg"],
    Draft = true,
});
Console.WriteLine($"Post with media: {postWithMedia.Id}");

var tiktokProfile = profiles.Data.FirstOrDefault(p => p.Platform == Platform.TikTok);
var instagramProfile = profiles.Data.FirstOrDefault(p => p.Platform == Platform.Instagram);

// Create a post with platform-specific params
var postWithParams = await client.Posts.CreateAsync(new CreatePostParams
{
    Body = "Cross-platform post!",
    Profiles = [tiktokProfile.Id, instagramProfile.Id],
    Media = ["https://example.com/image.jpg"],
    Draft = true,
    Platforms = new PlatformParams
    {
        Instagram = new InstagramParams
        {
            Format = InstagramFormat.Post,
            Collaborators = ["friend_username"],
        },
        TikTok = new TikTokParams
        {
            Format = TikTokFormat.Image,
            PrivacyStatus = TikTokPrivacy.PublicToEveryone,
            AutoAddMusic = true,
        },
    },
});
Console.WriteLine($"Post with platform params: {postWithParams.Id}");

// Create a scheduled post
var scheduledPost = await client.Posts.CreateAsync(new CreatePostParams
{
    Body = "Scheduled post",
    Profiles = [profileId],
    ScheduledAt = new DateTimeOffset(2025, 12, 25, 9, 0, 0, TimeSpan.Zero),
});
Console.WriteLine($"Scheduled post: {scheduledPost.Id}");

// // Publish a draft
var published = await client.Posts.PublishDraftAsync(post.Id);
Console.WriteLine($"Published: {published.Id} ({published.Status})");

// Create a thread post
var threadPost = await client.Posts.CreateAsync(new CreatePostParams
{
    Body = "Here's a thread about PostProxy 🧵",
    Profiles = [profileId],
    Thread =
    [
        new ThreadChildInput { Body = "First, connect your social accounts." },
        new ThreadChildInput { Body = "Then, create posts with media!", Media = ["https://example.com/demo.jpg"] },
        new ThreadChildInput { Body = "Finally, schedule or publish instantly." },
    ],
});
Console.WriteLine($"Thread post: {threadPost.Id} ({threadPost.Thread?.Count} children)");

// List posts with filters
var postList = await client.Posts.ListAsync(new ListPostsParams
{
    Page = 0,
    PerPage = 10,
});
Console.WriteLine($"Posts: {postList.Total} total");

// Delete a post
var deleted = await client.Posts.DeleteAsync(post.Id);
Console.WriteLine($"Deleted: {deleted.Deleted}");
