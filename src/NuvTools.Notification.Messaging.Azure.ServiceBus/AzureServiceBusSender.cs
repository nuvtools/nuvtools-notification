using Azure.Messaging.ServiceBus;
using NuvTools.Notification.Messaging.Configuration;
using NuvTools.Notification.Messaging.Interfaces;
using System.Text.Json;

namespace NuvTools.Notification.Messaging.Azure.ServiceBus;

/// <summary>
/// Provides a base implementation for sending messages to Azure Service Bus entities (queues or topics).
/// </summary>
/// <typeparam name="TBody">
/// The type of the message body. Must be a reference type.
/// </typeparam>
/// <remarks>
/// This abstract class encapsulates the logic for serializing messages and sending them to Azure Service Bus.
/// It supports initialization using a <see cref="ServiceBusClient"/>, a connection string, or a <see cref="MessagingSection"/> configuration.
/// </remarks>
public abstract class AzureServiceBusSender<TBody> : IMessageSender<TBody> where TBody : class
{
    private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly ServiceBusSender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureServiceBusSender{TBody}"/> class using an existing <see cref="ServiceBusClient"/>.
    /// </summary>
    /// <param name="client">The Azure Service Bus client.</param>
    /// <param name="entityName">The name of the queue or topic to send messages to.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="client"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="entityName"/> is null or empty.</exception>
    protected AzureServiceBusSender(ServiceBusClient client, string entityName)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrEmpty(entityName);

        _sender = client.CreateSender(entityName);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureServiceBusSender{TBody}"/> class using a connection string.
    /// </summary>
    /// <param name="connectionString">The Azure Service Bus connection string.</param>
    /// <param name="entityName">The name of the queue or topic to send messages to.</param>
    protected AzureServiceBusSender(string connectionString, string entityName)
        : this(new ServiceBusClient(connectionString), entityName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureServiceBusSender{TBody}"/> class using a <see cref="MessagingSection"/> configuration.
    /// </summary>
    /// <param name="messagingSection">The messaging configuration section containing connection details.</param>
    protected AzureServiceBusSender(MessagingSection messagingSection)
        : this(messagingSection.ConnectionString, messagingSection.Name)
    {
    }

    /// <summary>
    /// Sends a message asynchronously to the configured Azure Service Bus entity.
    /// </summary>
    /// <param name="message">The message to send, including body and metadata.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous send operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
    public async Task SendAsync(Message<TBody> message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        string jsonBody = JsonSerializer.Serialize(message.Body, DefaultJsonSerializerOptions);
        var sbMessage = new ServiceBusMessage(jsonBody)
        {
            MessageId = message.MessageId,
            ContentType = "application/json",
            Subject = message.Subject,
            CorrelationId = message.CorrelationId
        };

        if (message.TimeToLive is not null)
            sbMessage.TimeToLive = message.TimeToLive.Value;

        foreach (var kvp in message.Properties)
            sbMessage.ApplicationProperties.TryAdd(kvp.Key, kvp.Value);

        await _sender.SendMessageAsync(sbMessage, cancellationToken);
    }
}