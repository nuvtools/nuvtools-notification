namespace NuvTools.Notification.Messaging.Interfaces;

/// <summary>
/// Provides operational methods for managing the lifecycle of a received message
/// (e.g., completing, abandoning, dead-lettering).
/// </summary>
public interface IMessageContext
{
    /// <summary>
    /// Marks the message as successfully processed and removes it from the queue.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A task representing the asynchronous complete operation.</returns>
    Task CompleteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Abandons the message, making it available for reprocessing by returning it to the queue.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A task representing the asynchronous abandon operation.</returns>
    Task AbandonAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves the message to the dead-letter queue with a specified reason and optional error description.
    /// </summary>
    /// <param name="reason">The reason why the message is being dead-lettered.</param>
    /// <param name="errorDescription">Optional detailed description of the error that caused the message to be dead-lettered.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A task representing the asynchronous dead-letter operation.</returns>
    Task DeadLetterAsync(string reason, string? errorDescription = null, CancellationToken cancellationToken = default);
}