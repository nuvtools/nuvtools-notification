# Nuv Tools Notification Libraries

A set of modular .NET libraries to support notification and messaging scenarios, including email, push, SMS, real-time, and cloud-based messaging.

## Libraries Overview

### 1. NuvTools.Notification.Messaging
Abstraction library for building messaging solutions in .NET applications. Provides interfaces and base components to support integration with various messaging services and protocols, enabling a consistent approach to sending and receiving messages.

### 2. NuvTools.Notification.Messaging.Azure.ServiceBus
Infrastructure library for integrating Azure Service Bus messaging into .NET applications. Implements the abstractions from `NuvTools.Notification.Messaging` to provide scalable and reliable cloud-based communication using Azure Service Bus queues and topics.

### 3. NuvTools.Notification.Mail
Library focused on email notification scenarios. Defines abstractions and helpers for composing and sending emails in a consistent way across different providers.

### 4. NuvTools.Notification.Mail.Smtp
Infrastructure library that implements the abstractions from `NuvTools.Notification.Mail` using the SMTP protocol. Enables sending emails via standard SMTP servers.

### 5. NuvTools.Notification.Realtime
Abstraction library for building real-time notification and messaging solutions in .NET applications. Provides interfaces and base components to support integration with various real-time protocols and services.

### 6. NuvTools.Notification.Realtime.Azure.SignalR
Infrastructure library for integrating Azure SignalR real-time messaging into .NET applications. Implements abstractions for scalable, cloud-based notifications and live communication using SignalR.

### 7. NuvTools.Notification.Realtime.Azure.SignalR.Client
Client library for integrating Azure SignalR real-time messaging into .NET applications. Provides components for connecting, sending, and receiving live notifications using SignalR protocols.

## Typical Usage

- For messaging scenarios (queues, topics, distributed communication), use:
  - `NuvTools.Notification.Messaging` (abstractions)
  - `NuvTools.Notification.Messaging.Azure.ServiceBus` (Azure Service Bus implementation)

- For email notifications, use:
  - `NuvTools.Notification.Mail` (abstractions)
  - `NuvTools.Notification.Mail.Smtp` (SMTP implementation)

- For real-time notifications, use:
  - `NuvTools.Notification.Realtime` (abstractions)
  - `NuvTools.Notification.Realtime.Azure.SignalR` (Azure SignalR server implementation)
  - `NuvTools.Notification.Realtime.Azure.SignalR.Client` (Azure SignalR client implementation)

## Target Frameworks

All libraries target `.NET 8` and `.NET 9` for modern, high-performance applications.

---

For more details, see the documentation in each library or visit [nuvtools.com](https://nuvtools.com).