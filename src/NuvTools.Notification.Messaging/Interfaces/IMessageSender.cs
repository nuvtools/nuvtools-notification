namespace NuvTools.Notification.Messaging.Interfaces;

/// <summary>
/// Represents an asynchronous sender capable of delivering messages whose bodies are of type <typeparamref name="TBody"/>.
/// Implementations perform the transport-specific work to transmit a <see cref="Message{TBody}"/>.
/// </summary>
/// <typeparam name="TBody">The message body type. Must be a reference type.</typeparam>
public interface IMessageSender<TBody> where TBody : class
{
    /// <summary>
    /// Sends the specified <paramref name="message"/> asynchronously.
    /// </summary>
    /// <param name="message">The message to send. Implementations SHOULD validate that <paramref name="message"/> is not <c>null</c>.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous send operation.</returns>
    /// <exception cref="ArgumentNullException">May be thrown by implementations if <paramref name="message"/> is <c>null</c>.</exception>
    /// <exception cref="OperationCanceledException">May be thrown if the <paramref name="cancellationToken"/> is canceled before the operation completes.</exception>
    Task SendAsync(Message<TBody> message, CancellationToken cancellationToken);
}
