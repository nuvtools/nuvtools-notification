namespace NuvTools.Notification.Mail;

/// <summary>
/// Represents an email address with an optional display name.
/// </summary>
/// <remarks>
/// This class is used to specify email addresses for senders and recipients in mail messages.
/// It supports both simple email addresses and addresses with friendly display names.
/// </remarks>
public class MailAddress
{
    /// <summary>
    /// Gets or sets the email address (e.g., "user@example.com").
    /// </summary>
    public required string Address { get; set; }

    /// <summary>
    /// Gets or sets the optional display name for the email address (e.g., "John Doe").
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the optional template variables used to personalize the message for this recipient.
    /// </summary>
    /// <remarks>
    /// Each provider defines its own template syntax for the placeholders in the subject and body.
    /// Providers with no personalization support throw <see cref="NotSupportedException"/> instead of
    /// delivering the message with the placeholders unresolved.
    /// </remarks>
    public IDictionary<string, object?>? Variables { get; set; }
}
