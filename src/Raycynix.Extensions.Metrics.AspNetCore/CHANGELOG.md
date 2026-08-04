# Changelog

## 3.0.0
### Added
- Added stable OpenTelemetry registration for the shared Raycynix meter and ASP.NET Core request instrumentation.

### Removed
- Removed the prometheus-net HTTP middleware and endpoint wrappers.
- Removed implicit coupling to the obsolete metrics configuration model.
- Kept the prerelease OpenTelemetry Prometheus exporter out of the stable package dependency graph; applications can opt into it explicitly.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.

## 1.0.0
### Added
- Initial package release.
