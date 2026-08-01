# Mail

Email notification libraries providing a clean abstraction over SMTP and cloud email delivery.

## Libraries

| Library | Type | Description |
|---------|------|-------------|
| **NuvTools.Notification.Mail** | Abstraction | `IMailService` abstraction with `MailMessage`, `MailAddress`, `MailPart` and `MailSendResult` models. Supports HTML and plain text content, custom headers, attachments, inline images, personalization variables, tags and scheduled delivery. |
| **NuvTools.Notification.Mail.Smtp** | Implementation | SMTP implementation using MailKit. Configurable via `SmtpMailConfigurationSection` with authentication, SSL/TLS, and attachment support. |
| **NuvTools.Notification.Mail.Twilio** | Implementation | [Twilio Email API](https://www.twilio.com/docs/email/api/overview) implementation using a resilient typed `HttpClient`. |

The application layer references **only** `NuvTools.Notification.Mail` and depends on `IMailService`.
Implementation packages contain nothing but the provider service and its configuration, so they are
referenced exclusively by the composition root.

## Installation

```bash
dotnet add package NuvTools.Notification.Mail
```

Then, in the composition root, one of:

```bash
dotnet add package NuvTools.Notification.Mail.Smtp
dotnet add package NuvTools.Notification.Mail.Twilio
```

## Quick Start

### Sending Emails

```csharp
var mailMessage = new MailMessage
{
    From = new MailAddress { Address = "sender@example.com", DisplayName = "Support Team" },
    To = [new() { Address = "user@example.com", DisplayName = "John Doe" }],
    Subject = "Welcome!",
    Body = "<h1>Welcome to our service!</h1>",
    TextBody = "Welcome to our service!"
};

var result = await mailService.SendAsync(mailMessage, cancellationToken);

Console.WriteLine(result.Reference);         // SMTP: Message-ID | Twilio: operationId
Console.WriteLine(result.TrackingLocation);  // URL to poll for delivery status, when the provider exposes one
```

`MailSendResult` means the request was **accepted** by the provider, not that the email was delivered —
the Twilio Email API in particular processes requests asynchronously and answers with `202 Accepted`.

### Provider Support

Features with no equivalent on the selected provider throw `NotSupportedException` instead of silently
delivering a wrong message:

| Feature | SMTP | Twilio |
|---------|------|--------|
| `Body`, `TextBody`, `Headers` | ✔ | ✔ |
| `Parts` (attachments, `ContentId` inline) | ✔ | ✔ |
| `MailAddress.Variables` (personalization) | `NotSupportedException` | ✔ (Liquid) |
| `Tags` | `NotSupportedException` | ✔ |
| `ScheduledFor` | `NotSupportedException` | ✔ (up to 7 days) |

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
            FileName = "invoice.pdf",
            Content = fileStream
        }
    ]
};

await mailService.SendAsync(mailMessage);
```

`FileName` is required: attachments sent without a name are displayed with an arbitrary name chosen by the
email client (e.g., `ATT00001.pdf` or `noname`).

Note that `MediaType` and `MediaExtension` form the MIME type (`application` + `pdf` → `application/pdf`),
so `MediaExtension` is the media subtype rather than the file extension. For a `.docx` attachment the
subtype is `vnd.openxmlformats-officedocument.wordprocessingml.document`, while the `.docx` extension
belongs to `FileName`.

For Twilio, the total request size, including attachments, must not exceed 10 MB.

### Inline Images

Set `ContentId` to embed the attachment and reference it from the HTML body:

```csharp
using var logo = File.OpenRead("logo.png");
var mailMessage = new MailMessage
{
    From = new MailAddress { Address = "noreply@company.com" },
    To = [new() { Address = "customer@example.com" }],
    Subject = "Newsletter",
    Body = "<p>Our latest news</p><img src=\"cid:logo\" />",
    Parts =
    [
        new()
        {
            MediaType = "image",
            MediaExtension = "png",
            FileName = "logo.png",
            ContentId = "logo",
            Content = logo
        }
    ]
};
```

### Personalization, Tags and Scheduling

```csharp
var mailMessage = new MailMessage
{
    From = new MailAddress { Address = "newsletter@company.com", DisplayName = "Newsletter" },
    To =
    [
        new()
        {
            Address = "jane.doe@example.com",
            Variables = new Dictionary<string, object?> { ["firstName"] = "Jane" }
        },
        new()
        {
            Address = "john.doe@example.com",
            Variables = new Dictionary<string, object?> { ["firstName"] = "John" }
        }
    ],
    Subject = "Hello {{ firstName }}",
    Body = "<p>Hey {{ firstName | default: 'there' }}, your order is ready.</p>",
    Headers = new Dictionary<string, string> { ["X-Campaign-ID"] = "CAMPAIGN-2026-Q1" },
    Tags = new Dictionary<string, string> { ["campaign"] = "monthlyNewsletter" },
    ScheduledFor = DateTimeOffset.UtcNow.AddHours(2)
};

var result = await mailService.SendAsync(mailMessage);
```

Placeholder syntax is defined by the provider — Twilio uses [Liquid](https://liquidjs.com/) in the subject,
HTML and plain text. Twilio tags are limited to 10 pairs, with keys up to 128 characters and values up to 256.

## Configuration

Each provider has its own configuration class deriving from `MailConfigurationSection`, which holds only the
settings shared by every provider:

```
MailConfigurationSection              (NuvTools.Notification.Mail)      From, DisplayName
├── SmtpMailConfigurationSection      (…Mail.Smtp)                      Host, Port, UserName, Password, AllowInvalidCertificates
└── TwilioMailConfigurationSection    (…Mail.Twilio)                    AccountSid, AuthToken, IpPoolName, BaseUrl
```

Section names carry no dots (`MailSmtp`, `MailTwilio`) so every setting can be overridden by an environment
variable on Linux and Azure App Service, where a dot is not valid in a variable name:

```bash
MailTwilio__AuthToken=...
MailSmtp__Password=...
```

### Common Properties

| Property | Default | Description |
|----------|---------|-------------|
| `From` | `null` | Default sender address, used when the message does not specify one. |
| `DisplayName` | `null` | Default sender display name. |

### appsettings.json

SMTP — section `MailSmtp`:

```json
{
  "MailSmtp": {
    "From": "noreply@yourcompany.com",
    "DisplayName": "Your Company",
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UserName": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

| Property | Default | Description |
|----------|---------|-------------|
| `Host` | *(required)* | SMTP server hostname or IP address. |
| `Port` | `0` | SMTP server port (e.g., 25, 587 or 465). |
| `UserName` | *(required)* | User name for SMTP authentication. |
| `Password` | *(required)* | Password for SMTP authentication. |
| `AllowInvalidCertificates` | `false` | Accepts the server TLS certificate without validation. |

The server TLS certificate is validated by default. Development environments using a self-signed certificate
can opt out with `"AllowInvalidCertificates": true` — this disables chain, name and expiration checks and
exposes the connection, and the credentials sent over it, to man-in-the-middle attacks. Never enable it in
production.

Twilio — section `MailTwilio`:

```json
{
  "MailTwilio": {
    "From": "noreply@yourcompany.com",
    "DisplayName": "Your Company",
    "AccountSid": "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "AuthToken": "your-auth-token"
  }
}
```

| Property | Default | Description |
|----------|---------|-------------|
| `AccountSid` | *(required)* | Twilio Account SID, used as the Basic authentication user name. |
| `AuthToken` | *(required)* | Twilio Auth Token, used as the Basic authentication password. |
| `IpPoolName` | `null` | IP Pool used for sending. Dedicated IP accounts only. |
| `BaseUrl` | `https://comms.twilio.com/` | Base address of the Twilio Email API. |

### Dependency Injection Setup

Each implementation package exposes a single registration method that binds its section and registers the
service as `IMailService`:

```csharp
services.AddSmtpMail(configuration);      // NuvTools.Notification.Mail.Smtp
services.AddTwilioMail(configuration);    // NuvTools.Notification.Mail.Twilio
```

Both accept a `sectionName` argument when the configuration lives under a different key:

```csharp
services.AddSmtpMail(configuration, "MyCompany:Smtp");
```

`AddTwilioMail` binds the configuration section and registers `TwilioMailService` as a typed `HttpClient`
resolved as `IMailService`, with the Basic authentication header and a standard resilience pipeline
(2 retries, 30s per-attempt timeout, circuit breaker, 90s total timeout). The pipeline can be customized:

```csharp
services.AddTwilioMail(configuration, configureResilience: options =>
{
    options.Retry.MaxRetryAttempts = 4;
    options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(3);
});
```

Provider failures surface as exceptions: `TwilioMailException` carries the HTTP status code and the raw
response body with all validation errors returned by the API.
