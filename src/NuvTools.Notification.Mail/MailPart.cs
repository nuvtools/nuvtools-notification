namespace NuvTools.Notification.Mail;

/// <summary>
/// Represents an email attachment with media type, extension, and content stream.
/// </summary>
/// <remarks>
/// This class is used to attach files to email messages. The content is provided as a stream,
/// and both the media type and extension are required to properly identify the attachment format.
/// </remarks>
/// <example>
/// <code language="c#">
/// var attachment = new MailPart
/// {
///     MediaType = "application",
///     MediaExtension = "pdf",
///     Content = fileStream
/// };
/// </code>
/// </example>
public class MailPart
{
    /// <summary>
    /// Gets or sets the MIME media type of the attachment (e.g., "application", "image", "text").
    /// </summary>
    public required string MediaType { get; set; }

    /// <summary>
    /// Gets or sets the file extension or media subtype of the attachment (e.g., "pdf", "png", "plain").
    /// </summary>
    public required string MediaExtension { get; set; }

    /// <summary>
    /// Gets or sets the content stream of the attachment.
    /// </summary>
    public required Stream Content { get; set; }
}