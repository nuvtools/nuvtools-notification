using System.Text.Json.Serialization;

namespace NuvTools.Notification.Mail.Twilio.Models;

/// <summary>
/// Recipient of the email, optionally carrying Liquid personalization variables.
/// </summary>
internal sealed class TwilioEmailRecipient
{
    [JsonPropertyName("address")]
    public required string Address { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("variables")]
    public IDictionary<string, object?>? Variables { get; set; }
}
