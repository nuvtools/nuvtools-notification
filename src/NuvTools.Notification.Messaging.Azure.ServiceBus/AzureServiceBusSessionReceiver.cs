using Azure.Core;
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
public abstract class AzureServiceBusSessionReceiver<TBody, TConsumer>
    : AzureServiceBusReceiverBase<TBody, TConsumer>
    where TBody : class
    where TConsumer : IMessageConsumer<TBody>
{
    private readonly ServiceBusSessionProcessor _processor;

    /// <param name="logger">Logger used for processing diagnostics.</param>
    /// <param name="serviceProvider">Provider used to resolve <typeparamref name="TConsumer"/> per message.</param>
    /// <param name="messagingSection">The messaging configuration section containing connection details.</param>
    /// <param name="credential">
    /// Credential used when the section authenticates by fully qualified namespace. When null, one is derived
    /// from <see cref="MessagingSection.ManagedIdentityClientId"/>.
    /// </param>
    protected AzureServiceBusSessionReceiver(
        ILogger logger,
        IServiceProvider serviceProvider,
        MessagingSection messagingSection,
        TokenCredential? credential = null)
        : base(logger, serviceProvider, messagingSection, credential)
    {
        _processor = CreateProcessor(Client, messagingSection);
    }

    private static ServiceBusSessionProcessor CreateProcessor(ServiceBusClient client, MessagingSection section)
    {
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
