namespace NuvTools.Notification.Messaging.Interfaces;

public interface IMessageConsumer<T> where T : class
{
    Task ConsumeAsync(Message<T> message, CancellationToken cancellationToken);
}
