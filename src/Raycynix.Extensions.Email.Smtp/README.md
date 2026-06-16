# Raycynix.Extensions.Email.Smtp

SMTP provider integration for `Raycynix.Extensions.Email`.

## What It Provides

- `AddSmtp(...)`
- `SmtpConfiguration`
- SMTP provider-specific validation
- `IEmailSender` implementation backed by `System.Net.Mail.SmtpClient`

The provider is selected by calling `.AddSmtp(...)`.

## Usage

```csharp
builder.Services
    .AddRaycynixEmail(builder.Configuration, email =>
    {
        email.DefaultFromAddress = "no-reply@example.com";
        email.DefaultFromDisplayName = "Example App";
    })
    .AddSmtp(smtp =>
    {
        smtp.Host = "smtp.example.com";
        smtp.Port = 587;
        smtp.EnableSsl = true;
        smtp.Username = "smtp-user";
        smtp.Password = "smtp-password";
    });
```

## Configuration

```json
{
  "EmailConfiguration": {
    "DefaultFromAddress": "no-reply@example.com",
    "DefaultFromDisplayName": "Example App",
    "SmtpConfiguration": {
      "Host": "smtp.example.com",
      "Port": 587,
      "EnableSsl": true,
      "Username": "smtp-user",
      "Password": "smtp-password",
      "TimeoutMilliseconds": 100000
    }
  }
}
```
