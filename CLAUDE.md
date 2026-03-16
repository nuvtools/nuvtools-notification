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
| **Mail** | `NuvTools.Notification.Mail` | `NuvTools.Notification.Mail.Smtp` (MailKit) |
| **Realtime** | `NuvTools.Notification.Realtime` | `NuvTools.Notification.Realtime.Azure.SignalR` (server), `NuvTools.Notification.Realtime.Azure.SignalR.Client` |

### Key Interfaces

- **Messaging sender**: `IMessageSender<TBody>` — `SendAsync(Message<TBody>, CancellationToken)`
- **Messaging consumer**: `IMessageConsumer<TBody>` — `ConsumeAsync(Message<TBody>, IMessageContext, CancellationToken)`
- **Message lifecycle**: `IMessageContext` — `CompleteAsync`, `AbandonAsync`, `DeadLetterAsync` (explicit acknowledgment; `AutoCompleteMessages` defaults to `false`)
- **Mail**: `IMailService` — `SendAsync(MailMessage)`
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

### SignalR Convention

`AzureSignalRSender<T>` broadcasts via `hubContext.Clients.All` using method name `"Consume_{typeof(T).Name}"`. `AzureSignalRReceiver<T>` registers a handler for the same method name and applies debouncing (default 1000ms) to prevent event flooding.

### Serialization

All message serialization uses `System.Text.Json` with `JsonSerializerDefaults.Web` (camelCase property names).

## Configuration Pattern

All libraries use `IOptions<TSection>` with typed configuration classes registered via `ServiceCollectionExtensions`:
- `AddMessagingQueueConfiguration<T>(services, configuration, sectionName)` → `MessagingSection`
- `AddMailConfiguration<T>(services, configuration, sectionName)` → `MailConfigurationSection`

Default config section names match the namespace (e.g., `"NuvTools.Notification.Messaging"`).

## Project Settings

- **Target Frameworks**: `net8;net9;net10.0`
- **Assembly Signing**: All projects use `.snk` strong name signing
- **Nullable Reference Types**: Enabled
- **Implicit Usings**: Enabled
- **XML Documentation**: Generated for all public APIs
- **Code Analysis**: `EnforceCodeStyleInBuild` enabled
