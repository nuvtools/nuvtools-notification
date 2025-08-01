namespace NuvTools.Notification.Messaging;

public class Message<T>(T body) where T : class
{
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
    public string? CorrelationId { get; set; }
    public string? Subject { get; set; }
    public TimeSpan? TimeToLive { get; set; }
    public Dictionary<string, object> Properties { get; set; } = [];
    public required T Body { get; set; } = body;
}
