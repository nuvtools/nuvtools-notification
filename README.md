# NuvTools Notification Libraries

A comprehensive set of modular .NET libraries for notification and messaging scenarios, including email, real-time communication, and cloud-based messaging with Azure Service Bus and SignalR.

## Table of Contents

- [Overview](#overview)
- [Libraries](#libraries)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Usage Examples](#usage-examples)
- [Configuration](#configuration)
- [Architecture](#architecture)
- [Building and Development](#building-and-development)
- [Features](#features)

## Overview

NuvTools Notification Libraries provide a clean abstraction layer over various notification systems, allowing you to write code against interfaces and swap implementations without changing your business logic. The libraries follow the **abstraction-implementation pattern**, where core abstractions are separate from infrastructure-specific implementations.

## Libraries

| Library | Type | Purpose |
|---------|------|---------|
| **NuvTools.Notification.Messaging** | Abstraction | Core messaging interfaces and models for queues and topics |
| **NuvTools.Notification.Messaging.Azure.ServiceBus** | Implementation | Azure Service Bus provider for reliable cloud messaging |
| **NuvTools.Notification.Mail** | Abstraction | Email notification abstractions with support for HTML and attachments |
| **NuvTools.Notification.Mail.Smtp** | Implementation | SMTP provider using MailKit for email delivery |
| **NuvTools.Notification.Realtime** | Abstraction | Real-time messaging abstractions for live notifications |
| **NuvTools.Notification.Realtime.Azure.SignalR** | Implementation | Azure SignalR server-side implementation |
| **NuvTools.Notification.Realtime.Azure.SignalR.Client** | Implementation | Azure SignalR client for receiving real-time messages |

### Library Descriptions

#### Messaging Libraries

**NuvTools.Notification.Messaging**
- Core abstractions: `IMessageSender<TBody>`, `IMessageConsumer<TBody>`, `IMessageContext`
- Provides `Message<T>` envelope with metadata (MessageId, CorrelationId, Subject, TimeToLive)
- Transport-agnostic design for maximum flexibility

**NuvTools.Notification.Messaging.Azure.ServiceBus**
- Implements messaging abstractions using Azure Service Bus
- Background service (`AzureServiceBusReceiver<TBody, TConsumer>`) for consuming messages
- Automatic deserialization, error handling, and dead-lettering
- Support for both queues and topics/subscriptions

#### Mail Libraries

**NuvTools.Notification.Mail**
- `IMailService` abstraction for sending emails
- `MailMessage`, `MailAddress`, and `MailPart` models
- Support for HTML content and file attachments

**NuvTools.Notification.Mail.Smtp**
- SMTP implementation using MailKit
- Configurable via `MailConfigurationSection`
- Supports authentication, SSL/TLS, and attachments

#### Real-time Libraries

**NuvTools.Notification.Realtime**
- `IMessageSender<T>` abstraction for broadcasting messages

**NuvTools.Notification.Realtime.Azure.SignalR**
- Server-side SignalR implementation (`AzureSignalRSender<T>`)
- Broadcasts to all connected clients
- Requires `SignalRHub` registration

**NuvTools.Notification.Realtime.Azure.SignalR.Client**
- Client-side SignalR receiver (`AzureSignalRReceiver<T>`)
- Built-in debouncing to prevent rapid-fire events
- Automatic reconnection support

## Installation

Install the packages you need via NuGet:

### Messaging
```bash
dotnet add package NuvTools.Notification.Messaging
dotnet add package NuvTools.Notification.Messaging.Azure.ServiceBus
```

### Email
```bash
dotnet add package NuvTools.Notification.Mail
dotnet add package NuvTools.Notification.Mail.Smtp
```

### Real-time
```bash
dotnet add package NuvTools.Notification.Realtime
dotnet add package NuvTools.Notification.Realtime.Azure.SignalR
dotnet add package NuvTools.Notification.Realtime.Azure.SignalR.Client
```

## Quick Start

### Sending Messages with Azure Service Bus

```csharp
// Define your sender
public class OrderSender : AzureServiceBusSender<Order>
{
    public OrderSender(IOptions<MessagingSection> options)
        : base(options.Value) { }
}

// Send a message
var message = new Message<Order>(order)
{
    Subject = "order.created",
    CorrelationId = correlationId
};
await orderSender.SendAsync(message, cancellationToken);
```

### Receiving Messages with Azure Service Bus

```csharp
// Define your consumer
public class OrderConsumer : IMessageConsumer<Order>
{
    public async Task ConsumeAsync(Message<Order> message, IMessageContext context, CancellationToken cancellationToken)
    {
        // Process the order
        await ProcessOrder(message.Body);

        // Complete the message
        await context.CompleteAsync(cancellationToken);
    }
}

// Define your background receiver
public class OrderReceiver : AzureServiceBusReceiver<Order, OrderConsumer>
{
    public OrderReceiver(ILogger<OrderReceiver> logger, IServiceProvider serviceProvider, IOptions<MessagingSection> options)
        : base(logger, serviceProvider, options.Value) { }
}
```

### Sending Emails via SMTP

```csharp
var mailMessage = new MailMessage
{
    From = new MailAddress { Address = "sender@example.com", DisplayName = "Support Team" },
    To = new List<MailAddress> { new() { Address = "user@example.com", DisplayName = "John Doe" } },
    Subject = "Welcome!",
    Body = "<h1>Welcome to our service!</h1>"
};

await mailService.SendAsync(mailMessage);
```

### Real-time Notifications with SignalR

**Server-side:**
```csharp
// Broadcast a notification
var notification = new UserNotification { Message = "New order received", UserId = userId };
await signalRSender.SendAsync(notification, cancellationToken);
```

**Client-side:**
```csharp
var receiver = new AzureSignalRReceiver<UserNotification>("https://yourapp.com/notificationHub");
receiver.MessageReceived += (notification, token) =>
{
    Console.WriteLine($"Received: {notification.Message}");
};
await receiver.ConnectAsync();
```

## Usage Examples

### Complete Azure Service Bus Setup

```csharp
// appsettings.json
{
  "NuvTools.Notification.Messaging": {
    "Name": "orders-queue",
    "ConnectionString": "Endpoint=sb://...",
    "MaxConcurrentCalls": 10,
    "MaxAutoLockRenewalDuration": "00:30:00",
    "AutoCompleteMessages": false
  }
}

// Program.cs
services.AddMessagingQueueConfiguration<MessagingSection>(configuration);
services.AddSingleton<OrderSender>();
services.AddScoped<OrderConsumer>();
services.AddHostedService<OrderReceiver>();
```

### Email with Attachments

```csharp
using var fileStream = File.OpenRead("document.pdf");
var mailMessage = new MailMessage
{
    From = new MailAddress { Address = "noreply@company.com" },
    To = new List<MailAddress> { new() { Address = "customer@example.com" } },
    Subject = "Your Invoice",
    Body = "<p>Please find your invoice attached.</p>",
    Parts = new List<MailPart>
    {
        new()
        {
            MediaType = "application",
            MediaExtension = "pdf",
            Content = fileStream
        }
    }
};

await mailService.SendAsync(mailMessage);
```

### Message Error Handling

```csharp
public class ResilientOrderConsumer : IMessageConsumer<Order>
{
    public async Task ConsumeAsync(Message<Order> message, IMessageContext context, CancellationToken cancellationToken)
    {
        try
        {
            await ProcessOrder(message.Body);
            await context.CompleteAsync(cancellationToken);
        }
        catch (ValidationException ex)
        {
            // Invalid message - send to dead letter queue
            await context.DeadLetterAsync("ValidationFailed", ex.Message, cancellationToken);
        }
        catch (Exception ex)
        {
            // Transient error - abandon for retry
            await context.AbandonAsync(cancellationToken);
        }
    }
}
```

## Configuration

### Email Configuration (appsettings.json)

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

### Messaging Configuration (appsettings.json)

```json
{
  "NuvTools.Notification.Messaging": {
    "Name": "my-queue",
    "SubscriptionName": null,
    "ConnectionString": "Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=...",
    "MaxAutoLockRenewalDuration": "00:30:00",
    "MaxConcurrentCalls": 10,
    "AutoCompleteMessages": false
  }
}
```

### Dependency Injection Setup

```csharp
// Configure mail
services.AddMailConfiguration(configuration);
services.AddSingleton<IMailService, SMTPMailService>();

// Configure messaging
services.AddMessagingQueueConfiguration<MessagingSection>(configuration);

// Configure SignalR
services.AddSignalR();
services.AddSingleton<IMessageSender<Notification>, AzureSignalRSender<Notification>>();

// In your app configuration
app.MapHub<SignalRHub>("/notificationHub");
```

## Architecture

### Design Principles

1. **Separation of Concerns**: Abstractions are separate from implementations
2. **Dependency Inversion**: Depend on interfaces, not concrete implementations
3. **Provider Pattern**: Easily swap implementations (e.g., SMTP → SendGrid)
4. **Generic Message Envelope**: Type-safe message bodies with common metadata
5. **Background Processing**: Built-in hosted services for message consumers

### Key Patterns

- **Abstraction Libraries**: Define interfaces and models (`IMessageSender`, `IMailService`)
- **Implementation Libraries**: Provide concrete implementations (`AzureServiceBusSender`, `SMTPMailService`)
- **Message Envelope**: `Message<T>` wraps your payload with common properties
- **Consumer Pattern**: `IMessageConsumer<TBody>` handles incoming messages
- **Context Pattern**: `IMessageContext` manages message lifecycle (complete, abandon, dead-letter)

## Building and Development

### Prerequisites
- .NET 8 SDK or later (for .NET 8, 9, and 10 support)
- Visual Studio 2022 17.4+ or JetBrains Rider 2023.1+ (for .slnx support)
- Azure Service Bus namespace (for messaging libraries)
- SMTP server or credentials (for email libraries)

### Build Commands

Build the entire solution:
```bash
dotnet build NuvTools.Notification.slnx
```

Build in Release mode and generate NuGet packages:
```bash
dotnet build -c Release NuvTools.Notification.slnx
```

Restore packages:
```bash
dotnet restore NuvTools.Notification.slnx
```

Build a specific project:
```bash
dotnet build src/NuvTools.Notification.Messaging/NuvTools.Notification.Messaging.csproj
```

Clean build artifacts:
```bash
dotnet clean NuvTools.Notification.slnx
```

### Solution Format

This project uses the modern `.slnx` (XML-based solution) format introduced in Visual Studio 2022, which provides:
- Better merge conflict resolution in version control
- More readable diffs
- Cleaner XML structure
- Full compatibility with `dotnet` CLI

## Features

### Core Features
- **Strong-named assemblies**: All libraries are signed with strong names for enhanced security
- **XML documentation**: Comprehensive inline documentation for all public APIs
- **NuGet packaging**: Automatic package generation on build with proper metadata
- **Multi-targeting**: Support for .NET 8, 9, and 10
- **Nullable reference types**: Full nullable annotation for better null safety
- **Implicit usings**: Cleaner code with global using directives

### Messaging Features
- **Automatic deserialization**: JSON deserialization with error handling
- **Dead-letter queue support**: Automatic routing of failed messages
- **Message context**: Explicit control over message completion
- **Retry logic**: Built-in abandonment for transient failures
- **Concurrent processing**: Configurable parallel message processing
- **Auto-lock renewal**: Automatic message lock extension

### Email Features
- **HTML email support**: Rich HTML content with inline styles
- **File attachments**: Support for multiple attachments with streams
- **Multiple recipients**: Send to multiple addresses in one call
- **Configurable SMTP**: Full SMTP configuration via appsettings.json
- **Fallback sender**: Default sender address from configuration

### Real-time Features
- **Broadcast messaging**: Send to all connected clients
- **Type-safe messages**: Generic message types for compile-time safety
- **Debouncing**: Built-in debouncing to prevent event flooding
- **Auto-reconnection**: Automatic reconnection on connection loss
- **Event-based API**: Simple event subscription model

## Target Frameworks

All libraries target `.NET 8`, `.NET 9`, and `.NET 10` for modern, high-performance applications.

## Contributing

Contributions are welcome! Please ensure:
1. All code follows existing patterns and conventions
2. XML documentation is provided for all public APIs
3. Code builds without warnings

## License

Copyright © 2025 Nuv Tools. All rights reserved.

## Resources

- **Website**: [nuvtools.com](https://nuvtools.com)
- **Documentation**: See XML documentation in each library
- **NuGet Packages**: Available on NuGet.org