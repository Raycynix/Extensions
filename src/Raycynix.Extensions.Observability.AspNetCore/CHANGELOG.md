# Changelog

## 3.0.0
### Changed
- Added OpenTelemetry ASP.NET Core metrics instrumentation through Metrics 3.0.
- Added OpenTelemetry ASP.NET Core tracing instrumentation through Tracing 3.0.
- `MapRaycynixObservabilityEndpoints` now maps health checks only; metrics exporters expose their endpoints explicitly.
- Removed the obsolete metrics request middleware from `UseRaycynixObservability`.
- Removed the obsolete custom tracing middleware from `UseRaycynixObservability`.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.
- Added provider-agnostic correlation diagnostics through standard `Microsoft.Extensions.Logging` scopes.
- Added `ObservabilityAspNetCoreConfiguration.IncludeIdentityInLoggingScope` for disabling user and subject values in request logging scopes.

### Changed
- Removed Serilog-specific log context usage from correlation middleware.

## 1.0.0
### Added
- Initial package release.
