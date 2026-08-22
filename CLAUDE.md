# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Modular .NET library framework for notification and messaging. Follows an **abstraction-implementation pattern** where each notification domain has an abstraction library defining interfaces and one or more infrastructure-specific implementation libraries.

## Build Commands

```bash
dotnet build NuvTools.Notification.slnx              # Build all projects
dotnet build src/NuvTools.Notification.Messaging/NuvTools.Notification.Messaging.csproj  # Build one project
dotnet build -c Release NuvTools.Notification.slnx    # Release build (generates NuGet packages)
```

No test projects exist in this repository.

## Architecture

### Three Notification Domains

| Domain | Abstraction | Implementation(s) |
|--------|------------|-------------------|
| **Messaging** | `NuvTools.Notification.Messaging` | `NuvTools.Notification.Messaging.Azure.ServiceBus` |
| **Mail** | `NuvTools.Notification.Mail` | `NuvTools.Notification.Mail.Smtp` (MailKit), `NuvTools.Notification.Mail.Twilio` (Twilio Email API) |
| **Realtime** | `NuvTools.Notification.Realtime` | `NuvTools.Notification.Realtime.Azure.SignalR` (server), `NuvTools.Notification.Realtime.Azure.SignalR.Client` |

### Key Interfaces

- **Messaging sender**: `IMessageSender<TBody>` — `SendAsync(Message<TBody>, CancellationToken)`
- **Messaging consumer**: `IMessageConsumer<TBody>` — `ConsumeAsync(Message<TBody>, IMessageContext, CancellationToken)`
- **Message lifecycle**: `IMessageContext` — `CompleteAsync`, `AbandonAsync`, `DeadLetterAsync` (explicit acknowledgment; `AutoCompleteMessages` defaults to `false`)
- **Mail**: `IMailService` — `SendAsync(MailMessage, CancellationToken)` returning `MailSendResult` (`Reference`, `TrackingLocation`)
- **Realtime sender**: `Realtime.Interfaces.IMessageSender<T>` — `SendAsync(T, CancellationToken)` (separate namespace from messaging sender)

### Message Envelope Pattern

`Message<T>` wraps payloads with metadata: `MessageId` (auto GUID), `CorrelationId`, `Subject`, `SessionId` (for ordered processing), `TimeToLive`, `ScheduledEnqueueTime` (delayed delivery), and `Properties` dictionary for custom headers.

### Azure Service Bus Receiver Hierarchy

```
BackgroundService
  └─ AzureServiceBusReceiverBase<TBody, TConsumer>    (shared processing, error handling, retry)
       ├─ AzureServiceBusReceiver<TBody, TConsumer>    (standard parallel processing)
       └─ AzureServiceBusSessionReceiver<TBody, TConsumer> (sequential per-SessionId, MaxConcurrentCallsPerSession=1)
```

- **Consumer resolution**: Creates a new DI scope per message (`ServiceProvider.CreateScope()`), resolves `TConsumer` from that scope.
- **Error handling**: Deserialization failures → dead-letter. Consumer exceptions → abandon for retry. Message lock lost → warning logged, Azure retries. Processor stopped → restart with 3 exponential backoff attempts (5s, 10s, 20s).
- **Double-completion guard**: Internal `AzureMessageContext` tracks `IsMessageCompleted` flag to prevent calling complete/abandon/dead-letter twice.

### Azure Service Bus Sender

`AzureServiceBusSender<TBody>` is abstract — subclass to specify entity. Supports initialization from `ServiceBusClient`, connection string, or `MessagingSection`.

### Service Bus Authentication

`ServiceBusClientFactory.Create(MessagingSection, TokenCredential?)` is the single place a `ServiceBusClient` is built. `ConnectionString` (shared access key) takes precedence; otherwise `FullyQualifiedNamespace` is used with the supplied credential, or a `DefaultAzureCredential` derived from `ManagedIdentityClientId`. Senders and both receivers take an optional trailing `TokenCredential`.

`DefaultAzureCredential` comes from **Azure.Core 1.60+**, which absorbed the Azure.Identity types — referencing `Azure.Identity` as well makes them ambiguous (CS0433), so this package references `Azure.Core` directly instead.

Receivers build their processor from the base class's `Client` rather than opening a second connection, which is why they use explicit constructors instead of primary constructors (derived field initializers run before the base constructor).

### SignalR Convention

`AzureSignalRSender<T>` broadcasts via `hubContext.Clients.All` using method name `"Consume_{typeof(T).Name}"`. `AzureSignalRReceiver<T>` registers a handler for the same method name and applies debouncing (default 1000ms) to prevent event flooding.

### Mail Layering Rule

The application layer references **only** `NuvTools.Notification.Mail`. Implementation packages must not declare abstractions of their own — no provider-specific interface, message or result type. Every provider-neutral capability (`TextBody`, `Headers`, `Tags`, `ScheduledFor`, `MailAddress.Variables`, `MailPart.ContentId`, `MailSendResult`) lives in the abstraction; implementation packages hold only the service, its configuration section and internal DTOs.

Capabilities a provider cannot honor throw `NotSupportedException` (e.g., SMTP for `ScheduledFor`, `Tags` and `Variables`) — never silently ignored, since sending immediately or with unresolved placeholders is worse than failing.

### Twilio Email Convention

`TwilioMailService` is a typed `HttpClient` posting to `POST {BaseUrl}v1/Emails` (`https://comms.twilio.com/` by default) with Basic authentication built from `AccountSid:AuthToken`. This is the **Twilio Email API**, not the legacy SendGrid v3 API.

- **Registration**: `AddTwilioMail(configuration)` is the only entry point — it binds the section, configures the client, applies `AddStandardResilienceHandler`, and registers the service as `IMailService`.
- **Async by design**: the API answers `202 Accepted` with `{ operationId, operationLocation }`, mapped to `MailSendResult.Reference`/`TrackingLocation`. Acceptance is not delivery.
- **Error handling**: non-2xx responses throw `TwilioMailException` carrying the status code and raw body (Twilio returns all validation errors at once). Matches the "exceptions propagate" style of `SMTPMailService`.
- **Attachments**: streams are Base64-encoded in memory; `contentType` is `"{MediaType}/{MediaExtension}"` and `filename` comes from the required `MailPart.FileName`. Requests over 10 MB are rejected locally.
- **Twilio-only settings** (e.g., `IpPoolName`) live in `TwilioMailConfigurationSection`, never on the message.

### Serialization

All message serialization uses `System.Text.Json` with `JsonSerializerDefaults.Web` (camelCase property names). The Twilio DTOs additionally pin every property with explicit `[JsonPropertyName]` because the API mixes casings (`filename` vs `contentType`).

## Configuration Pattern

All libraries use `IOptions<TSection>` with typed configuration classes registered via `ServiceCollectionExtensions`:
- `AddMessagingQueueConfiguration<T>(services, configuration, sectionName)` → `MessagingSection`
- `AddMailConfiguration<T>(services, configuration, sectionName)` → `MailConfigurationSection` or a derived provider section
- `AddSmtpMail(services, configuration, sectionName)` → `SmtpMailConfigurationSection` + `IMailService`
- `AddTwilioMail(services, configuration, sectionName, configureResilience)` → `TwilioMailConfigurationSection` + typed `HttpClient` as `IMailService`

Mail configuration classes form a hierarchy: `MailConfigurationSection` (abstraction) holds only what every provider shares (`From`, `DisplayName`); `SmtpMailConfigurationSection` and `TwilioMailConfigurationSection` live in their own packages and add the infrastructure settings.

Default config section names must contain **no dots** and no `NuvTools` prefix — a dotted key cannot be overridden by an environment variable on Linux/Azure App Service (`MailTwilio__AuthToken` works, `NuvTools.Notification.Mail.Twilio__AuthToken` does not). Current names: `"Mail"`, `"MailSmtp"`, `"MailTwilio"`, `"Messaging"`. Realtime has no configuration section.

## Project Settings

- **Target Frameworks**: `net8;net9;net10.0`
- **Assembly Signing**: All projects use `.snk` strong name signing
- **Nullable Reference Types**: Enabled
- **Implicit Usings**: Enabled
- **XML Documentation**: Generated for all public APIs
- **Code Analysis**: `EnforceCodeStyleInBuild` enabled
