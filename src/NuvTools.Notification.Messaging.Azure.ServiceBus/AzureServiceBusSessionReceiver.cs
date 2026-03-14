using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using NuvTools.Notification.Messaging.Configuration;
using NuvTools.Notification.Messaging.Interfaces;

namespace NuvTools.Notification.Messaging.Azure.ServiceBus;

/// <summary>
///     Background service for receiving and processing messages from session-enabled Azure Service Bus queues.
///     <para>
///         Messages with the same <c>SessionId</c> are processed sequentially, while different sessions
///         can be processed in parallel.
///     </para>
///     <typeparam name="TBody">The type of the message body to deserialize and process.</typeparam>
///     <typeparam name="TConsumer">
///         The consumer type that implements <see cref="IMessageConsumer{TBody}"/> and handles the message.
///     </typeparam>
/// </summary>
public abstract class AzureServiceBusSessionReceiver<TBody, TConsumer>(
    ILogger logger,
    IServiceProvider serviceProvider,
    MessagingSection messagingSection)
    : AzureServiceBusReceiverBase<TBody, TConsumer>(logger, serviceProvider, messagingSection)
    where TBody : class
    where TConsumer : IMessageConsumer<TBody>
{
    private readonly ServiceBusSessionProcessor _processor = CreateProcessor(messagingSection);

    private static ServiceBusSessionProcessor CreateProcessor(MessagingSection section)
    {
        var client = new ServiceBusClient(section.ConnectionString);
        var options = new ServiceBusSessionProcessorOptions
        {
            MaxAutoLockRenewalDuration = section.MaxAutoLockRenewalDuration,
            MaxConcurrentSessions = section.MaxConcurrentCalls,
            MaxConcurrentCallsPerSession = 1,
            AutoCompleteMessages = section.AutoCompleteMessages
        };

        return string.IsNullOrEmpty(section.SubscriptionName)
            ? client.CreateSessionProcessor(section.Name, options)
            : client.CreateSessionProcessor(section.Name, section.SubscriptionName!, options);
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

    private Task HandleMessage(ProcessSessionMessageEventArgs args)
        => ProcessReceivedMessageAsync(
            args.Message,
            args.SessionId,
            ct => args.CompleteMessageAsync(args.Message, ct),
            ct => args.AbandonMessageAsync(args.Message, cancellationToken: ct),
            (reason, description, ct) => args.DeadLetterMessageAsync(args.Message, reason, description, ct),
            args.CancellationToken);
}
