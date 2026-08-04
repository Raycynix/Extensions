# Changelog

## 3.0.0
### Added
- Added stable OpenTelemetry ASP.NET Core request instrumentation and `TracerProviderBuilder` composition.
- Enabled standard `TraceId` and `SpanId` logging scopes through `LoggerFactoryOptions`.

### Removed
- Removed `TracingMiddleware` and `UseRaycynixTracing()`.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.
- Added provider-agnostic request trace diagnostics through standard `Microsoft.Extensions.Logging` scopes.

### Changed
- Removed Serilog-specific log context usage from tracing middleware.

## 1.0.0
### Added
- Initial package release.
