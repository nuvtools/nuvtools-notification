using NuvTools.Common.ResultWrapper;

namespace NuvTools.Notification.Messaging.Interfaces;

public interface IMessageSender<T> where T : Message<T>
{
    Task<IResult> SendAsync(T message, CancellationToken cancellationToken);
}
