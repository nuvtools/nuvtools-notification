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
}
