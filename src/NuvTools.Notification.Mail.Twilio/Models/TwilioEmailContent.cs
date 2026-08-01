using System.Text.Json.Serialization;

namespace NuvTools.Notification.Mail.Twilio.Models;

/// <summary>
/// Content of the email, including attachments and custom headers.
/// </summary>
internal sealed class TwilioEmailContent
{
    [JsonPropertyName("subject")]
    public required string Subject { get; set; }

    [JsonPropertyName("html")]
    public required string Html { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("attachments")]
    public IList<TwilioEmailAttachment>? Attachments { get; set; }

    [JsonPropertyName("headers")]
    public IDictionary<string, string>? Headers { get; set; }
}
