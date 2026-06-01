# Changelog

## 2.1.0
### Added
- Added `LoggingBuilder` for optional logging integrations.
- Added support for external Serilog configurators through `IRaycynixLoggingConfigurator`.
- Added a no-configuration `AddRaycynixLogging()` overload for default logging registration.

### Changed
- Moved shared logging configuration types to `Raycynix.Extensions.Logging.Abstractions`.
- Removed Elasticsearch sink setup from the base logging package; use `Raycynix.Extensions.Logging.Elastic` when Elasticsearch output is required.
- `UseRaycynixLogging(...)` now prefers the `LoggingConfiguration` registered in DI, while preserving the previous fallback to host configuration and defaults when logging services are not registered.

## 2.0.0
### Added
- Added updated package examples and test coverage for the structured logging API.

### Changed
- Reworked the logger API around message-template arguments instead of metadata payload objects.
- Updated typed logger overloads and XML documentation to reflect the new template-based contract.

### Fixed
- Fixed correlation id enrichment so ambient operation context values are included in log events.
- Fixed log output when no extra structured arguments are provided so messages no longer append `null`.
