using Microsoft.AspNetCore.SignalR;
using NuvTools.Notification.Realtime.Interfaces;

namespace NuvTools.Notification.Realtime.Azure.SignalR;

/// <summary>
/// Azure SignalR implementation of <see cref="IMessageSender{T}"/> for broadcasting real-time messages to all connected clients.
/// </summary>
/// <typeparam name="T">The type of message to send. Must be a reference type.</typeparam>
/// <param name="hubContext">The SignalR hub context used to send messages to clients.</param>
/// <remarks>
/// This sender broadcasts messages to all connected clients using a method name pattern of "Consume_{TypeName}".
/// Clients must register a handler for this method name to receive messages.
/// </remarks>
public class AzureSignalRSender<T>(IHubContext<SignalRHub> hubContext) : IMessageSender<T> where T : class
{
    /// <summary>
    /// Sends a message asynchronously to all connected SignalR clients.
    /// </summary>
    /// <param name="message">The message to broadcast to all clients.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A task representing the asynchronous send operation.</returns>
    /// <remarks>
    /// The message is sent using the method name "Consume_{TypeName}" where {TypeName} is the name of type <typeparamref name="T"/>.
    /// All connected clients that have registered a handler for this method will receive the message.
    /// </remarks>
    public async Task SendAsync(T message, CancellationToken cancellationToken)
    {
        await hubContext.Clients.All.SendAsync($"Consume_{typeof(T).Name}", message, cancellationToken);
    }
}
