# Messaging

Cloud-based messaging libraries with Azure Service Bus support, including session-enabled queues and scheduled delivery.

## Libraries

| Library | Type | Description |
|---------|------|-------------|
| **NuvTools.Notification.Messaging** | Abstraction | Core messaging interfaces and models: `IMessageSender<TBody>`, `IMessageConsumer<TBody>`, `IMessageContext`. Provides `Message<T>` envelope with metadata (MessageId, CorrelationId, Subject, SessionId, TimeToLive, ScheduledEnqueueTime) and a flexible `Properties` dictionary for custom headers. |
| **NuvTools.Notification.Messaging.Azure.ServiceBus** | Implementation | Azure Service Bus provider with `AzureServiceBusSender<TBody>` (abstract sender for queues/topics), `AzureServiceBusReceiver<TBody, TConsumer>` (standard parallel processing), `AzureServiceBusSessionReceiver<TBody, TConsumer>` (session-enabled sequential per-SessionId). Includes automatic JSON deserialization, dead-lettering, exponential backoff retry, scheduled delivery, and message TTL support. |

## Installation

```bash
dotnet add package NuvTools.Notification.Messaging
dotnet add package NuvTools.Notification.Messaging.Azure.ServiceBus
```

## Quick Start

### Sending Messages

```csharp
// Define your sender
public class OrderSender(IOptions<MessagingSection> options)
    : AzureServiceBusSender<Order>(options.Value);

// Send a message
var message = new Message<Order>(order)
{
    Subject = "order.created",
    CorrelationId = correlationId
};
await orderSender.SendAsync(message, cancellationToken);
```

### Sending Scheduled Messages

```csharp
// Schedule a message for future delivery
var message = new Message<Reminder>(reminder)
{
    Subject = "reminder.scheduled",
    ScheduledEnqueueTime = DateTimeOffset.UtcNow.AddHours(1),
    TimeToLive = TimeSpan.FromDays(1)
};
await reminderSender.SendAsync(message, cancellationToken);
```

### Receiving Messages

```csharp
// Define your consumer
public class OrderConsumer : IMessageConsumer<Order>
{
    public async Task ConsumeAsync(Message<Order> message, IMessageContext context,
        CancellationToken cancellationToken)
    {
        await ProcessOrder(message.Body);
        await context.CompleteAsync(cancellationToken);
    }
}

// Define your background receiver
public class OrderReceiver(
    ILogger<OrderReceiver> logger,
    IServiceProvider serviceProvider,
    IOptions<MessagingSection> options)
    : AzureServiceBusReceiver<Order, OrderConsumer>(logger, serviceProvider, options.Value);
```

### Session-Enabled Receiver

For ordered processing where messages with the same `SessionId` must be processed sequentially:

```csharp
// Send messages with a session ID
var message = new Message<OrderEvent>(orderEvent)
{
    SessionId = orderId.ToString(),
    Subject = "order.updated"
};
await sender.SendAsync(message, cancellationToken);

// Define a session-aware receiver
public class OrderEventReceiver(
    ILogger<OrderEventReceiver> logger,
    IServiceProvider serviceProvider,
    IOptions<MessagingSection> options)
    : AzureServiceBusSessionReceiver<OrderEvent, OrderEventConsumer>(
        logger, serviceProvider, options.Value);
```

## Usage Examples

### Complete Azure Service Bus Setup

```json
// appsettings.json
{
  "NuvTools.Notification.Messaging": {
    "Name": "orders-queue",
    "ConnectionString": "Endpoint=sb://...",
    "MaxConcurrentCalls": 10,
    "MaxAutoLockRenewalDuration": "00:30:00",
    "AutoCompleteMessages": false,
    "RequiresSession": false
  }
}
```

```csharp
// Program.cs
services.AddMessagingQueueConfiguration<MessagingSection>(configuration);
services.AddSingleton<OrderSender>();
services.AddScoped<OrderConsumer>();
services.AddHostedService<OrderReceiver>();
```

### Session-Enabled Queue Setup

```json
{
  "NuvTools.Notification.Messaging": {
    "Name": "order-events-queue",
    "ConnectionString": "Endpoint=sb://...",
    "MaxConcurrentCalls": 5,
    "RequiresSession": true
  }
}
```

```csharp
services.AddHostedService<OrderEventReceiver>();  // Uses AzureServiceBusSessionReceiver
```

When `RequiresSession` is `true`, use `AzureServiceBusSessionReceiver` which enforces `MaxConcurrentCallsPerSession = 1` to guarantee sequential processing per session, while processing up to `MaxConcurrentCalls` sessions in parallel.

### Message Error Handling

```csharp
public class ResilientOrderConsumer : IMessageConsumer<Order>
{
    public async Task ConsumeAsync(Message<Order> message, IMessageContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            await ProcessOrder(message.Body);
            await context.CompleteAsync(cancellationToken);
        }
        catch (ValidationException ex)
        {
            // Invalid message - send to dead-letter queue
            await context.DeadLetterAsync("ValidationFailed", ex.Message, cancellationToken);
        }
        catch (Exception)
        {
            // Transient error - abandon for retry
            await context.AbandonAsync(cancellationToken);
        }
    }
}
```

### Using Custom Message Properties

```csharp
var message = new Message<Order>(order)
{
    Subject = "order.created",
    Properties =
    {
        ["priority"] = "high",
        ["source"] = "web-api",
        ["tenant-id"] = tenantId
    }
};
await sender.SendAsync(message, cancellationToken);
```

Properties are mapped to `ApplicationProperties` on Azure Service Bus messages and are available in the consumer via `message.Properties`.

## Configuration

### appsettings.json

```json
{
  "NuvTools.Notification.Messaging": {
    "Name": "my-queue",
    "SubscriptionName": null,
    "ConnectionString": "Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=...",
    "MaxAutoLockRenewalDuration": "00:30:00",
    "MaxConcurrentCalls": 10,
    "AutoCompleteMessages": false,
    "RequiresSession": false
  }
}
```

### Configuration Properties

| Property | Default | Description |
|----------|---------|-------------|
| `Name` | *(required)* | Queue or topic name |
| `SubscriptionName` | `null` | Subscription name (for topic/subscription scenarios) |
| `ConnectionString` | *(required)* | Azure Service Bus connection string |
| `MaxAutoLockRenewalDuration` | `00:30:00` | How long to automatically renew the message lock |
| `MaxConcurrentCalls` | `10` | Max parallel message handlers (or max concurrent sessions) |
| `AutoCompleteMessages` | `false` | Auto-complete messages after handler returns |
| `RequiresSession` | `false` | Enable session support for ordered processing |

### Dependency Injection Setup

```csharp
services.AddMessagingQueueConfiguration<MessagingSection>(configuration);
```

## Architecture

### Receiver Hierarchy

```
BackgroundService
  +-- AzureServiceBusReceiverBase<TBody, TConsumer>    (shared processing, error handling, retry)
       |-- AzureServiceBusReceiver<TBody, TConsumer>    (standard parallel processing)
       +-- AzureServiceBusSessionReceiver<TBody, TConsumer> (sequential per-SessionId)
```

Both `AzureServiceBusReceiver` and `AzureServiceBusSessionReceiver` inherit from a shared `AzureServiceBusReceiverBase` which provides:

- **Message processing**: JSON deserialization, consumer dispatch via scoped DI, and lifecycle management
- **Error handling**: Dead-lettering for deserialization failures, abandon for transient errors, warning on message lock loss
- **Resilience**: Automatic processor restart with exponential backoff (3 retries at 5s, 10s, 20s intervals)
- **Graceful shutdown**: Proper stop and dispose of processor and client resources

Consumers are resolved from a new DI scope per message, enabling transactional processing and proper resource cleanup.
