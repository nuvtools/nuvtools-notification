using System.Text.Json.Serialization;

namespace NuvTools.Notification.Mail.Twilio.Models;

/// <summary>
/// Delivery schedule of the email.
/// </summary>
internal sealed class TwilioEmailSchedule
{
    [JsonPropertyName("sendAt")]
    public required IList<string> SendAt { get; set; }
}
