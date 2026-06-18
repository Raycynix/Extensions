# Raycynix.Extensions.Email

Shared email registration infrastructure for Raycynix applications.

## What It Provides

- `AddRaycynixEmail(...)`
- `EmailConfiguration`
- email provider resolution
- email builder support for provider packages

This package registers the shared email infrastructure. A provider package, such as `Raycynix.Extensions.Email.Smtp`, must be added to provide an `IEmailSender` implementation.

## Usage

```csharp
builder.Services
    .AddRaycynixEmail(builder.Configuration, email =>
    {
        email.DefaultFromAddress = "no-reply@example.com";
        email.DefaultFromDisplayName = "Example App";
    })
    .AddSmtp();
```

## Configuration

```json
{
  "EmailConfiguration": {
    "DefaultFromAddress": "no-reply@example.com",
    "DefaultFromDisplayName": "Example App",
    "DefaultReplyToAddress": "support@example.com",
    "DefaultReplyToDisplayName": "Support"
  }
}
```
