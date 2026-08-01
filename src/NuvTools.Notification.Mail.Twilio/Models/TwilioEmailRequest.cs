using System.Text.Json.Serialization;

namespace NuvTools.Notification.Mail.Twilio.Models;

/// <summary>
/// Payload sent to <c>POST /v1/Emails</c>.
/// </summary>
internal sealed class TwilioEmailRequest
{
    [JsonPropertyName("from")]
    public required TwilioEmailSender From { get; set; }

    [JsonPropertyName("to")]
    public required IList<TwilioEmailRecipient> To { get; set; }

    [JsonPropertyName("content")]
    public required TwilioEmailContent Content { get; set; }

    [JsonPropertyName("tags")]
    public IDictionary<string, string>? Tags { get; set; }

    [JsonPropertyName("schedule")]
    public TwilioEmailSchedule? Schedule { get; set; }

    [JsonPropertyName("ipPoolName")]
    public string? IpPoolName { get; set; }
}
