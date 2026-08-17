# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [1.13.0] - 2026-08-17

### Added

- **Interactive DMs on Facebook and Instagram.** `SendMessageParams` gained **`QuickReplies`** (up to 13 tappable chips above the participant's composer, gone once tapped — each `Title` + `Payload`, typed as the new `QuickReply` record) and **`Buttons`** (up to 3 attached to the message, typed as `MessageButton` with `MessageButton.WebUrl(title, url)` / `MessageButton.Postback(title, payload)` factories). An optional **`Card`** (`MessageCard`: `Subtitle`, `ImageUrl`, `DefaultAction` as `CardDefaultAction`, with a `WebUrl(url)` factory) fills in the rest of the card carrying `Buttons`, and requires them. Meta's equivalent of what Telegram has had via `ReplyMarkup`.
- **`Message.TappedAction`** — set on inbound messages created by a tap, typed as the new `TappedAction` record (`Kind`, `Payload`, `Title`) with the new `TappedActionKind` enum. Present on the `message.*` webhook payloads too, so you no longer dig through `PlatformData` for the payload you set. Derived rather than stored, so it also resolves for taps recorded earlier, including Instagram ice-breaker taps and Telegram callback queries (`TappedActionKind.CallbackQuery` — the one part of this that isn't Meta-only).
- `Message.QuickReplies`, `Message.Buttons`, and `Message.Card`, echoing back what was sent.
- Quick-replies and buttons examples in `examples/ManageMessages.cs`, plus the previously undocumented `ReplyMarkup` / `ReplyToExternalId` in the README's Direct Messages section.

### Notes

- Buttons are delivered as a Meta generic template and your `Body` becomes its element title, so **`Body` is capped at 80 characters when buttons are present** — Meta's limit, not ours; longer text is rejected with a `422` naming the length. Buttons cannot be combined with media.
- **Instagram is stricter than Messenger**: it delivers quick replies only on a plain-text message, so `QuickReplies` with media or with `Buttons` returns `422` on Instagram while both are accepted on Facebook.
- Meta-only — `QuickReplies` / `Buttons` / `Card` return `422` on Telegram and Bluesky chats, where `ReplyMarkup` remains the Telegram equivalent.
- Validation is server-side and names the offending index (e.g. `buttons[1].url must be an https:// URL`), surfacing as the usual exception for a `422`. The SDK does not duplicate the limits.
- The new parameters are sent on the JSON path only. To combine quick replies with an attachment, pass `Media` as a hosted URL rather than uploading via `MediaFiles`.

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
