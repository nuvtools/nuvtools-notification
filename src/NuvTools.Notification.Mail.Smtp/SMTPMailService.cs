using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NuvTools.Notification.Mail.Smtp.Configuration;

namespace NuvTools.Notification.Mail.Smtp;

/// <summary>
/// SMTP implementation of <see cref="IMailService"/> using MailKit for sending email messages.
/// </summary>
/// <param name="appMailConfiguration">The mail configuration options containing SMTP server settings.</param>
/// <remarks>
/// This service uses the MailKit library to send emails via SMTP protocol.
/// It supports HTML and plain text body content, custom headers, multiple recipients, file attachments
/// and inline images.
/// The SMTP connection is configured using settings from <see cref="SmtpMailConfigurationSection"/>, and the
/// server TLS certificate is validated unless <see cref="SmtpMailConfigurationSection.AllowInvalidCertificates"/>
/// is explicitly enabled.
/// </remarks>
public class SMTPMailService(IOptions<SmtpMailConfigurationSection> appMailConfiguration) : IMailService
{
    private readonly SmtpMailConfigurationSection _appMailConfiguration = appMailConfiguration.Value;

    /// <summary>
    /// Sends an email message asynchronously using the configured SMTP server.
    /// </summary>
    /// <param name="request">The mail message to send, including sender, recipients, subject, body, and optional attachments.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The Message-ID assigned to the delivered message.</returns>
    /// <remarks>
    /// This method constructs a MIME message from the provided <paramref name="request"/>, connects to the SMTP server,
    /// authenticates using the configured credentials, sends the message, and disconnects.
    /// If the message includes attachments via <see cref="MailMessage.Parts"/>, they are added as MIME attachments
    /// named after <see cref="MailPart.FileName"/>, or embedded inline when <see cref="MailPart.ContentId"/> is informed.
    /// The sender address and display name can be overridden per message or fall back to configuration defaults.
    /// </remarks>
    /// <exception cref="NotSupportedException">
    /// Thrown when the message requires provider features SMTP has no equivalent for, namely
    /// <see cref="MailMessage.ScheduledFor"/>, <see cref="MailMessage.Tags"/> and <see cref="MailAddress.Variables"/>.
    /// Failing is preferred over delivering the message immediately, untagged or with unresolved placeholders.
    /// </exception>
    public async Task<MailSendResult> SendAsync(MailMessage request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.ScheduledFor is not null)
            throw new NotSupportedException("Scheduled delivery is not supported by SMTP.");

        if (request.Tags is not null && request.Tags.Count > 0)
            throw new NotSupportedException("Tags are not supported by SMTP.");

        if (request.To.Exists(e => e.Variables is not null && e.Variables.Count > 0))
            throw new NotSupportedException("Personalization variables are not supported by SMTP.");

        var message = new MimeMessage();
        var bodyBuilder = new BodyBuilder();

        message.From.Add(new MailboxAddress(request.From.DisplayName ?? _appMailConfiguration.DisplayName,
                                            request.From.Address ?? _appMailConfiguration.From!));
        message.To.AddRange(request.To.Select(e => new MailboxAddress(e.DisplayName, e.Address)));

        message.Subject = request.Subject;
        bodyBuilder.HtmlBody = request.Body;
        bodyBuilder.TextBody = request.TextBody;

        if (request.Headers != null)
        {
            foreach (var header in request.Headers)
                message.Headers.Add(header.Key, header.Value);
        }

        if (request.Parts != null && request.Parts.Count > 0)
        {
            foreach (var item in request.Parts.Where(e => e.ContentId != null))
            {
                var inline = bodyBuilder.LinkedResources.Add(item.FileName, item.Content, new ContentType(item.MediaType, item.MediaExtension), cancellationToken);
                inline.ContentId = item.ContentId;
            }
        }

        message.Body = bodyBuilder.ToMessageBody();

        var attachments = request.Parts?.Where(e => e.ContentId == null).ToList();

        if (attachments != null && attachments.Count > 0)
        {
            var multipart = new Multipart("mixed")
                {
                    message.Body
                };

            foreach (var item in attachments)
            {
                var attachment = new MimePart(item.MediaType, item.MediaExtension)
                {
                    Content = new MimeContent(item.Content),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = item.FileName
                };

                multipart.Add(attachment);
            }

            message.Body = multipart;
        }

        using (var client = new SmtpClient())
        {
            if (_appMailConfiguration.AllowInvalidCertificates)
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync(_appMailConfiguration.Host, _appMailConfiguration.Port, SecureSocketOptions.Auto, cancellationToken);
            await client.AuthenticateAsync(_appMailConfiguration.UserName, _appMailConfiguration.Password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }

        return new MailSendResult(message.MessageId);
    }
}
