namespace NuvTools.Notification.Mail.Configuration;

/// <summary>
/// Contains the mail configuration that should be loaded from appsettings file.
/// <para>The default section name is "NuvTools.Notification.Mail"</para>
/// </summary>
public class MailConfigurationSection
{
    /// <summary>
    /// Gets or sets the default sender email address to use when not explicitly specified in the message.
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// Gets or sets the SMTP server hostname or IP address.
    /// </summary>
    public required string Host { get; set; }

    /// <summary>
    /// Gets or sets the SMTP server port number (e.g., 25, 587, or 465).
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the username for SMTP authentication.
    /// </summary>
    public required string UserName { get; set; }

    /// <summary>
    /// Gets or sets the password for SMTP authentication.
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// Gets or sets the default display name for the sender when not explicitly specified in the message.
    /// </summary>
    public string? DisplayName { get; set; }
}