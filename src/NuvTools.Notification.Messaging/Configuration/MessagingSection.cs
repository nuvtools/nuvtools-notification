namespace NuvTools.Notification.Messaging.Configuration;

/// <summary>
/// Represents the configuration settings for a messaging queue, including its name, connection string,
/// subscription name, and various operational parameters.
/// </summary>
/// <remarks>
/// This class is typically used to bind configuration sections from appsettings or other configuration sources
/// for messaging queue consumers or producers.
/// </remarks>
public class MessagingSection
{
    /// <summary>
    /// Gets or sets the name of the messaging queue or topic.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the subscription name for the messaging queue, if applicable.
    /// </summary>
    public string? SubscriptionName { get; set; }

    /// <summary>
    /// Gets or sets the connection string used to connect to the messaging service.
    /// </summary>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the maximum duration for which the message lock will be automatically renewed.
    /// Default is 30 minutes.
    /// </summary>
    public TimeSpan MaxAutoLockRenewalDuration { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Gets or sets the maximum number of concurrent calls to the message handler.
    /// Default is 10.
    /// </summary>
    public int MaxConcurrentCalls { get; set; } = 10;

    /// <summary>
    /// Gets or sets a value indicating whether messages should be automatically completed after processing.
    /// Default is false.
    /// </summary>
    public bool AutoCompleteMessages { get; set; } = false;
}