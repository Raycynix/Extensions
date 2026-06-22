# Changelog

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.
- Added optional Microsoft `ILogger<T>` diagnostics for SMTP configuration validation, MIME creation counts, and delivery phases.

## 2.0.0
### Added
- Added SMTP email provider registration through `AddSmtp(...)`.
- Added SMTP configuration binding and validation.
- Added MailKit-based SMTP `IEmailSender` implementation for text, HTML, recipients, reply-to, headers, and attachments.
