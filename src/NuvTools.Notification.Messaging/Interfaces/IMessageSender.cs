namespace NuvTools.Notification.Messaging.Interfaces;

public interface IMessageSender<TBody> where TBody : class
{
    Task SendAsync(Message<TBody> message, CancellationToken cancellationToken);
}
