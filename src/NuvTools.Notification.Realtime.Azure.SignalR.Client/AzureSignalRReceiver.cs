using Microsoft.AspNetCore.SignalR.Client;

namespace NuvTools.Notification.Realtime.Azure.SignalR.Client;

/// <summary>
/// SignalR client for receiving real-time messages from an Azure SignalR hub with debouncing support.
/// </summary>
/// <typeparam name="T">The type of message to receive.</typeparam>
/// <param name="url">The URL of the SignalR hub to connect to.</param>
/// <remarks>
/// This receiver establishes a connection to a SignalR hub and listens for messages of type <typeparamref name="T"/>.
/// It includes built-in debouncing to prevent rapid-fire message handling and supports automatic reconnection.
/// The receiver listens for messages on a method named "Consume_{TypeName}" where {TypeName} is the name of type <typeparamref name="T"/>.
/// </remarks>
public class AzureSignalRReceiver<T>(string url) : IDisposable
{
    /// <summary>
    /// Gets or sets the debounce delay to prevent rapid-fire message handling.
    /// Default is 1000 milliseconds (1 second).
    /// </summary>
    /// <remarks>
    /// When multiple messages arrive in quick succession, only the last message within the debounce window
    /// will trigger the <see cref="MessageReceived"/> event.
    /// </remarks>
    public TimeSpan DebounceDelay { get; set; } = TimeSpan.FromMilliseconds(1000);

    /// <summary>
    /// Event raised when a message is received after the debounce delay has elapsed.
    /// </summary>
    public event Action<T, CancellationToken>? MessageReceived;

    private HubConnection? _connection;
    private CancellationTokenSource? _debounceCts;

    /// <summary>
    /// Establishes a connection to the SignalR hub and starts listening for messages.
    /// </summary>
    /// <returns>A task representing the asynchronous connect operation.</returns>
    /// <remarks>
    /// If already connected, this method returns immediately without creating a new connection.
    /// The connection is configured with automatic reconnection enabled.
    /// Messages are received on a method named "Consume_{TypeName}".
    /// </remarks>
    public async Task ConnectAsync()
    {
        if (_connection is { State: HubConnectionState.Connected })
            return;

        _connection = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect()
            .Build();

        string methodName = $"Consume_{typeof(T).Name}";

        _connection.On<T>(methodName, message =>
        {
            var cts = new CancellationTokenSource();
            _debounceCts?.Cancel();
            _debounceCts = cts;
            var token = cts.Token;

            _ = Task.Delay(DebounceDelay, token)
                .ContinueWith(t =>
                {
                    if (!t.IsCanceled)
                        MessageReceived?.Invoke(message, token);
                }, TaskScheduler.Default);
        });

        await _connection.StartAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Disconnects from the SignalR hub and disposes the connection.
    /// </summary>
    /// <returns>A task representing the asynchronous disconnect operation.</returns>
    /// <remarks>
    /// This method also cancels any pending debounce operations.
    /// </remarks>
    public async Task DisconnectAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
            _debounceCts?.Cancel();
        }
    }

    /// <summary>
    /// Disposes the receiver, disconnecting from the hub and releasing all resources.
    /// </summary>
    /// <remarks>
    /// This method cancels any pending debounce operations and synchronously disposes the hub connection.
    /// </remarks>
    public void Dispose()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();

        _connection?.DisposeAsync().AsTask().GetAwaiter().GetResult();

        GC.SuppressFinalize(this);
    }
}
