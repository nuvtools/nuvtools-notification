namespace NuvTools.Notification.Messaging.Configuration;

/// <summary>
/// Represents the configuration settings for a messaging queue, including its name and connection string.
/// </summary>
public class MessagingQueueSection
{
    public required string Name { get; set; }
    public required string ConnectionString { get; set; }
}