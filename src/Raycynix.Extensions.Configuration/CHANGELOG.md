# Changelog

## 3.0.0
### Changed
- Aligned the package version with the net10.0 / Microsoft.Extensions 10 package line.
- Renamed the source setup options type from `ConfigurationSourcesConfiguration` to `ConfigurationSourcesOptions` to follow the Options naming convention for setup objects.
- Updated package description, tags, release notes, and README content for the current typed options, diagnostics, feature flag, reload, and logging behavior.

## 2.2.0
### Added
- Started unified versioning for Raycynix packages from this release.
- Added optional `Microsoft.Extensions.Logging.ILogger<T>` diagnostics that do not require a logger provider to be registered.
- Added detailed `LogDebug` diagnostics for configuration change tracking startup, registrations, reload policy decisions, and change handler execution.
- Added optional diagnostics for configuration validation and redacted snapshot access.

## 2.0.0
### Added
- Added required-section validation for typed configuration and feature flags.
- Added named options support for typed registrations and accessors.
- Added approved runtime snapshots so rejected reloads do not replace the exposed current configuration.
- Added diagnostics for registrations, retained reload decisions, and redacted snapshots.
- Added configurable diagnostics options for snapshot access and reload history length.
- Added default and custom configuration redactor support.
- Added expanded reload behaviors for apply, reject, ignore, and restart-required decisions.

## 1.0.1
### Added
- Initial package release.
