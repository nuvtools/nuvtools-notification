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
///     <remarks>
///         To use, inherit from this class and implement <see cref="GetMessagingSettings"/> to provide configuration.
///         Register your <typeparamref name="TConsumer"/> in the DI container.
///     </remarks>
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
    ///     Provides the messaging configuration settings for the receiver.
    /// </summary>
    /// <returns>The <see cref="MessagingSection"/> containing Service Bus settings.</returns>
    protected abstract MessagingSection GetMessagingSettings();

    /// <summary>
    ///     Initializes a new instance of the <see cref="AzureServiceBusReceiver{TBody, TConsumer}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    public AzureServiceBusReceiver(
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        var messagingSettings = GetMessagingSettings();
        _client = new ServiceBusClient(messagingSettings.ConnectionString);

        var options = new ServiceBusProcessorOptions
        {
            MaxAutoLockRenewalDuration = messagingSettings.MaxAutoLockRenewalDuration,
            MaxConcurrentCalls = messagingSettings.MaxConcurrentCalls,
            AutoCompleteMessages = messagingSettings.AutoCompleteMessages
        };

        _processor = string.IsNullOrEmpty(messagingSettings.SubscriptionName)
            ? _client.CreateProcessor(messagingSettings.Name, options) // Queue
            : _client.CreateProcessor(messagingSettings.Name, messagingSettings.SubscriptionName!, options); // Topic/Subscription
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

            var deserializedBody = JsonSerializer.Deserialize<TBody>(body, DefaultJsonSerializerOptions);

            if (deserializedBody is null)
            {
                _logger.LogWarning("Unable to deserialize body message.");
                await args.AbandonMessageAsync(args.Message);
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
            await consumer.ConsumeAsync(message, args.CancellationToken);

            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
            await args.AbandonMessageAsync(args.Message);
        }
    }

    /// <summary>
    ///     Handles errors that occur during message processing.
    /// </summary>
    /// <param name="args">The error event arguments.</param>
    private Task HandleError(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus error");
        return Task.CompletedTask;
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