using Azure.Messaging.ServiceBus;
using NuvTools.Common.ResultWrapper;
using NuvTools.Notification.Messaging.Configuration;
using NuvTools.Notification.Messaging.Interfaces;
using System.Text.Json;

namespace NuvTools.Notification.Messaging.Azure.ServiceBus;

public abstract class AzureServiceBusSender<T> : IMessageSender<T> where T : Message<T>
{
    private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly ServiceBusSender _sender;

    protected AzureServiceBusSender(ServiceBusClient client, string queueName)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrEmpty(queueName);

        _sender = client.CreateSender(queueName);
    }

    protected AzureServiceBusSender(string connectionString, string queueName)
        : this(new ServiceBusClient(connectionString), queueName)
    {
    }

    protected AzureServiceBusSender(MessagingQueueSection messagingQueueSection)
        : this(messagingQueueSection.ConnectionString, messagingQueueSection.Name)
    {
    }

    public async Task<IResult> SendAsync(T message, CancellationToken cancellationToken = default)
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

        return Result.Success("Message sent successfully");
    }
}