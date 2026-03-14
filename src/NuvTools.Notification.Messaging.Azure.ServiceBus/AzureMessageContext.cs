using NuvTools.Notification.Messaging.Interfaces;

namespace NuvTools.Notification.Messaging.Azure.ServiceBus;

/// <summary>
/// Azure Service Bus implementation of <see cref="IMessageContext"/>.
/// Uses delegates to abstract over both <c>ProcessMessageEventArgs</c> and <c>ProcessSessionMessageEventArgs</c>,
/// while preventing duplicate completion attempts.
/// </summary>
internal class AzureMessageContext(
    Func<CancellationToken, Task> completeAsync,
    Func<CancellationToken, Task> abandonAsync,
    Func<string, string?, CancellationToken, Task> deadLetterAsync) : IMessageContext
{
    public bool IsMessageCompleted { get; private set; }

    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        if (IsMessageCompleted) return;
        try
        {
            await completeAsync(cancellationToken);
        }
        finally
        {
            IsMessageCompleted = true;
        }
    }

    public async Task AbandonAsync(CancellationToken cancellationToken = default)
    {
        if (IsMessageCompleted) return;
        try
        {
            await abandonAsync(cancellationToken);
        }
        finally
        {
            IsMessageCompleted = true;
        }
    }

    public async Task DeadLetterAsync(string reason, string? errorDescription = null, CancellationToken cancellationToken = default)
    {
        if (IsMessageCompleted) return;
        try
        {
            await deadLetterAsync(reason, errorDescription, cancellationToken);
        }
        finally
        {
            IsMessageCompleted = true;
        }
    }
}
