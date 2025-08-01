namespace NuvTools.Notification.Messaging.Interfaces;

public interface IMessageConsumer<TBody> where TBody : class
{
    Task ConsumeAsync(Message<TBody> message, CancellationToken cancellationToken);
}
