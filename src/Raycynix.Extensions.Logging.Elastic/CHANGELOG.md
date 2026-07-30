# Changelog

## 3.0.0
### Changed
- Replaced `ElasticConfiguration` with `ElasticOptions`.
- Changed the nested section from `LoggingConfiguration:ElasticConfiguration` to `LoggingOptions:ElasticOptions`.
- Restricted enabled Elasticsearch endpoints to absolute HTTP or HTTPS URLs.

### Fixed
- Prevented the Elasticsearch configurator from resolving options through the logging provider during Serilog startup.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.

## 2.1.0
### Added
- Initial Elasticsearch logging integration package.
- Added `AddElastic()` for registering the Serilog Elasticsearch sink through `LoggingBuilder`.
- Added `ElasticConfiguration`, bound from `LoggingConfiguration:ElasticConfiguration`.
