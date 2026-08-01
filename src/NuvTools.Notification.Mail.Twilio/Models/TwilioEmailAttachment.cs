using System.Text.Json.Serialization;

namespace NuvTools.Notification.Mail.Twilio.Models;

/// <summary>
/// Base64 encoded attachment of the email.
/// </summary>
internal sealed class TwilioEmailAttachment
{
    [JsonPropertyName("filename")]
    public required string FileName { get; set; }

    [JsonPropertyName("contentType")]
    public required string ContentType { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }

    [JsonPropertyName("cid")]
    public string? Cid { get; set; }
}
