using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NuvTools.Notification.Mail.Configuration;

namespace NuvTools.Notification.Mail.Smtp;

public class SMTPMailService(IOptions<MailConfigurationSection> appMailConfiguration) : IMailService
{
    private readonly MailConfigurationSection _appMailConfiguration = appMailConfiguration.Value;

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