using System.Text.Json.Serialization;

namespace NuvTools.Notification.Mail.Twilio.Models;

/// <summary>
/// Sending identity associated with the email.
/// </summary>
internal sealed class TwilioEmailSender
{
    [JsonPropertyName("address")]
    public required string Address { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
