using Microsoft.AspNetCore.SignalR.Client;

namespace NuvTools.Notification.Realtime.Azure.SignalR.Client;

public class AzureSignalRReceiver<T>(string url) : IDisposable
{
    public TimeSpan DebounceDelay { get; set; } = TimeSpan.FromMilliseconds(1000);
    public event Action<T, CancellationToken>? MessageReceived;

    private HubConnection? _connection;
    private CancellationTokenSource? _debounceCts;

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

    public async Task DisconnectAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
            _debounceCts?.Cancel();
        }
    }

    public void Dispose()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();

        _connection?.DisposeAsync().AsTask().GetAwaiter().GetResult();

        GC.SuppressFinalize(this);
    }
}
