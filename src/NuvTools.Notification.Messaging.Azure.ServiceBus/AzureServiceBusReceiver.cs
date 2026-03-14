using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using NuvTools.Notification.Messaging.Configuration;
using NuvTools.Notification.Messaging.Interfaces;

namespace NuvTools.Notification.Messaging.Azure.ServiceBus;

/// <summary>
///     Background service for receiving and processing messages from Azure Service Bus queues or topic/subscriptions.
///     <typeparam name="TBody">The type of the message body to deserialize and process.</typeparam>
///     <typeparam name="TConsumer">
///         The consumer type that implements <see cref="IMessageConsumer{TBody}"/> and handles the message.
///     </typeparam>
/// </summary>
public abstract class AzureServiceBusReceiver<TBody, TConsumer>(
    ILogger logger,
    IServiceProvider serviceProvider,
    MessagingSection messagingSection)
    : AzureServiceBusReceiverBase<TBody, TConsumer>(logger, serviceProvider, messagingSection)
    where TBody : class
    where TConsumer : IMessageConsumer<TBody>
{
    private readonly ServiceBusProcessor _processor = CreateProcessor(messagingSection);

    private static ServiceBusProcessor CreateProcessor(MessagingSection section)
    {
        var client = new ServiceBusClient(section.ConnectionString);
        var options = new ServiceBusProcessorOptions
        {
            MaxAutoLockRenewalDuration = section.MaxAutoLockRenewalDuration,
            MaxConcurrentCalls = section.MaxConcurrentCalls,
            AutoCompleteMessages = section.AutoCompleteMessages
        };

        return string.IsNullOrEmpty(section.SubscriptionName)
            ? client.CreateProcessor(section.Name, options)
            : client.CreateProcessor(section.Name, section.SubscriptionName!, options);
    }

    protected override bool IsProcessing => _processor.IsProcessing;

    protected override void RegisterHandlers()
    {
        _processor.ProcessMessageAsync += HandleMessage;
        _processor.ProcessErrorAsync += HandleError;
    }

    protected override Task StartProcessorAsync(CancellationToken cancellationToken)
        => _processor.StartProcessingAsync(cancellationToken);

    protected override Task StopProcessorAsync(CancellationToken cancellationToken)
        => _processor.StopProcessingAsync(cancellationToken);

    protected override Task DisposeProcessorAsync()
        => _processor.DisposeAsync().AsTask();

    protected override Task RestartProcessorWithRetry(CancellationToken cancellationToken)
        => RestartWithRetry(_processor.StartProcessingAsync, cancellationToken);

    private Task HandleMessage(ProcessMessageEventArgs args)
        => ProcessReceivedMessageAsync(
            args.Message,
            sessionId: null,
            ct => args.CompleteMessageAsync(args.Message, ct),
            ct => args.AbandonMessageAsync(args.Message, cancellationToken: ct),
            (reason, description, ct) => args.DeadLetterMessageAsync(args.Message, reason, description, ct),
            args.CancellationToken);
}
