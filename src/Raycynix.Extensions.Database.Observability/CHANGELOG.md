# Changelog

## 2.0.0
### Added
- Added optional database observability integration with tracing and metrics support.
- Added `AddObservability()` for Raycynix database builder pipelines.
- Added operation counters and duration histograms for initialization, creation, migration, and model-building operations.

### Changed
- Moved the concrete database observability implementation out of the core database package.
