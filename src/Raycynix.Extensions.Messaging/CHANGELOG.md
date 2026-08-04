# Changelog

## 3.0.0
### Changed
- Migrated optional messaging counters and duration histograms to `System.Diagnostics.Metrics`.
- Replaced positional Prometheus labels with named, provider-neutral tags and explicit instrument units.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.
- Added optional Microsoft `ILogger<T>` diagnostics for publish, dispatch, incoming processing, and outbox recovery flows.

## 1.0.1
### Added
- Initial package release.
