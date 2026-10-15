# NuvTools Notification Libraries

A comprehensive set of modular .NET libraries for notification and messaging scenarios, including email, real-time communication, and cloud-based messaging with Azure Service Bus and SignalR.

## Libraries

| Library | NuGet | Type |
|---------|-------|------|
| **NuvTools.Notification.Messaging** | [![NuGet](https://img.shields.io/nuget/v/NuvTools.Notification.Messaging.svg)](https://www.nuget.org/packages/NuvTools.Notification.Messaging) | Abstraction |
| **NuvTools.Notification.Messaging.Azure.ServiceBus** | [![NuGet](https://img.shields.io/nuget/v/NuvTools.Notification.Messaging.Azure.ServiceBus.svg)](https://www.nuget.org/packages/NuvTools.Notification.Messaging.Azure.ServiceBus) | Implementation |
| **NuvTools.Notification.Mail** | [![NuGet](https://img.shields.io/nuget/v/NuvTools.Notification.Mail.svg)](https://www.nuget.org/packages/NuvTools.Notification.Mail) | Abstraction |
| **NuvTools.Notification.Mail.Smtp** | [![NuGet](https://img.shields.io/nuget/v/NuvTools.Notification.Mail.Smtp.svg)](https://www.nuget.org/packages/NuvTools.Notification.Mail.Smtp) | Implementation |
| **NuvTools.Notification.Mail.Twilio** | [![NuGet](https://img.shields.io/nuget/v/NuvTools.Notification.Mail.Twilio.svg)](https://www.nuget.org/packages/NuvTools.Notification.Mail.Twilio) | Implementation |
| **NuvTools.Notification.Realtime** | [![NuGet](https://img.shields.io/nuget/v/NuvTools.Notification.Realtime.svg)](https://www.nuget.org/packages/NuvTools.Notification.Realtime) | Abstraction |
| **NuvTools.Notification.Realtime.Azure.SignalR** | [![NuGet](https://img.shields.io/nuget/v/NuvTools.Notification.Realtime.Azure.SignalR.svg)](https://www.nuget.org/packages/NuvTools.Notification.Realtime.Azure.SignalR) | Implementation |
| **NuvTools.Notification.Realtime.Azure.SignalR.Client** | [![NuGet](https://img.shields.io/nuget/v/NuvTools.Notification.Realtime.Azure.SignalR.Client.svg)](https://www.nuget.org/packages/NuvTools.Notification.Realtime.Azure.SignalR.Client) | Implementation |

## Documentation

- **[Mail](docs/Mail.md)** — Email notifications via SMTP with MailKit or the Twilio Email API
- **[Messaging](docs/Messaging.md)** — Azure Service Bus queues/topics with session and scheduled delivery support
- **[Realtime](docs/Realtime.md)** — Real-time broadcasting with Azure SignalR

## Architecture

### Design Principles

1. **Separation of Concerns**: Abstractions are separate from implementations
2. **Dependency Inversion**: Depend on interfaces, not concrete implementations
3. **Provider Pattern**: Easily swap implementations (e.g., SMTP to Twilio Email)
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

## Releasing

Publishing to nuget.org is irreversible per version, so the workflow makes it the last and smallest
step.

**One workflow per library.** Each package has its own, so a run and an approval each concern
exactly one package. The steps themselves are shared, in the reusable `package-publish.yml`.

| Package | Workflow | Release tag |
|---|---|---|
| `NuvTools.Notification.Mail` | `mail-publish.yml` | `mail-vX.Y.Z` |
| `NuvTools.Notification.Mail.Smtp` | `smtp-publish.yml` | `smtp-vX.Y.Z` |
| `NuvTools.Notification.Mail.Twilio` | `twilio-publish.yml` | `twilio-vX.Y.Z` |
| `NuvTools.Notification.Messaging` | `messaging-publish.yml` | `messaging-vX.Y.Z` |
| `NuvTools.Notification.Messaging.Azure.ServiceBus` | `servicebus-publish.yml` | `servicebus-vX.Y.Z` |
| `NuvTools.Notification.Realtime` | `realtime-publish.yml` | `realtime-vX.Y.Z` |
| `NuvTools.Notification.Realtime.Azure.SignalR` | `signalr-publish.yml` | `signalr-vX.Y.Z` |
| `NuvTools.Notification.Realtime.Azure.SignalR.Client` | `signalr-client-publish.yml` | `signalr-client-vX.Y.Z` |

1. Set the package's `<Version>` in its `.csproj` and merge it. That is the version that gets
   published; the workflow reads it and never computes one.
2. Run the library's workflow by hand (*Actions → Mail Publish → Run workflow*) with *push*
   checked.
3. The workflow builds the solution with `-warnaserror`, runs the tests, takes **that package only**
   from the build output, verifies it carries its README, licence, icon, symbols and every target
   framework, and then waits for approval on the `nuget` GitHub Environment before pushing.
4. **After the push succeeds, the workflow tags the commit** with the tag in the table above. The tag is a
   record of what is on nuget.org, never what starts a release, so it cannot exist for a version
   that did not get there. A run whose tag already exists stops before building: the version has
   been released and `<Version>` needs a bump.

**A referenced package goes first.** A package that references another of this repository as a
project depends on whatever `<Version>` that one has in the commit. The workflow checks that
version is on nuget.org and refuses to push a package whose dependency is not there yet. So:

- `NuvTools.Notification.Mail` before `NuvTools.Notification.Mail.Smtp` and `NuvTools.Notification.Mail.Twilio`.
- `NuvTools.Notification.Messaging` before `NuvTools.Notification.Messaging.Azure.ServiceBus`.
- `NuvTools.Notification.Realtime` before `NuvTools.Notification.Realtime.Azure.SignalR`.

That check knows a version exists, not what is in it: when a referenced package changes its public
API, bump and publish it first, then bump and publish the packages built on it.

**Versions published by hand are refused.** Releases made before these workflows exist on nuget.org
with no tag. Pushing one again would skip the duplicate and then tag the current commit as a
release it is not, so a version that is already on nuget.org stops the run. The exception is a
re-run of the same run, which is how a push that succeeded and a tag that failed gets finished.

**No API key is stored.** The push uses nuget.org's trusted publishing: the job presents a GitHub
OIDC token, nuget.org checks it against the policy registered for this repository and answers with a
key that lives for one hour. The repository needs one secret, `NUGET_USER` — the nuget.org profile
name the policy belongs to — and the policy itself, registered under *Trusted Publishing* on
nuget.org with the owner `nuvtools`, the repository `nuvtools-notification`, the workflow file and the
environment `nuget`.

Run a library's workflow by hand with *push* unchecked for a dry run that stops after the
verification.

## License

Copyright © 2026 Nuv Tools. All rights reserved.

## Resources

- **Website**: [nuvtools.com](https://nuvtools.com)
- **NuGet Packages**: Available on [NuGet.org](https://www.nuget.org/profiles/NuvTools)
