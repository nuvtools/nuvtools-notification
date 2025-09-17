using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NuvTools.Notification.Messaging.Configuration;
using NuvTools.Notification.Messaging.Interfaces;
using System.Text.Json;

namespace NuvTools.Notification.Messaging.Azure.ServiceBus;

/// <summary>
///     Abstract background service for receiving and processing messages from Azure Service Bus.
///     <para>
///         This class sets up a <see cref="ServiceBusProcessor"/> for either a queue or a topic/subscription,
///         deserializes incoming messages, and dispatches them to a registered <see cref="IMessageConsumer{TBody}"/>.
///     </para>
///     <typeparam name="TBody">The type of the message body to deserialize and process.</typeparam>
///     <typeparam name="TConsumer">
///         The consumer type that implements <see cref="IMessageConsumer{TBody}"/> and handles the message.
///     </typeparam>
/// </summary>
public abstract class AzureServiceBusReceiver<TBody, TConsumer> : BackgroundService
    where TBody : class
    where TConsumer : IMessageConsumer<TBody>
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusProcessor _processor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    ///     Initializes a new instance of the <see cref="AzureServiceBusReceiver{TBody, TConsumer}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    /// <param name="messagingSection">The messaging configuration section containing Service Bus settings.</param>
    public AzureServiceBusReceiver(
        ILogger logger,
        IServiceProvider serviceProvider,
        MessagingSection messagingSection)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        _client = new ServiceBusClient(messagingSection.ConnectionString);

        var options = new ServiceBusProcessorOptions
        {
            MaxAutoLockRenewalDuration = messagingSection.MaxAutoLockRenewalDuration,
            MaxConcurrentCalls = messagingSection.MaxConcurrentCalls,
            AutoCompleteMessages = messagingSection.AutoCompleteMessages
        };

        _processor = string.IsNullOrEmpty(messagingSection.SubscriptionName)
            ? _client.CreateProcessor(messagingSection.Name, options) // Queue
            : _client.CreateProcessor(messagingSection.Name, messagingSection.SubscriptionName!, options); // Topic/Subscription
    }

    /// <summary>
    ///     Starts the background message processing loop.
    /// </summary>
    /// <param name="stoppingToken">A token to signal cancellation.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += HandleMessage;
        _processor.ProcessErrorAsync += HandleError;
        await _processor.StartProcessingAsync(stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    /// <summary>
    ///     Handles incoming messages, deserializes the body, and dispatches to the consumer.
    /// </summary>
    /// <param name="args">The message event arguments.</param>
    private async Task HandleMessage(ProcessMessageEventArgs args)
    {
        try
        {
            var body = args.Message.Body.ToString();
            _logger.LogInformation("Received: {Body}", body);

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
                TimeToLive = args.Message.TimeToLive
            };

            foreach (var kvp in args.Message.ApplicationProperties)
                message.Properties.TryAdd(kvp.Key, kvp.Value);

            using var scope = _serviceProvider.CreateScope();
            var consumer = scope.ServiceProvider.GetRequiredService<TConsumer>();

            try
            {
                await consumer.ConsumeAsync(message, args.CancellationToken);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming message {MessageId}", args.Message.MessageId);
                await args.AbandonMessageAsync(args.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unexpected fatal error in HandleMessage loop");
            await args.AbandonMessageAsync(args.Message);
        }
    }


    /// <summary>
    ///     Handles errors that occur during message processing.
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
            _logger.LogWarning("Processor stopped for {EntityPath}, attempting restart...", args.EntityPath);
            await RestartProcessorWithRetry(args.CancellationToken);
            _logger.LogInformation("Processor successfully restarted for {EntityPath}", args.EntityPath);
        }

        await Task.CompletedTask;
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
                _logger.LogInformation("Processor restarted successfully.");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Restart attempt {Attempt} failed", attempt);
                await Task.Delay(delay, cancellationToken);
                delay = delay * 2; // exponential backoff
            }
        }

        _logger.LogCritical("Processor could not be restarted after 3 attempts.");
    }


    /// <summary>
    ///     Stops the message processor and disposes resources.
    /// </summary>
    /// <param name="cancellationToken">A token to signal cancellation.</param>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync(cancellationToken);
        await _processor.DisposeAsync();
        await _client.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}