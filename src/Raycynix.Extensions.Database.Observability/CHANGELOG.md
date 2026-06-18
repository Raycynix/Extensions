# Changelog

## 2.2.0
### Added
- Started unified versioning for Raycynix packages from this release.
- Added optional `Microsoft.Extensions.Logging.ILogger<T>` diagnostics for observability setup and operation recording.

## 2.1.0
### Added
- Added optional database tracing and metrics observability integration for initialization, creation, migration, and model-building operations.

## 2.0.0
### Added
- Added optional database observability integration with tracing and metrics support.
- Added `AddObservability()` for Raycynix database builder pipelines.
- Added operation counters and duration histograms for initialization, creation, migration, and model-building operations.

### Changed
- Moved the concrete database observability implementation out of the core database package.
