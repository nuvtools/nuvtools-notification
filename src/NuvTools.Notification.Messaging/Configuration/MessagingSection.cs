namespace NuvTools.Notification.Messaging.Configuration;

/// <summary>
/// Represents the configuration settings for a messaging queue, including its name, connection details,
/// subscription name, and various operational parameters.
/// </summary>
/// <remarks>
/// This class is typically used to bind configuration sections from appsettings or other configuration sources
/// for messaging queue consumers or producers.
/// <para>
/// Connectivity is declared either with <see cref="ConnectionString"/> (shared access key) or with
/// <see cref="FullyQualifiedNamespace"/> (Entra ID credential). When both are present the connection string wins.
/// </para>
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
    /// Leave empty to authenticate with an Entra ID credential against <see cref="FullyQualifiedNamespace"/>.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the fully qualified namespace of the messaging service
    /// (for example <c>contoso.servicebus.windows.net</c>), used when authenticating with an Entra ID
    /// credential instead of a connection string.
    /// </summary>
    public string? FullyQualifiedNamespace { get; set; }

    /// <summary>
    /// Gets or sets the client ID of the user-assigned managed identity to authenticate with when
    /// <see cref="FullyQualifiedNamespace"/> is used and no explicit credential is supplied.
    /// Leave empty to use the system-assigned identity or the ambient developer credential.
    /// </summary>
    public string? ManagedIdentityClientId { get; set; }

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

    /// <summary>
    /// Gets or sets a value indicating whether the queue or subscription requires sessions.
    /// When enabled, the receiver must use a session-aware processor.
    /// Default is false.
    /// </summary>
    public bool RequiresSession { get; set; }
}
