using PostProxy;
using PostProxy.Models;
using PostProxy.Parameters;

var client = PostProxyClient.Builder("your-api-key")
    .Build();

var profileId = "your-instagram-profile-id";

var chats = await client.Chats.ListAsync(profileId, new ListChatsParams { PerPage = 1 });
var chat = chats.Data[0];

// Show the participant we're on it, then send a product carousel
await client.Chats.MarkReadAsync(chat.Id);
await client.Chats.StartTypingAsync(chat.Id);

await client.Messages.SendAsync(chat.Id, new SendMessageParams
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
            Subtitle = "$92 — back in stock soon",
            ImageUrl = "https://cdn.example.com/blue.png",
            Buttons = [MessageButton.Postback("Notify me", "NOTIFY:blue")],
        },
    ],
});

await client.Chats.StopTypingAsync(chat.Id);
