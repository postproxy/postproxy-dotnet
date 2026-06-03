# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

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
