namespace NuvTools.Notification.Messaging.Interfaces;

public interface IMessageSender<T> where T : Message<T>
{
    Task SendAsync(T message, CancellationToken cancellationToken);
}
