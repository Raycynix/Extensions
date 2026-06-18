# Changelog

## 2.2.0
### Added
- Started unified versioning for Raycynix packages from this release.
- Added optional feature gate middleware diagnostics through standard `Microsoft.Extensions.Logging.ILogger<T>`.
- Added `LogDebug` diagnostics for feature gate evaluation flow.

## 2.0.0
### Added
- Added configurable feature gate response status codes.
- Added lazy feature flag accessor resolution so ungated endpoints do not require feature flag services.
- Added feature gate options registration helper.
- Aligned ASP.NET Core integrations with Configuration v2.

## 1.0.0
### Added
- Initial package release.
