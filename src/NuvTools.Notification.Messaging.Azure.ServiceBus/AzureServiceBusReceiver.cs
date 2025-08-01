using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NuvTools.Notification.Messaging.Configuration;
using NuvTools.Notification.Messaging.Interfaces;
using System.Text.Json;

namespace NuvTools.Notification.Messaging.Azure.ServiceBus;

public abstract class AzureServiceBusReceiver<TBody, TConsumer> : BackgroundService
    where TBody : class
    where TConsumer : IMessageConsumer<TBody>
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusProcessor _processor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new(JsonSerializerDefaults.Web);

    protected abstract MessagingQueueSection GetMessagingQueueSettings();

    public AzureServiceBusReceiver(
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        var queueSettings = GetMessagingQueueSettings();
        _client = new ServiceBusClient(queueSettings.ConnectionString);
        _processor = _client.CreateProcessor(queueSettings.Name, new ServiceBusProcessorOptions
        {
            MaxAutoLockRenewalDuration = queueSettings.MaxAutoLockRenewalDuration,
            MaxConcurrentCalls = queueSettings.MaxConcurrentCalls,
            AutoCompleteMessages = queueSettings.AutoCompleteMessages
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += HandleMessage;
        _processor.ProcessErrorAsync += HandleError;
        await _processor.StartProcessingAsync(stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleMessage(ProcessMessageEventArgs args)
    {
        try
        {
            var body = args.Message.Body.ToString();
            _logger.LogInformation("Received: {Body}", body);

            var message = JsonSerializer.Deserialize<Message<TBody>>(body, DefaultJsonSerializerOptions);

            if (message is null)
            {
                _logger.LogWarning("Unable to deserialize message.");
                await args.AbandonMessageAsync(args.Message);
                return;
            }

            message.MessageId = args.Message.MessageId;
            message.CorrelationId ??= args.Message.CorrelationId;
            message.Subject ??= args.Message.Subject;
            message.TimeToLive ??= args.Message.TimeToLive;

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

    private Task HandleError(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus error");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync(cancellationToken);
        await _processor.DisposeAsync();
        await _client.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}

