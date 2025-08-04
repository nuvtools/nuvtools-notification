using Microsoft.AspNetCore.SignalR;
using NuvTools.Notification.Realtime.Interfaces;

namespace NuvTools.Notification.Realtime.Azure.SignalR;

public class AzureSignalRSender<T>(IHubContext<SignalRHub> hubContext) : IMessageSender<T> where T : class
{
    public async Task SendAsync(T message, CancellationToken cancellationToken)
    {
        await hubContext.Clients.All.SendAsync($"Consume_{typeof(T).Name}", message, cancellationToken);
    }
}
