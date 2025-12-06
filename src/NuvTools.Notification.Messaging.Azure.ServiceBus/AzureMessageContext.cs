using Azure.Messaging.ServiceBus;
using NuvTools.Notification.Messaging.Interfaces;

namespace NuvTools.Notification.Messaging.Azure.ServiceBus;

/// <summary>
/// Azure Service Bus implementation of <see cref="IMessageContext"/>.
/// Wraps <see cref="ProcessMessageEventArgs"/> and provides message lifecycle management operations
/// while preventing duplicate completion attempts.
/// </summary>
/// <param name="args">The Azure Service Bus message processing event arguments.</param>
/// <remarks>
/// This class tracks whether a message has already been completed, abandoned, or dead-lettered
/// to prevent duplicate operations which would result in exceptions.
/// </remarks>
internal class AzureMessageContext(ProcessMessageEventArgs args) : IMessageContext
{
    /// <summary>
    /// Gets a value indicating whether the message has been completed, abandoned, or dead-lettered.
    /// </summary>
    public bool IsMessageCompleted { get; private set; }

    /// <summary>
    /// Marks the message as successfully processed and removes it from the queue.
    /// If the message has already been completed, this method returns immediately without action.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A task representing the asynchronous complete operation.</returns>
    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        if (IsMessageCompleted) return;
        await args.CompleteMessageAsync(args.Message, cancellationToken);
        IsMessageCompleted = true;
    }

    /// <summary>
    /// Abandons the message, making it available for reprocessing by returning it to the queue.
    /// If the message has already been completed, this method returns immediately without action.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A task representing the asynchronous abandon operation.</returns>
    public async Task AbandonAsync(CancellationToken cancellationToken = default)
    {
        if (IsMessageCompleted) return;
        await args.AbandonMessageAsync(args.Message, cancellationToken: cancellationToken);
        IsMessageCompleted = true;
    }

    /// <summary>
    /// Moves the message to the dead-letter queue with a specified reason and optional error description.
    /// If the message has already been completed, this method returns immediately without action.
    /// </summary>
    /// <param name="reason">The reason why the message is being dead-lettered.</param>
    /// <param name="errorDescription">Optional detailed description of the error that caused the message to be dead-lettered.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A task representing the asynchronous dead-letter operation.</returns>
    public async Task DeadLetterAsync(string reason, string? errorDescription = null, CancellationToken cancellationToken = default)
    {
        if (IsMessageCompleted) return;
        await args.DeadLetterMessageAsync(args.Message, reason, errorDescription, cancellationToken);
        IsMessageCompleted = true;
    }
}
