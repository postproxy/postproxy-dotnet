# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [1.12.0] - 2026-08-06

### Added

- **Post syncs & backfill.** `Profiles.BackfillPostsAsync(id, from)` walks a profile's feed backwards from the newest post and imports the history behind it; `Profiles.ListPostSyncsAsync(id, ...)` and `Profiles.GetPostSyncAsync(id, postSyncId)` expose every post pull — the one fired on connect, the recurring poll, and backfills — as the new `PostSync` record, with the `PostSyncTrigger` and `PostSyncStatus` enums.
- **`Comments.ListAllAsync(postIds, profiles, from, to, page, perPage, profileGroupId)`** — comments across every post in the profile group in one request. Flat: replies are their own entries linked by `ParentExternalId`, typed as the new `BulkComment` record (adds `PostId`, `ProfileId`, `Platform`).
- `from` and `to` arguments on `Comments.ListAsync`, filtering on when PostProxy received the comment.
- **Idempotency.** Every write sends an `Idempotency-Key` header when given one: parameters records gain an `IdempotencyKey` property, and the remaining write methods take an `idempotencyKey` argument (added as overloads, so existing calls still compile).
- `ConflictException` (409), thrown for a duplicate submission (`Response["duplicate_post_id"]`), a backfill already running (`Response["profile_sync_id"]`), or an in-flight idempotency key. Previously these surfaced as a bare `PostProxyException`.
- **Instagram user tags.** `InstagramParams.UserTags` with the new `InstagramUserTag` record (`Username`, `X`, `Y`, `MediaIndex`) — tag accounts on feed posts, reels, and stories.
- `StatsRecord.RawStats` — every metric under its original platform name, alongside the normalized `Stats`.
- `examples/BackfillPosts.cs`, and cross-post comment listing in `examples/ManageComments.cs`.

### Changed

- LinkedIn post stats now normalize `likes`, `comments`, `shares`, and `clicks` alongside `impressions` (server-side; `Stats` was already an open dictionary).
- `HUMAN_AGENT` is now approved on **both** Facebook and Instagram and extends the reply window to 7 days. `SendMessageParams.Tag` is unchanged — see the README for Meta's policy limits.

## [1.11.0] - 2026-07-14

### Added

- `Profiles.GetIceBreakersAsync`, `Profiles.SetIceBreakersAsync`, and `Profiles.DeleteIceBreakersAsync` for managing Instagram DM ice breakers, with `IceBreaker` and `IceBreakersResponse` models.
- `Profiles.AssignPlacementToGroupAsync(id, placementId, targetProfileGroupId)` to move a placement (Facebook Page, Telegram channel, GBP location) to another profile group; returns the new `AssignedPlacement` model.
- `Placement.Metadata` property.
- Twitter polls: `TwitterFormat.Poll`, and `TwitterParams` gains `PollOptions` (2-4 choices, max 25 chars each) and `PollDurationMinutes` (5-10080).

## [1.10.0] - 2026-06-03

### Added

- **Direct Messages.** New `ChatsResource` (`client.Chats`) with `ListAsync`, `CreateAsync`, `GetAsync`, `ArchiveAsync`, `UnarchiveAsync` and `MessagesResource` (`client.Messages`) with `ListAsync`, `SendAsync`, `GetAsync`, `EditAsync`, `ReactAsync`, `UnreactAsync`. `SendAsync` supports text, media URLs, and local media files (multipart upload), plus `tag`, `reply_to_external_id`, and `reply_markup`.
- New models: `Chat`, `Message`, `Reaction`, `Attachment`. New `MessageDirection` and `MessageStatus` enums.
- `Comment.Attachments` (`IReadOnlyList<Attachment>`) and `Comment.Metadata` for media attachments and author signals.
- `CommentsResource.PrivateReplyAsync` — privately reply to a public comment; returns a `Message`.
- 10 new `WebhookEventType` members: `profile_comment.created`, `message.received`, `message.sent`, `message.delivered`, `message.read`, `message.edited`, `message.deleted`, `message.failed_waiting_for_retry`, `message.failed`, `reaction.received`.
- New typed webhook payloads `MessageEventData`, `ReactionEventData`, `ProfileCommentCreatedData` with `WebhookEvents.AsMessageEvent`, `AsReactionEvent`, `AsProfileCommentCreated` decoders.

## [1.9.0] - 2026-05-15

### Added

- `Platform.GoogleBusiness` (serialized as `google_business`) for posts and profiles.
- `ProfileCommentsResource`: `ListAsync`, `GetAsync`, `CreateAsync`, `DeleteAsync` for review replies via `/api/profiles/:profile_id/comments`. Accessed via `client.ProfileComments`.
- `MediaPlatformError` record and `Media.Platforms` property for per-media platform error reporting.
- `PlatformParams.GoogleBusiness` (`Dictionary<string, object>`) for Google Business post parameters.
