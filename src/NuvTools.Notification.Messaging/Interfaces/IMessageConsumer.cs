namespace NuvTools.Notification.Messaging.Interfaces;

/// <summary>
/// Defines a contract for consuming messages of a specific type.
/// </summary>
/// <typeparam name="TBody">
/// The type of the message body. Must be a reference type.
/// </typeparam>
/// <remarks>
/// Implement this interface to handle incoming messages in a messaging system.
/// </remarks>
public interface IMessageConsumer<TBody> where TBody : class
{
    /// <summary>
    /// Consumes a message asynchronously.
    /// </summary>
    /// <param name="message">
    /// The message to be consumed, containing the body and metadata.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous consume operation.
    /// </returns>
    Task ConsumeAsync(Message<TBody> message, CancellationToken cancellationToken);
}
