# NuvTools Notification Libraries

A comprehensive set of modular .NET libraries for notification and messaging scenarios, including email, real-time communication, and cloud-based messaging with Azure Service Bus and SignalR.

## Libraries

| Library | NuGet | Type |
|---------|-------|------|
| **NuvTools.Notification.Messaging** | [![NuGet](https://img.shields.io/nuget/v/NuvTools.Notification.Messaging.svg)](https://www.nuget.org/packages/NuvTools.Notification.Messaging) | Abstraction |
| **NuvTools.Notification.Messaging.Azure.ServiceBus** | [![NuGet](https://img.shields.io/nuget/v/NuvTools.Notification.Messaging.Azure.ServiceBus.svg)](https://www.nuget.org/packages/NuvTools.Notification.Messaging.Azure.ServiceBus) | Implementation |
| **NuvTools.Notification.Mail** | [![NuGet](https://img.shields.io/nuget/v/NuvTools.Notification.Mail.svg)](https://www.nuget.org/packages/NuvTools.Notification.Mail) | Abstraction |
| **NuvTools.Notification.Mail.Smtp** | [![NuGet](https://img.shields.io/nuget/v/NuvTools.Notification.Mail.Smtp.svg)](https://www.nuget.org/packages/NuvTools.Notification.Mail.Smtp) | Implementation |
| **NuvTools.Notification.Realtime** | [![NuGet](https://img.shields.io/nuget/v/NuvTools.Notification.Realtime.svg)](https://www.nuget.org/packages/NuvTools.Notification.Realtime) | Abstraction |
| **NuvTools.Notification.Realtime.Azure.SignalR** | [![NuGet](https://img.shields.io/nuget/v/NuvTools.Notification.Realtime.Azure.SignalR.svg)](https://www.nuget.org/packages/NuvTools.Notification.Realtime.Azure.SignalR) | Implementation |
| **NuvTools.Notification.Realtime.Azure.SignalR.Client** | [![NuGet](https://img.shields.io/nuget/v/NuvTools.Notification.Realtime.Azure.SignalR.Client.svg)](https://www.nuget.org/packages/NuvTools.Notification.Realtime.Azure.SignalR.Client) | Implementation |

## Documentation

- **[Mail](docs/Mail.md)** — Email notifications via SMTP with MailKit
- **[Messaging](docs/Messaging.md)** — Azure Service Bus queues/topics with session and scheduled delivery support
- **[Realtime](docs/Realtime.md)** — Real-time broadcasting with Azure SignalR

## Architecture

### Design Principles

1. **Separation of Concerns**: Abstractions are separate from implementations
2. **Dependency Inversion**: Depend on interfaces, not concrete implementations
3. **Provider Pattern**: Easily swap implementations (e.g., SMTP to SendGrid)
4. **Generic Message Envelope**: Type-safe message bodies with common metadata
5. **Background Processing**: Built-in hosted services for message consumers

## Building and Development

### Prerequisites
- .NET 8 SDK or later
- Visual Studio 2022 17.4+ or JetBrains Rider 2023.1+ (for `.slnx` support)

### Build Commands

```bash
dotnet build NuvTools.Notification.slnx                # Build all
dotnet build -c Release NuvTools.Notification.slnx      # Release build + NuGet packages
dotnet restore NuvTools.Notification.slnx               # Restore packages
dotnet clean NuvTools.Notification.slnx                 # Clean build artifacts
```

## Target Frameworks

All libraries target `.NET 8`, `.NET 9`, and `.NET 10`.

## Contributing

Contributions are welcome! Please ensure:
1. All code follows existing patterns and conventions
2. XML documentation is provided for all public APIs
3. Code builds without warnings

## License

Copyright © 2026 Nuv Tools. All rights reserved.

## Resources

- **Website**: [nuvtools.com](https://nuvtools.com)
- **NuGet Packages**: Available on [NuGet.org](https://www.nuget.org/profiles/NuvTools)
