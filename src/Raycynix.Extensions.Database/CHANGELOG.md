# Changelog

## 2.0.0
### Added
- Added `RaycynixDatabaseContext` as the extensible base context.
- Added `AddRaycynixDatabase<TContext>(...)` for custom context registration.
- Added default no-op database observability through `IDatabaseObservability`.

### Changed
- Moved shared database configuration and contracts to `Raycynix.Extensions.Database.Abstractions`.
- Moved tracing and metrics database observability to `Raycynix.Extensions.Database.Observability`.
- Reduced core database package dependencies by removing direct metrics and tracing references.

## 1.0.2
### Added
- Added package-level changelog tracking.

### Changed
- Updated database package examples and test stubs to align with the current logging abstractions.

### Fixed
- Updated test fake loggers so database test runs are no longer coupled to removed metadata-based logging APIs.
