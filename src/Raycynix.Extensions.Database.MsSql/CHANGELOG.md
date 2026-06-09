# Changelog

## 2.1.0
### Added
- Added SQL Server provider registration with provider-specific validation.
- Added SQL Server connection-string composition.
- Added retry, command timeout, MARS, certificate trust, and migrations assembly support.

### Changed
- Kept `TrustServerCertificate` disabled by default unless explicitly configured.

## 2.0.0
### Added
- Added SQL Server provider registration through `AddMsSql(...)`.
- Added provider-specific structured configuration validation.
- Added SQL Server connection-string composition.
- Added command timeout, retry, MARS, certificate trust, and migrations assembly support.

### Changed
- Default `TrustServerCertificate` is disabled unless explicitly configured.

## 1.0.2
### Added
- Initial package release.
