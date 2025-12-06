using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NuvTools.Notification.Mail.Configuration;

namespace NuvTools.Notification.Mail.Smtp;

/// <summary>
/// SMTP implementation of <see cref="IMailService"/> using MailKit for sending email messages.
/// </summary>
/// <param name="appMailConfiguration">The mail configuration options containing SMTP server settings.</param>
/// <remarks>
/// This service uses the MailKit library to send emails via SMTP protocol.
/// It supports HTML body content, multiple recipients, and file attachments.
/// The SMTP connection is configured using settings from <see cref="MailConfigurationSection"/>.
/// </remarks>
public class SMTPMailService(IOptions<MailConfigurationSection> appMailConfiguration) : IMailService
{
    private readonly MailConfigurationSection _appMailConfiguration = appMailConfiguration.Value;

    /// <summary>
    /// Sends an email message asynchronously using the configured SMTP server.
    /// </summary>
    /// <param name="request">The mail message to send, including sender, recipients, subject, body, and optional attachments.</param>
    /// <returns>A task representing the asynchronous send operation.</returns>
    /// <remarks>
    /// This method constructs a MIME message from the provided <paramref name="request"/>, connects to the SMTP server,
    /// authenticates using the configured credentials, sends the message, and disconnects.
    /// If the message includes attachments via <see cref="MailMessage.Parts"/>, they are added as MIME attachments.
    /// The sender address and display name can be overridden per message or fall back to configuration defaults.
    /// </remarks>
    public async Task SendAsync(MailMessage request)
    {
        var message = new MimeMessage();
        var bodyBuilder = new BodyBuilder();

        message.From.Add(new MailboxAddress(request.From.DisplayName ?? _appMailConfiguration.DisplayName,
                                            request.From.Address ?? _appMailConfiguration.From));
        message.To.AddRange(request.To.Select(e => new MailboxAddress(e.DisplayName, e.Address)));

        message.Subject = request.Subject;
        bodyBuilder.HtmlBody = request.Body;
        message.Body = bodyBuilder.ToMessageBody();

        if (request.Parts != null && request.Parts.Count > 0)
        {
            var multipart = new Multipart("mixed")
                {
                    message.Body
                };

            foreach (var item in request.Parts)
            {
                var attachment = new MimePart(item.MediaType, item.MediaExtension)
                {
                    Content = new MimeContent(item.Content),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64
                };

                multipart.Add(attachment);
            }

            message.Body = multipart;
        }

        using (var client = new SmtpClient())
        {
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            await client.ConnectAsync(_appMailConfiguration.Host, _appMailConfiguration.Port, SecureSocketOptions.Auto);
            await client.AuthenticateAsync(_appMailConfiguration.UserName, _appMailConfiguration.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}