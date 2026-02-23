using System.Text.Json.Serialization;

namespace PostProxy.Models;

[JsonConverter(typeof(JsonStringEnumConverter<Platform>))]
public enum Platform
{
    [JsonStringEnumMemberName("facebook")] Facebook,
    [JsonStringEnumMemberName("instagram")] Instagram,
    [JsonStringEnumMemberName("tiktok")] TikTok,
    [JsonStringEnumMemberName("linkedin")] LinkedIn,
    [JsonStringEnumMemberName("youtube")] YouTube,
    [JsonStringEnumMemberName("twitter")] Twitter,
    [JsonStringEnumMemberName("threads")] Threads,
    [JsonStringEnumMemberName("pinterest")] Pinterest,
}

[JsonConverter(typeof(JsonStringEnumConverter<PostStatus>))]
public enum PostStatus
{
    [JsonStringEnumMemberName("pending")] Pending,
    [JsonStringEnumMemberName("draft")] Draft,
    [JsonStringEnumMemberName("processing")] Processing,
    [JsonStringEnumMemberName("processed")] Processed,
    [JsonStringEnumMemberName("scheduled")] Scheduled,
}

[JsonConverter(typeof(JsonStringEnumConverter<ProfileStatus>))]
public enum ProfileStatus
{
    [JsonStringEnumMemberName("active")] Active,
    [JsonStringEnumMemberName("expired")] Expired,
    [JsonStringEnumMemberName("inactive")] Inactive,
}

[JsonConverter(typeof(JsonStringEnumConverter<PlatformPostStatus>))]
public enum PlatformPostStatus
{
    [JsonStringEnumMemberName("pending")] Pending,
    [JsonStringEnumMemberName("processing")] Processing,
    [JsonStringEnumMemberName("published")] Published,
    [JsonStringEnumMemberName("failed")] Failed,
    [JsonStringEnumMemberName("deleted")] Deleted,
}

[JsonConverter(typeof(JsonStringEnumConverter<FacebookFormat>))]
public enum FacebookFormat
{
    [JsonStringEnumMemberName("post")] Post,
    [JsonStringEnumMemberName("story")] Story,
}

[JsonConverter(typeof(JsonStringEnumConverter<InstagramFormat>))]
public enum InstagramFormat
{
    [JsonStringEnumMemberName("post")] Post,
    [JsonStringEnumMemberName("reel")] Reel,
    [JsonStringEnumMemberName("story")] Story,
}

[JsonConverter(typeof(JsonStringEnumConverter<TikTokFormat>))]
public enum TikTokFormat
{
    [JsonStringEnumMemberName("video")] Video,
    [JsonStringEnumMemberName("image")] Image,
}

[JsonConverter(typeof(JsonStringEnumConverter<LinkedInFormat>))]
public enum LinkedInFormat
{
    [JsonStringEnumMemberName("post")] Post,
}

[JsonConverter(typeof(JsonStringEnumConverter<YouTubeFormat>))]
public enum YouTubeFormat
{
    [JsonStringEnumMemberName("post")] Post,
}

[JsonConverter(typeof(JsonStringEnumConverter<TwitterFormat>))]
public enum TwitterFormat
{
    [JsonStringEnumMemberName("post")] Post,
}

[JsonConverter(typeof(JsonStringEnumConverter<ThreadsFormat>))]
public enum ThreadsFormat
{
    [JsonStringEnumMemberName("post")] Post,
}

[JsonConverter(typeof(JsonStringEnumConverter<PinterestFormat>))]
public enum PinterestFormat
{
    [JsonStringEnumMemberName("pin")] Pin,
}

[JsonConverter(typeof(JsonStringEnumConverter<YouTubePrivacy>))]
public enum YouTubePrivacy
{
    [JsonStringEnumMemberName("public")] Public,
    [JsonStringEnumMemberName("unlisted")] Unlisted,
    [JsonStringEnumMemberName("private")] Private,
}

[JsonConverter(typeof(JsonStringEnumConverter<TikTokPrivacy>))]
public enum TikTokPrivacy
{
    [JsonStringEnumMemberName("PUBLIC_TO_EVERYONE")] PublicToEveryone,
    [JsonStringEnumMemberName("MUTUAL_FOLLOW_FRIENDS")] MutualFollowFriends,
    [JsonStringEnumMemberName("FOLLOWER_OF_CREATOR")] FollowerOfCreator,
    [JsonStringEnumMemberName("SELF_ONLY")] SelfOnly,
}
