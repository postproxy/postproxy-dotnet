using PostProxy;
using PostProxy.Models;
using PostProxy.Parameters;

var client = PostProxyClient.Builder(Environment.GetEnvironmentVariable("POSTPROXY_API_KEY")!)
    .Build();

var profileId = "your-youtube-profile-id";

// List the channel's playlists, page by page
string? pageToken = null;
do
{
    var page = await client.Profiles.ListYouTubePlaylistsAsync(profileId, pageToken);
    foreach (var p in page.Data)
        Console.WriteLine($"{p.Id}: {p.Title} ({p.PrivacyStatus}, {p.ItemCount} videos)");
    pageToken = page.NextPageToken;
} while (pageToken is not null);

// Create a playlist and publish a video into it
var playlist = await client.Profiles.CreateYouTubePlaylistAsync(profileId, "Tutorials",
    description: "Step-by-step guides",
    privacyStatus: YouTubePrivacy.Public);

var post = await client.Posts.CreateAsync(new CreatePostParams
{
    Body = "How to set up your account",
    Profiles = [profileId],
    Media = ["https://example.com/tutorial.mp4"],
    Platforms = new PlatformParams
    {
        YouTube = new YouTubeParams { Title = "How to set up your account", PlaylistId = playlist.Id },
    },
});
Console.WriteLine($"Post: {post.Id}");

// Rename it later; arguments left null keep their value
await client.Profiles.UpdateYouTubePlaylistAsync(profileId, playlist.Id, title: "Getting started");

// Deleting a playlist keeps its videos
// await client.Profiles.DeleteYouTubePlaylistAsync(profileId, playlist.Id);
