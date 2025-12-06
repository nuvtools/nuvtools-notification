using Microsoft.AspNetCore.SignalR;

namespace NuvTools.Notification.Realtime.Azure.SignalR;

/// <summary>
/// SignalR hub for real-time message broadcasting.
/// </summary>
/// <remarks>
/// This hub serves as the connection point for SignalR clients.
/// It is used by <see cref="AzureSignalRSender{T}"/> to broadcast messages to connected clients.
/// Register this hub in your application startup using <c>app.MapHub&lt;SignalRHub&gt;("/hub-endpoint")</c>.
/// </remarks>
public class SignalRHub : Hub;
