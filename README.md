# Nuv Tools Notification Libraries

A set of modular .NET libraries to support notification and messaging scenarios, including email, push, SMS, and cloud-based messaging.

## Libraries Overview

### 1. NuvTools.Notification.Messaging
An abstraction library for building messaging solutions in .NET applications. It provides interfaces and base components to support integration with various messaging services and protocols, enabling a consistent approach to sending and receiving messages.

### 2. NuvTools.Notification.Messaging.Azure.ServiceBus
Infrastructure library for integrating Azure Service Bus messaging into .NET applications. It implements the abstractions from `NuvTools.Notification.Messaging` to provide scalable and reliable cloud-based communication using Azure Service Bus queues and topics.

### 3. NuvTools.Notification.Mail
A library focused on email notification scenarios. It defines abstractions and helpers for composing and sending emails in a consistent way across different providers.

### 4. NuvTools.Notification.Mail.Smtp
Infrastructure library that implements the abstractions from `NuvTools.Notification.Mail` using the SMTP protocol. It enables sending emails via standard SMTP servers.

## Typical Usage

- For messaging scenarios (queues, topics, distributed communication), use:
  - `NuvTools.Notification.Messaging` (abstractions)
  - `NuvTools.Notification.Messaging.Azure.ServiceBus` (Azure Service Bus implementation)

- For email notifications, use:
  - `NuvTools.Notification.Mail` (abstractions)
  - `NuvTools.Notification.Mail.Smtp` (SMTP implementation)

## Target Frameworks

All libraries target `.NET 8` and `.NET 9` for modern, high-performance applications.

---

For more details, see the documentation in each library or visit [nuvtools.com](https://nuvtools.com).