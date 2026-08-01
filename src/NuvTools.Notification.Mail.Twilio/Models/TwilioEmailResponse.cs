using System.Text.Json.Serialization;

namespace NuvTools.Notification.Mail.Twilio.Models;

/// <summary>
/// Response returned by <c>POST /v1/Emails</c> when the request is accepted.
/// </summary>
internal sealed class TwilioEmailResponse
{
    [JsonPropertyName("operationId")]
    public string? OperationId { get; set; }

    [JsonPropertyName("operationLocation")]
    public string? OperationLocation { get; set; }
}
