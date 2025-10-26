namespace NuvTools.Notification.Messaging.Interfaces;

/// <summary>
/// Provides operational methods for managing the lifecycle of a received message
/// (e.g., completing, abandoning, dead-lettering).
/// </summary>
public interface IMessageContext
{
    Task CompleteAsync(CancellationToken cancellationToken = default);
    Task AbandonAsync(CancellationToken cancellationToken = default);
    Task DeadLetterAsync(string reason, string? errorDescription = null, CancellationToken cancellationToken = default);
}