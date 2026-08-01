using NuvTools.Notification.Mail.Configuration;

namespace NuvTools.Notification.Mail.Twilio.Configuration;

/// <summary>
/// Contains the Twilio Email configuration that should be loaded from appsettings file.
/// <para>The default section name is "MailTwilio"</para>
/// </summary>
/// <remarks>
/// Extends <see cref="MailConfigurationSection"/> with the Twilio Email API settings, inheriting the shared
/// sender defaults. Credentials are sent using HTTP Basic authentication, where <see cref="AccountSid"/> is
/// the user name and <see cref="AuthToken"/> is the password.
/// </remarks>
public class TwilioMailConfigurationSection : MailConfigurationSection
{
    /// <summary>
    /// The default base address of the Twilio Email API.
    /// </summary>
    public const string DefaultBaseUrl = "https://comms.twilio.com/";

    /// <summary>
    /// Gets or sets the Twilio Account SID (e.g., "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx").
    /// </summary>
    public required string AccountSid { get; set; }

    /// <summary>
    /// Gets or sets the Twilio Auth Token used as the password for Basic authentication.
    /// </summary>
    public required string AuthToken { get; set; }

    /// <summary>
    /// Gets or sets the default IP Pool name used for sending emails.
    /// Only applies to accounts with dedicated IPs.
    /// </summary>
    public string? IpPoolName { get; set; }

    /// <summary>
    /// Gets or sets the base address of the Twilio Email API.
    /// Defaults to <see cref="DefaultBaseUrl"/>.
    /// </summary>
    public string BaseUrl { get; set; } = DefaultBaseUrl;
}
