# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [1.9.0] - 2026-05-15

### Added

- `Platform.GoogleBusiness` (serialized as `google_business`) for posts and profiles.
- `ProfileCommentsResource`: `ListAsync`, `GetAsync`, `CreateAsync`, `DeleteAsync` for review replies via `/api/profiles/:profile_id/comments`. Accessed via `client.ProfileComments`.
- `MediaPlatformError` record and `Media.Platforms` property for per-media platform error reporting.
- `PlatformParams.GoogleBusiness` (`Dictionary<string, object>`) for Google Business post parameters.
