using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NuvTools.Notification.Messaging.Configuration;
using NuvTools.Notification.Messaging.Interfaces;
using System.Text.Json;

namespace NuvTools.Notification.Messaging.Azure.ServiceBus;

/// <summary>
///     Abstract background service for receiving and processing messages from session-enabled Azure Service Bus queues.
///     <para>
///         Messages with the same <c>SessionId</c> are processed sequentially, while different sessions
///         can be processed in parallel. This is useful for scenarios where order matters per logical group
///         (e.g., per company) but parallelism across groups is desired.
///     </para>
///     <typeparam name="TBody">The type of the message body to deserialize and process.</typeparam>
///     <typeparam name="TConsumer">
///         The consumer type that implements <see cref="IMessageConsumer{TBody}"/> and handles the message.
///     </typeparam>
/// </summary>
public abstract class AzureServiceBusSessionReceiver<TBody, TConsumer> : BackgroundService
    where TBody : class
    where TConsumer : IMessageConsumer<TBody>
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSessionProcessor _processor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    ///     Initializes a new instance of the <see cref="AzureServiceBusSessionReceiver{TBody, TConsumer}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    /// <param name="messagingSection">The messaging configuration section containing Service Bus settings.</param>
    public AzureServiceBusSessionReceiver(
        ILogger logger,
        IServiceProvider serviceProvider,
        MessagingSection messagingSection)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        _client = new ServiceBusClient(messagingSection.ConnectionString);

        var options = new ServiceBusSessionProcessorOptions
        {
            MaxAutoLockRenewalDuration = messagingSection.MaxAutoLockRenewalDuration,
            MaxConcurrentSessions = messagingSection.MaxConcurrentCalls,
            MaxConcurrentCallsPerSession = 1,
            AutoCompleteMessages = messagingSection.AutoCompleteMessages
        };

        _processor = string.IsNullOrEmpty(messagingSection.SubscriptionName)
            ? _client.CreateSessionProcessor(messagingSection.Name, options)
            : _client.CreateSessionProcessor(messagingSection.Name, messagingSection.SubscriptionName!, options);
    }

    /// <summary>
    ///     Starts the background session message processing loop.
    /// </summary>
    /// <param name="stoppingToken">A token to signal cancellation.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += HandleMessage;
        _processor.ProcessErrorAsync += HandleError;

        await _processor.StartProcessingAsync(stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) { }
    }

    /// <summary>
    ///     Handles incoming session messages, deserializes the body, and dispatches to the consumer.
    /// </summary>
    /// <param name="args">The session message event arguments.</param>
    private async Task HandleMessage(ProcessSessionMessageEventArgs args)
    {
        try
        {
            var body = args.Message.Body.ToString();
            _logger.LogDebug("Received (Session {SessionId}): {Body}", args.SessionId, body);

            TBody? deserializedBody = null;
            try
            {
                deserializedBody = JsonSerializer.Deserialize<TBody>(body, DefaultJsonSerializerOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Deserialization failed for message {MessageId}", args.Message.MessageId);
                await args.DeadLetterMessageAsync(args.Message, "DeserializationFailed", ex.Message);
                return;
            }

            if (deserializedBody is null)
            {
                _logger.LogWarning("Message {MessageId} deserialized to null", args.Message.MessageId);
                await args.DeadLetterMessageAsync(args.Message, "DeserializationReturnedNull");
                return;
            }

            var message = new Message<TBody>(deserializedBody)
            {
                MessageId = args.Message.MessageId,
                CorrelationId = args.Message.CorrelationId,
                Subject = args.Message.Subject,
                SessionId = args.Message.SessionId,
                TimeToLive = args.Message.TimeToLive
            };

            foreach (var kvp in args.Message.ApplicationProperties)
                message.Properties.TryAdd(kvp.Key, kvp.Value);

            using var scope = _serviceProvider.CreateScope();
            var consumer = scope.ServiceProvider.GetRequiredService<TConsumer>();

            var context = new AzureSessionMessageContext(args);

            try
            {
                await consumer.ConsumeAsync(message, context, args.CancellationToken);

                if (!context.IsMessageCompleted)
                    await args.CompleteMessageAsync(args.Message);
            }
            catch (ServiceBusException sbEx) when (sbEx.Reason == ServiceBusFailureReason.MessageLockLost)
            {
                _logger.LogWarning("Message lock lost for {MessageId} in session {SessionId}. The message will be retried automatically.", args.Message.MessageId, args.SessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming message {MessageId} in session {SessionId}", args.Message.MessageId, args.SessionId);

                if (!context.IsMessageCompleted)
                {
                    try
                    {
                        await args.AbandonMessageAsync(args.Message);
                    }
                    catch (ServiceBusException abandonEx) when (abandonEx.Reason == ServiceBusFailureReason.MessageLockLost)
                    {
                        _logger.LogWarning("Could not abandon message {MessageId} in session {SessionId} — lock already lost. Message will be retried automatically.", args.Message.MessageId, args.SessionId);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unexpected fatal error in HandleMessage loop for session {SessionId}", args.SessionId);

            try
            {
                await args.AbandonMessageAsync(args.Message);
            }
            catch (ServiceBusException abandonEx) when (abandonEx.Reason == ServiceBusFailureReason.MessageLockLost)
            {
                _logger.LogWarning("Could not abandon message {MessageId} in session {SessionId} after fatal error — lock already lost.", args.Message.MessageId, args.SessionId);
            }
        }
    }

    /// <summary>
    ///     Handles errors that occur during session message processing.
    /// </summary>
    /// <param name="args">The error event arguments.</param>
    private async Task HandleError(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception,
            "Service Bus error in {EntityPath} - ErrorSource: {ErrorSource}, Namespace: {FullyQualifiedNamespace}",
            args.EntityPath,
            args.ErrorSource,
            args.FullyQualifiedNamespace);

        if (!_processor.IsProcessing)
        {
            try
            {
                _logger.LogWarning("Session processor stopped for {EntityPath}, attempting restart...", args.EntityPath);
                await RestartProcessorWithRetry(args.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to restart session processor for {EntityPath}", args.EntityPath);
            }
        }
    }

    private async Task RestartProcessorWithRetry(CancellationToken cancellationToken)
    {
        var delay = TimeSpan.FromSeconds(5);

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                _logger.LogWarning("Restart attempt {Attempt}...", attempt);
                await _processor.StartProcessingAsync(cancellationToken);
                _logger.LogInformation("Session processor restarted successfully.");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Restart attempt {Attempt} failed", attempt);
                await Task.Delay(delay, cancellationToken);
                delay = delay * 2;
            }
        }

        _logger.LogCritical("Session processor could not be restarted after 3 attempts.");
    }

    /// <summary>
    ///     Stops the session message processor and disposes resources.
    /// </summary>
    /// <param name="cancellationToken">A token to signal cancellation.</param>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor.IsProcessing)
            await _processor.StopProcessingAsync(cancellationToken);

        await _processor.DisposeAsync();
        await _client.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
