using NuvTools.Notification.Mail.Configuration;

namespace NuvTools.Notification.Mail.Smtp.Configuration;

/// <summary>
/// Contains the SMTP configuration that should be loaded from appsettings file.
/// <para>The default section name is "MailSmtp"</para>
/// </summary>
/// <remarks>
/// Extends <see cref="MailConfigurationSection"/> with the SMTP server settings, inheriting the shared
/// sender defaults.
/// </remarks>
public class SmtpMailConfigurationSection : MailConfigurationSection
{
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
    /// Gets or sets a value indicating whether the server TLS certificate is accepted without validation.
    /// Defaults to <c>false</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Enabling this option disables certificate chain, name and expiration checks, which exposes the
    /// connection — and therefore the credentials sent over it — to man-in-the-middle attacks.
    /// </para>
    /// <para>
    /// Intended only for development environments with self-signed certificates. Never enable it in production.
    /// </para>
    /// </remarks>
    public bool AllowInvalidCertificates { get; set; }
}
