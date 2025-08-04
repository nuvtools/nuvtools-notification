namespace NuvTools.Notification.Realtime.Interfaces;

public interface IMessageSender<T> where T : class
{
    Task SendAsync(T message, CancellationToken cancellationToken);
}
