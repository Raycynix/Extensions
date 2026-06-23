# Changelog

## 2.2.0
### Added
- Started unified versioning for Raycynix packages from this release.
- Added optional `Microsoft.Extensions.Logging.ILogger<T>` diagnostics for MySQL provider validation, connection-string source selection, and EF Core provider configuration.

## 2.1.0
### Added
- Added MySQL provider registration with provider-specific validation.
- Added MySQL connection-string composition.
- Added retry, command timeout, pooling, user-variable, and migrations assembly support.

## 2.0.0
### Added
- Added MySQL provider registration through `AddMySql(...)`.
- Added provider-specific structured configuration validation.
- Added MySQL connection-string composition.
- Added command timeout, retry, pooling, user-variable, and migrations assembly support.

## 1.0.2
### Added
- Initial package release.
