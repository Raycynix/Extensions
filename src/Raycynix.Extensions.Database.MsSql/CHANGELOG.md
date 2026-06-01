# Changelog

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
