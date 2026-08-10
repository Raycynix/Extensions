# Changelog

## 3.0.0
### Changed
- Replaced the custom tracer registration with the shared standard `System.Diagnostics.ActivitySource`.
- Made listener, sampler, and exporter selection an explicit host concern.
- Removed runtime logging around individual tracing operations.

### Removed
- Removed the `Tracer` implementation and its assembly-derived source naming.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.
- Added optional `Microsoft.Extensions.Logging` diagnostics for tracer creation and activity operations.

### Changed
- Removed the dependency on `Raycynix.Extensions.Logging` from the tracing runtime package.

## 1.0.0
### Added
- Initial package release.
