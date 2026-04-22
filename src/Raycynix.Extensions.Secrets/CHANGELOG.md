# Changelog

## 1.1.0
### Added
- Added configuration-backed secret resolution through `IConfiguration`.
- Added `SecretOptions` for customizing provider precedence.
- Added required-secret and diagnostic APIs for provider-aware resolution and explain output.
- Added tests covering configuration-provider resolution and provider precedence.

### Changed
- Updated the default provider chain so configuration values are checked before environment-based fallbacks.
- Updated package documentation to position `Raycynix.Extensions.Secrets` as a unified secret-resolution layer over configuration and CI-friendly environment providers.

## 1.0.1
### Added
- Added a console example that demonstrates composite secret resolution across environment, GitHub-style, and TeamCity-style variables.

### Changed
- Clarified example usage around provider ordering and missing-secret fallback behavior.
