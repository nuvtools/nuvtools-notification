using System.Net;

namespace NuvTools.Notification.Mail.Twilio;

/// <summary>
/// Represents an error returned by the Twilio Email API.
/// </summary>
/// <remarks>
/// Twilio validates the whole payload before sending, returning as many issues as possible in a single
/// response. The raw response body is preserved in <see cref="ResponseContent"/> so the validation
/// details are not lost.
/// </remarks>
public class TwilioMailException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TwilioMailException"/> class.
    /// </summary>
    /// <param name="statusCode">The HTTP status code returned by the API.</param>
    /// <param name="responseContent">The raw response body returned by the API.</param>
    public TwilioMailException(HttpStatusCode statusCode, string? responseContent)
        : base($"Twilio Email API request failed with status {(int)statusCode} ({statusCode}). Response: {responseContent}")
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TwilioMailException"/> class with a custom message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public TwilioMailException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TwilioMailException"/> class with a custom message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public TwilioMailException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Gets the HTTP status code returned by the API, when available.
    /// </summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// Gets the raw response body returned by the API, when available.
    /// </summary>
    public string? ResponseContent { get; }
}
