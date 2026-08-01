namespace NuvTools.Notification.Mail;

/// <summary>
/// Represents an email attachment with media type, extension, and content stream.
/// </summary>
/// <remarks>
/// This class is used to attach files to email messages. The content is provided as a stream,
/// and both the media type and extension are required to properly identify the attachment format,
/// while the file name identifies it to the recipient.
/// </remarks>
/// <example>
/// <code language="c#">
/// var attachment = new MailPart
/// {
///     MediaType = "application",
///     MediaExtension = "pdf",
///     FileName = "invoice.pdf",
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
    /// Gets or sets the media subtype of the attachment (e.g., "pdf", "png", "plain").
    /// </summary>
    /// <remarks>
    /// Combined with <see cref="MediaType"/> it forms the MIME type of the attachment, so it is the media
    /// subtype rather than the file extension. For a Word document, the subtype is
    /// "vnd.openxmlformats-officedocument.wordprocessingml.document" while the ".docx" extension belongs
    /// to <see cref="FileName"/>.
    /// </remarks>
    public required string MediaExtension { get; set; }

    /// <summary>
    /// Gets or sets the file name presented to the recipient (e.g., "invoice.pdf").
    /// </summary>
    /// <remarks>
    /// Attachments sent without a name are displayed with an arbitrary name chosen by the email client
    /// (e.g., "ATT00001.pdf" or "noname"), so the file name is always required.
    /// </remarks>
    public required string FileName { get; set; }

    /// <summary>
    /// Gets or sets the content stream of the attachment.
    /// </summary>
    public required Stream Content { get; set; }

    /// <summary>
    /// Gets or sets the optional Content-ID used to reference the attachment from the HTML body.
    /// </summary>
    /// <remarks>
    /// When informed, the attachment is embedded in the message and can be referenced as
    /// <c>&lt;img src="cid:yourContentIdHere" /&gt;</c> instead of being listed as a regular attachment.
    /// </remarks>
    public string? ContentId { get; set; }
}