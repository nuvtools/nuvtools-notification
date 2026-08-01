namespace NuvTools.Notification.Mail.Configuration;

/// <summary>
/// Contains the mail settings shared by every provider, loaded from the appsettings file.
/// <para>The default section name is "Mail"</para>
/// </summary>
/// <remarks>
/// Provider specific packages derive from this class to add their own settings, keeping infrastructure
/// details out of the abstraction. Bind the derived section with
/// <see cref="ServiceCollectionExtensions.AddMailConfiguration{T}"/>.
/// </remarks>
public class MailConfigurationSection
{
    /// <summary>
    /// Gets or sets the default sender email address to use when not explicitly specified in the message.
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// Gets or sets the default display name for the sender when not explicitly specified in the message.
    /// </summary>
    public string? DisplayName { get; set; }
}
