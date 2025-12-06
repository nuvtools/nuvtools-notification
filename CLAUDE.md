# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a modular .NET library framework for notification and messaging scenarios. It follows an **abstraction-implementation pattern** where each notification domain has an abstraction library and one or more infrastructure/implementation libraries.

## Architecture Pattern

The codebase follows a consistent pattern across all notification domains:

1. **Abstraction Libraries** (e.g., `NuvTools.Notification.Messaging`, `NuvTools.Notification.Mail`, `NuvTools.Notification.Realtime`)
   - Define interfaces and base types
   - Contain no infrastructure dependencies
   - Provide configuration abstractions

2. **Implementation Libraries** (e.g., `NuvTools.Notification.Messaging.Azure.ServiceBus`, `NuvTools.Notification.Mail.Smtp`)
   - Implement the abstractions from their corresponding abstraction library
   - Contain infrastructure-specific dependencies (Azure SDK, SMTP, SignalR)
   - Inherit from abstract base classes or implement interfaces

### Key Abstraction Interfaces

- **Messaging**: `IMessageSender<TBody>` and `IMessageConsumer<TBody>` - generic message sending and consuming
- **Mail**: `IMailService` - email sending abstraction
- **Realtime**: `IMessageSender<T>` - real-time message sending

### Important Implementation Details

- **Azure Service Bus Receiver**: Inherits from `BackgroundService` and uses `ServiceBusProcessor`. It handles deserialization, consumer dispatching, error handling, and automatic processor restart on failure. Message completion is managed through `IMessageContext` to avoid double-completion.

- **Azure Service Bus Sender**: Abstract base class that serializes messages to JSON and sends via `ServiceBusSender`. Supports initialization from `ServiceBusClient`, connection string, or `MessagingSection`.

## Build and Development Commands

### Build
```bash
dotnet build NuvTools.Notification.slnx
```

### Build specific project
```bash
dotnet build src/NuvTools.Notification.Messaging/NuvTools.Notification.Messaging.csproj
```

### Restore packages
```bash
dotnet restore NuvTools.Notification.slnx
```

### Create NuGet packages (all libraries have `GeneratePackageOnBuild` enabled)
```bash
dotnet build -c Release NuvTools.Notification.slnx
```

## Project Structure

- **Target Frameworks**: All libraries target `net8`, `net9`, and `net10.0`
- **Assembly Signing**: All projects use strong name signing (`.snk` files)
- **Documentation**: All projects generate XML documentation files (`GenerateDocumentationFile`)
- **Code Analysis**: All projects enforce code style in build (`EnforceCodeStyleInBuild`)
- **Nullable Reference Types**: Enabled across all projects
- **Implicit Usings**: Enabled across all projects

## Library Dependencies

Each abstraction library is standalone or depends only on Microsoft.Extensions configuration packages. Implementation libraries reference their corresponding abstraction library and infrastructure-specific SDKs:

- **Azure.ServiceBus**: Used by `NuvTools.Notification.Messaging.Azure.ServiceBus`
- **Azure.SignalR**: Used by `NuvTools.Notification.Realtime.Azure.SignalR`
- **Microsoft.AspNetCore.SignalR.Client**: Used by `NuvTools.Notification.Realtime.Azure.SignalR.Client`

## Configuration Pattern

All libraries use a consistent configuration pattern with:
- `MessagingSection` or similar configuration classes
- `ServiceCollectionExtensions` for dependency injection registration
- Support for both direct instantiation and DI container registration

## Naming Conventions

- Abstraction libraries: `NuvTools.Notification.{Domain}`
- Implementation libraries: `NuvTools.Notification.{Domain}.{Infrastructure}.{Optional.Subcategory}`
- Example: `NuvTools.Notification.Realtime.Azure.SignalR.Client`
