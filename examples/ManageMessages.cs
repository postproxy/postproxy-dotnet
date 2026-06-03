using PostProxy;
using PostProxy.Models;
using PostProxy.Parameters;

var client = PostProxyClient.Builder(Environment.GetEnvironmentVariable("POSTPROXY_API_KEY")!)
    .ProfileGroupId(Environment.GetEnvironmentVariable("POSTPROXY_PROFILE_GROUP_ID")!)
    .Build();

var profileId = "your-profile-id";

// Create (or find) a chat with a participant
var chat = await client.Chats.CreateAsync(profileId, new CreateChatParams
{
    ParticipantExternalId = "igsid_8675309",
    ParticipantUsername = "jane_doe",
});
Console.WriteLine($"Chat: {chat.Id} with {chat.ParticipantUsername}");

// List chats for a profile
var chats = await client.Chats.ListAsync(profileId, new ListChatsParams { PerPage = 20 });
Console.WriteLine($"Total chats: {chats.Total}");
foreach (var c in chats.Data)
{
    Console.WriteLine($"  {c.Id}: {c.ParticipantName ?? c.ParticipantExternalId} (last message: {c.LastMessageAt})");
}

// List messages in a chat
var messages = await client.Messages.ListAsync(chat.Id, new ListMessagesParams { Direction = MessageDirection.Inbound });
foreach (var m in messages.Data)
{
    Console.WriteLine($"  [{m.Direction}] {m.Body}");
    foreach (var reaction in m.Reactions)
        Console.WriteLine($"    reaction: {reaction.Emoji} ({reaction.ReactionType})");
    foreach (var attachment in m.Attachments)
        Console.WriteLine($"    attachment: {attachment.Type} {attachment.Url}");
}

// Send a text message
var sent = await client.Messages.SendAsync(chat.Id, "Yes, we ship worldwide!");
Console.WriteLine($"Sent: {sent.Id} (status: {sent.Status})");

// Send a message with media (URLs or local files)
var withMedia = await client.Messages.SendAsync(chat.Id, new SendMessageParams
{
    Body = "Here's a photo of the product.",
    Media = new[] { "https://cdn.example.com/product.jpg" },
    // MediaFiles = new[] { "/path/to/photo.png" },
});
Console.WriteLine($"Sent with media: {withMedia.Id}");

// React / unreact to a message
await client.Messages.ReactAsync(sent.Id, new ReactParams { Reaction = "love", Emoji = "❤️" });
Console.WriteLine("Reacted to message");
await client.Messages.UnreactAsync(sent.Id);
Console.WriteLine("Removed reaction");

// Edit a message (Telegram supports editing)
var edited = await client.Messages.EditAsync(sent.Id, new EditMessageParams { Body = "Updated: we ship worldwide!" });
Console.WriteLine($"Edited: {edited.Body}");

// Archive / unarchive a chat (Bluesky)
await client.Chats.ArchiveAsync(chat.Id);
Console.WriteLine("Chat archived");
await client.Chats.UnarchiveAsync(chat.Id);
Console.WriteLine("Chat unarchived");

// Privately reply to a public comment (returns a Message)
var privateReply = await client.Comments.PrivateReplyAsync(
    "your-post-id", "comment-id", profileId, "DM-ing you the details!");
Console.WriteLine($"Private reply sent as message: {privateReply.Id}");
