# Mail

Email notification libraries providing a clean abstraction over SMTP delivery.

## Libraries

| Library | Type | Description |
|---------|------|-------------|
| **NuvTools.Notification.Mail** | Abstraction | `IMailService` abstraction with `MailMessage`, `MailAddress`, and `MailPart` models. Supports HTML content and file attachments. |
| **NuvTools.Notification.Mail.Smtp** | Implementation | SMTP implementation using MailKit. Configurable via `MailConfigurationSection` with authentication, SSL/TLS, and attachment support. |

## Installation

```bash
dotnet add package NuvTools.Notification.Mail
dotnet add package NuvTools.Notification.Mail.Smtp
```

## Quick Start

### Sending Emails via SMTP

```csharp
var mailMessage = new MailMessage
{
    From = new MailAddress { Address = "sender@example.com", DisplayName = "Support Team" },
    To = [new() { Address = "user@example.com", DisplayName = "John Doe" }],
    Subject = "Welcome!",
    Body = "<h1>Welcome to our service!</h1>"
};

await mailService.SendAsync(mailMessage);
```

## Usage Examples

### Email with Attachments

```csharp
using var fileStream = File.OpenRead("document.pdf");
var mailMessage = new MailMessage
{
    From = new MailAddress { Address = "noreply@company.com" },
    To = [new() { Address = "customer@example.com" }],
    Subject = "Your Invoice",
    Body = "<p>Please find your invoice attached.</p>",
    Parts =
    [
        new()
        {
            MediaType = "application",
            MediaExtension = "pdf",
            Content = fileStream
        }
    ]
};

await mailService.SendAsync(mailMessage);
```

## Configuration

### appsettings.json

```json
{
  "NuvTools.Notification.Mail": {
    "From": "noreply@yourcompany.com",
    "DisplayName": "Your Company",
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UserName": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

### Dependency Injection Setup

```csharp
services.AddMailConfiguration<MailConfigurationSection>(configuration);
services.AddSingleton<IMailService, SMTPMailService>();
```
