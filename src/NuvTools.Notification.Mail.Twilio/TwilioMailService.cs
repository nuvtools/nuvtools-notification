using Microsoft.Extensions.Options;
using NuvTools.Notification.Mail.Twilio.Configuration;
using NuvTools.Notification.Mail.Twilio.Models;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NuvTools.Notification.Mail.Twilio;

/// <summary>
/// Twilio Email API implementation of <see cref="IMailService"/>.
/// </summary>
/// <param name="httpClient">The typed client configured with the API base address and Basic authentication.</param>
/// <param name="appMailConfiguration">The mail configuration options containing the Twilio credentials and defaults.</param>
/// <remarks>
/// This service posts messages to the Twilio Email endpoint (<c>POST /v1/Emails</c>), which processes
/// requests asynchronously and answers with <c>202 Accepted</c> plus an operation identifier.
/// It supports HTML and plain text content, Base64 attachments, inline images, custom headers, tags,
/// Liquid personalization variables and scheduled delivery.
/// Register it through <see cref="Configuration.ServiceCollectionExtensions.AddTwilioMail"/>.
/// </remarks>
public class TwilioMailService(HttpClient httpClient, IOptions<TwilioMailConfigurationSection> appMailConfiguration) : IMailService
{
    /// <summary>
    /// Relative path of the Twilio Email send endpoint.
    /// </summary>
    public const string EmailsEndpoint = "v1/Emails";

    /// <summary>
    /// Maximum request size accepted by the Twilio Email API, including attachments.
    /// </summary>
    public const int MaxRequestSizeInBytes = 10 * 1024 * 1024;

    private const string RFC3339Format = "yyyy-MM-ddTHH:mm:ssZ";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly TwilioMailConfigurationSection _appMailConfiguration = appMailConfiguration.Value;

    /// <summary>
    /// Sends an email message asynchronously using the Twilio Email API.
    /// </summary>
    /// <param name="mailMessage">The mail message to send, including sender, recipients, subject, body, and optional attachments.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The identifier of the asynchronous operation created by the API, along with the URL to track it.</returns>
    /// <remarks>
    /// The returned result means the request was accepted for processing. The delivery status can be
    /// tracked through <see cref="MailSendResult.TrackingLocation"/>.
    /// </remarks>
    /// <exception cref="TwilioMailException">Thrown when the API rejects the request or answers unexpectedly.</exception>
    public async Task<MailSendResult> SendAsync(MailMessage mailMessage, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mailMessage);

        var payload = await BuildRequestAsync(mailMessage, cancellationToken).ConfigureAwait(false);

        var body = JsonSerializer.SerializeToUtf8Bytes(payload, JsonOptions);

        if (body.Length > MaxRequestSizeInBytes)
            throw new TwilioMailException($"The request size of {body.Length} bytes exceeds the Twilio Email API limit of {MaxRequestSizeInBytes} bytes.");

        using var content = new ByteArrayContent(body);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };

        using var response = await httpClient.PostAsync(EmailsEndpoint, content, cancellationToken).ConfigureAwait(false);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new TwilioMailException(response.StatusCode, responseContent);

        TwilioEmailResponse? result;

        try
        {
            result = JsonSerializer.Deserialize<TwilioEmailResponse>(responseContent, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new TwilioMailException($"The Twilio Email API returned an unexpected response: {responseContent}", ex);
        }

        return result?.OperationId is null
            ? throw new TwilioMailException($"The Twilio Email API returned no operation identifier: {responseContent}")
            : new MailSendResult(result.OperationId, result.OperationLocation);
    }

    private async Task<TwilioEmailRequest> BuildRequestAsync(MailMessage mailMessage, CancellationToken cancellationToken)
    {
        var fromAddress = mailMessage.From?.Address ?? _appMailConfiguration.From;

        if (string.IsNullOrWhiteSpace(fromAddress))
            throw new TwilioMailException("The sender address must be informed in the message or in the configuration.");

        if (mailMessage.To is null || mailMessage.To.Count == 0)
            throw new TwilioMailException("At least one recipient must be informed.");

        return new TwilioEmailRequest
        {
            From = new TwilioEmailSender
            {
                Address = fromAddress,
                Name = mailMessage.From?.DisplayName ?? _appMailConfiguration.DisplayName
            },
            To = [.. mailMessage.To.Select(e => new TwilioEmailRecipient
            {
                Address = e.Address,
                Name = e.DisplayName,
                Variables = e.Variables
            })],
            Content = new TwilioEmailContent
            {
                Subject = mailMessage.Subject,
                Html = mailMessage.Body,
                Text = mailMessage.TextBody,
                Headers = mailMessage.Headers,
                Attachments = await BuildAttachmentsAsync(mailMessage.Parts, cancellationToken).ConfigureAwait(false)
            },
            Tags = mailMessage.Tags,
            Schedule = mailMessage.ScheduledFor is null ? null : new TwilioEmailSchedule
            {
                SendAt = [mailMessage.ScheduledFor.Value.ToUniversalTime().ToString(RFC3339Format, CultureInfo.InvariantCulture)]
            },
            IpPoolName = _appMailConfiguration.IpPoolName
        };
    }

    private static async Task<IList<TwilioEmailAttachment>?> BuildAttachmentsAsync(List<MailPart>? parts, CancellationToken cancellationToken)
    {
        if (parts is null || parts.Count == 0) return null;

        var attachments = new List<TwilioEmailAttachment>(parts.Count);

        foreach (var part in parts)
        {
            using var buffer = new MemoryStream();
            await part.Content.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);

            attachments.Add(new TwilioEmailAttachment
            {
                FileName = part.FileName,
                ContentType = $"{part.MediaType}/{part.MediaExtension}",
                Content = Convert.ToBase64String(buffer.ToArray()),
                Cid = part.ContentId
            });
        }

        return attachments;
    }
}
