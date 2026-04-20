# Changelog

## 2.0.0
### Added
- Added updated package examples and test coverage for the structured logging API.

### Changed
- Reworked the logger API around message-template arguments instead of metadata payload objects.
- Updated typed logger overloads and XML documentation to reflect the new template-based contract.

### Fixed
- Fixed correlation id enrichment so ambient operation context values are included in log events.
- Fixed log output when no extra structured arguments are provided so messages no longer append `null`.
