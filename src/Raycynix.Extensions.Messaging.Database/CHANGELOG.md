# Changelog

## 1.0.1
### Added
- Added package-level changelog tracking.
- Expanded persistence test coverage around inbox/outbox concurrency, stale lease recovery, and cleanup behavior.

### Changed
- Updated test infrastructure to align with the current logging abstractions used by the shared packages.

### Fixed
- Updated no-op test logger implementations so database-backed messaging tests are no longer sensitive to removed metadata logging calls.
