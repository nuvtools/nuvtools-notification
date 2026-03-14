using Azure.Messaging.ServiceBus;
using NuvTools.Notification.Messaging.Interfaces;

namespace NuvTools.Notification.Messaging.Azure.ServiceBus;

/// <summary>
/// Azure Service Bus implementation of <see cref="IMessageContext"/> for session-enabled queues.
/// Wraps <see cref="ProcessSessionMessageEventArgs"/> and provides message lifecycle management operations
/// while preventing duplicate completion attempts.
/// </summary>
/// <param name="args">The Azure Service Bus session message processing event arguments.</param>
internal class AzureSessionMessageContext(ProcessSessionMessageEventArgs args) : IMessageContext
{
    /// <summary>
    /// Gets a value indicating whether the message has been completed, abandoned, or dead-lettered.
    /// </summary>
    public bool IsMessageCompleted { get; private set; }

    /// <inheritdoc />
    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        if (IsMessageCompleted) return;
        try
        {
            await args.CompleteMessageAsync(args.Message, cancellationToken);
        }
        finally
        {
            IsMessageCompleted = true;
        }
    }

    /// <inheritdoc />
    public async Task AbandonAsync(CancellationToken cancellationToken = default)
    {
        if (IsMessageCompleted) return;
        try
        {
            await args.AbandonMessageAsync(args.Message, cancellationToken: cancellationToken);
        }
        finally
        {
            IsMessageCompleted = true;
        }
    }

    /// <inheritdoc />
    public async Task DeadLetterAsync(string reason, string? errorDescription = null, CancellationToken cancellationToken = default)
    {
        if (IsMessageCompleted) return;
        try
        {
            await args.DeadLetterMessageAsync(args.Message, reason, errorDescription, cancellationToken);
        }
        finally
        {
            IsMessageCompleted = true;
        }
    }
}
