namespace NuvTools.Notification.Mail;

/// <summary>
/// Represents an email message with sender, recipients, subject, body, and optional attachments.
/// </summary>
/// <remarks>
/// This class serves as the abstraction for composing email messages across different mail providers.
/// It supports HTML body content and optional attachments through the <see cref="Parts"/> property.
/// </remarks>
public class MailMessage
{
    /// <summary>
    /// Gets or sets the sender's email address.
    /// </summary>
    public required MailAddress From { get; set; }

    /// <summary>
    /// Gets or sets the list of recipient email addresses.
    /// </summary>
    public required List<MailAddress> To { get; set; }

    /// <summary>
    /// Gets or sets the subject line of the email.
    /// </summary>
    public required string Subject { get; set; }

    /// <summary>
    /// Gets or sets the body content of the email.
    /// This is typically HTML content that will be rendered by the email client.
    /// </summary>
    public required string Body { get; set; }

    /// <summary>
    /// Gets or sets the optional list of attachments to include with the email.
    /// </summary>
    public List<MailPart>? Parts { get; set; }
}