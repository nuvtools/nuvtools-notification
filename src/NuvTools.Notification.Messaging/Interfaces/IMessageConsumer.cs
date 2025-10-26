namespace NuvTools.Notification.Messaging.Interfaces
{
    /// <summary>
    /// Defines a contract for consuming messages of a specific type.
    /// </summary>
    public interface IMessageConsumer<TBody> where TBody : class
    {
        /// <summary>
        /// Consumes a message asynchronously.
        /// </summary>
        /// <param name="message">The message to consume.</param>
        /// <param name="context">The operational context for managing message lifecycle.</param>
        /// <param name="cancellationToken">A token for cancellation.</param>
        Task ConsumeAsync(Message<TBody> message, IMessageContext context, CancellationToken cancellationToken);
    }
}