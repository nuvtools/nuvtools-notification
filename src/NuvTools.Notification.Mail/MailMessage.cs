namespace NuvTools.Notification.Mail;

/// <summary>
/// Represents an email message with sender, recipients, subject, body, and optional attachments.
/// </summary>
/// <remarks>
/// This class serves as the abstraction for composing email messages across different mail providers.
/// It supports HTML body content and optional attachments through the <see cref="Parts"/> property.
/// Features not supported by every provider (<see cref="Tags"/>, <see cref="ScheduledFor"/>) are declared here
/// so the application layer never depends on a provider specific package. Implementations that cannot honor
/// them throw <see cref="NotSupportedException"/> rather than silently ignoring the request.
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
    /// Gets or sets the optional plain text version of the body.
    /// </summary>
    /// <remarks>
    /// Used by email clients that cannot render HTML. Providers that generate it automatically from
    /// <see cref="Body"/> do so only when this property is not informed.
    /// </remarks>
    public string? TextBody { get; set; }

    /// <summary>
    /// Gets or sets the optional list of attachments to include with the email.
    /// </summary>
    public List<MailPart>? Parts { get; set; }

    /// <summary>
    /// Gets or sets the optional custom email headers.
    /// </summary>
    /// <remarks>
    /// Standard headers such as To, From, Subject, Reply-To, CC and BCC cannot be overridden.
    /// </remarks>
    public IDictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// Gets or sets the optional custom metadata used by the provider for filtering and tracking.
    /// </summary>
    /// <remarks>
    /// Tags are not transmitted to the recipient. Providers with no equivalent concept throw
    /// <see cref="NotSupportedException"/>.
    /// </remarks>
    public IDictionary<string, string>? Tags { get; set; }

    /// <summary>
    /// Gets or sets the optional date and time when the email should be delivered.
    /// </summary>
    /// <remarks>
    /// When <c>null</c>, the email is sent immediately. Providers with no scheduling support throw
    /// <see cref="NotSupportedException"/> instead of delivering the message immediately.
    /// </remarks>
    public DateTimeOffset? ScheduledFor { get; set; }
}