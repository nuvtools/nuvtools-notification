using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NuvTools.Notification.Messaging.Configuration;
using NuvTools.Notification.Messaging.Interfaces;
using System.Text.Json;

namespace NuvTools.Notification.Messaging.Azure.ServiceBus;

/// <summary>
///     Abstract base class for Azure Service Bus receivers, containing shared message processing logic.
///     <typeparam name="TBody">The type of the message body to deserialize and process.</typeparam>
///     <typeparam name="TConsumer">
///         The consumer type that implements <see cref="IMessageConsumer{TBody}"/> and handles the message.
///     </typeparam>
/// </summary>
public abstract class AzureServiceBusReceiverBase<TBody, TConsumer> : BackgroundService
    where TBody : class
    where TConsumer : IMessageConsumer<TBody>
{
    private protected readonly ServiceBusClient Client;
    private protected readonly IServiceProvider ServiceProvider;
    private protected readonly ILogger Logger;

    private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new(JsonSerializerDefaults.Web);

    protected AzureServiceBusReceiverBase(
        ILogger logger,
        IServiceProvider serviceProvider,
        MessagingSection messagingSection)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
        Client = new ServiceBusClient(messagingSection.ConnectionString);
    }

    protected abstract bool IsProcessing { get; }
    protected abstract Task StartProcessorAsync(CancellationToken cancellationToken);
    protected abstract Task StopProcessorAsync(CancellationToken cancellationToken);
    protected abstract Task DisposeProcessorAsync();
    protected abstract Task RestartProcessorWithRetry(CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        RegisterHandlers();

        await StartProcessorAsync(stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) { }
    }

    protected abstract void RegisterHandlers();

    /// <summary>
    ///     Shared message processing logic for both regular and session receivers.
    /// </summary>
    protected async Task ProcessReceivedMessageAsync(
        ServiceBusReceivedMessage message,
        string? sessionId,
        Func<CancellationToken, Task> completeAsync,
        Func<CancellationToken, Task> abandonAsync,
        Func<string, string?, CancellationToken, Task> deadLetterAsync,
        CancellationToken cancellationToken)
    {
        try
        {
            var body = message.Body.ToString();

            if (sessionId is not null)
                Logger.LogDebug("Received (Session {SessionId}): {Body}", sessionId, body);
            else
                Logger.LogDebug("Received: {Body}", body);

            TBody? deserializedBody = null;
            try
            {
                deserializedBody = JsonSerializer.Deserialize<TBody>(body, DefaultJsonSerializerOptions);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Deserialization failed for message {MessageId}", message.MessageId);
                await deadLetterAsync("DeserializationFailed", ex.Message, cancellationToken);
                return;
            }

            if (deserializedBody is null)
            {
                Logger.LogWarning("Message {MessageId} deserialized to null", message.MessageId);
                await deadLetterAsync("DeserializationReturnedNull", null, cancellationToken);
                return;
            }

            var msg = new Message<TBody>(deserializedBody)
            {
                MessageId = message.MessageId,
                CorrelationId = message.CorrelationId,
                Subject = message.Subject,
                SessionId = message.SessionId,
                TimeToLive = message.TimeToLive,
                ScheduledEnqueueTime = message.ScheduledEnqueueTime
            };

            foreach (var kvp in message.ApplicationProperties)
                msg.Properties.TryAdd(kvp.Key, kvp.Value);

            using var scope = ServiceProvider.CreateScope();
            var consumer = scope.ServiceProvider.GetRequiredService<TConsumer>();

            var context = new AzureMessageContext(completeAsync, abandonAsync, deadLetterAsync);

            try
            {
                await consumer.ConsumeAsync(msg, context, cancellationToken);

                if (!context.IsMessageCompleted)
                    await completeAsync(cancellationToken);
            }
            catch (ServiceBusException sbEx) when (sbEx.Reason == ServiceBusFailureReason.MessageLockLost)
            {
                if (sessionId is not null)
                    Logger.LogWarning("Message lock lost for {MessageId} in session {SessionId}. The message will be retried automatically.", message.MessageId, sessionId);
                else
                    Logger.LogWarning("Message lock lost for {MessageId}. The message will be retried automatically.", message.MessageId);
            }
            catch (Exception ex)
            {
                if (sessionId is not null)
                    Logger.LogError(ex, "Error consuming message {MessageId} in session {SessionId}", message.MessageId, sessionId);
                else
                    Logger.LogError(ex, "Error consuming message {MessageId}", message.MessageId);

                if (!context.IsMessageCompleted)
                {
                    try
                    {
                        await abandonAsync(cancellationToken);
                    }
                    catch (ServiceBusException abandonEx) when (abandonEx.Reason == ServiceBusFailureReason.MessageLockLost)
                    {
                        if (sessionId is not null)
                            Logger.LogWarning("Could not abandon message {MessageId} in session {SessionId} — lock already lost. Message will be retried automatically.", message.MessageId, sessionId);
                        else
                            Logger.LogWarning("Could not abandon message {MessageId} — lock already lost. Message will be retried automatically.", message.MessageId);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (sessionId is not null)
                Logger.LogCritical(ex, "Unexpected fatal error in HandleMessage loop for session {SessionId}", sessionId);
            else
                Logger.LogCritical(ex, "Unexpected fatal error in HandleMessage loop");

            try
            {
                await abandonAsync(cancellationToken);
            }
            catch (ServiceBusException abandonEx) when (abandonEx.Reason == ServiceBusFailureReason.MessageLockLost)
            {
                if (sessionId is not null)
                    Logger.LogWarning("Could not abandon message {MessageId} in session {SessionId} after fatal error — lock already lost.", message.MessageId, sessionId);
                else
                    Logger.LogWarning("Could not abandon message {MessageId} after fatal error — lock already lost.", message.MessageId);
            }
        }
    }

    protected async Task HandleError(ProcessErrorEventArgs args)
    {
        Logger.LogError(args.Exception,
            "Service Bus error in {EntityPath} - ErrorSource: {ErrorSource}, Namespace: {FullyQualifiedNamespace}",
            args.EntityPath,
            args.ErrorSource,
            args.FullyQualifiedNamespace);

        if (!IsProcessing)
        {
            try
            {
                Logger.LogWarning("Processor stopped for {EntityPath}, attempting restart...", args.EntityPath);
                await RestartProcessorWithRetry(args.CancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "Failed to restart processor for {EntityPath}", args.EntityPath);
            }
        }
    }

    protected async Task RestartWithRetry(Func<CancellationToken, Task> startProcessingAsync, CancellationToken cancellationToken)
    {
        var delay = TimeSpan.FromSeconds(5);

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                Logger.LogWarning("Restart attempt {Attempt}...", attempt);
                await startProcessingAsync(cancellationToken);
                Logger.LogInformation("Processor restarted successfully.");
                return;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Restart attempt {Attempt} failed", attempt);
                await Task.Delay(delay, cancellationToken);
                delay = delay * 2;
            }
        }

        Logger.LogCritical("Processor could not be restarted after 3 attempts.");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (IsProcessing)
            await StopProcessorAsync(cancellationToken);

        await DisposeProcessorAsync();
        await Client.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
