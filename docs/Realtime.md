# Realtime

Real-time notification libraries using Azure SignalR for live broadcasting and receiving.

## Libraries

| Library | Type | Description |
|---------|------|-------------|
| **NuvTools.Notification.Realtime** | Abstraction | `IMessageSender<T>` abstraction for broadcasting messages. |
| **NuvTools.Notification.Realtime.Azure.SignalR** | Implementation | Server-side SignalR implementation. `AzureSignalRSender<T>` broadcasts to all connected clients using the method name convention `"Consume_{TypeName}"`. |
| **NuvTools.Notification.Realtime.Azure.SignalR.Client** | Implementation | Client-side `AzureSignalRReceiver<T>` with built-in debouncing (default 1000ms) and automatic reconnection. |

## Installation

```bash
dotnet add package NuvTools.Notification.Realtime
dotnet add package NuvTools.Notification.Realtime.Azure.SignalR
dotnet add package NuvTools.Notification.Realtime.Azure.SignalR.Client
```

## Quick Start

### Server-side Sending

```csharp
var notification = new UserNotification { Message = "New order received", UserId = userId };
await signalRSender.SendAsync(notification, cancellationToken);
```

### Client-side Receiving

```csharp
var receiver = new AzureSignalRReceiver<UserNotification>("https://yourapp.com/hub-endpoint");
receiver.MessageReceived += (notification, token) =>
{
    Console.WriteLine($"Received: {notification.Message}");
};
await receiver.ConnectAsync();
```

## Architecture

### SignalR Method Convention

`AzureSignalRSender<T>` broadcasts messages using the method name `"Consume_{typeof(T).Name}"`. Clients (including `AzureSignalRReceiver<T>`) must register handlers for the same method name. The client receiver includes configurable debouncing to prevent rapid-fire event flooding.

### Dependency Injection Setup

```csharp
// Configure SignalR
services.AddSignalR();
services.AddSingleton<IMessageSender<Notification>, AzureSignalRSender<Notification>>();

// Map hub endpoint
app.MapHub<SignalRHub>("/hub-endpoint");
```
