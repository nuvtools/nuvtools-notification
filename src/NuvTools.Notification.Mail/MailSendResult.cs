namespace NuvTools.Notification.Mail;

/// <summary>
/// Represents the outcome of a send request accepted by the mail provider.
/// </summary>
/// <param name="Reference">
/// The provider reference of the send request, such as the message identifier assigned by an SMTP server
/// or the operation identifier returned by an HTTP based provider.
/// </param>
/// <param name="TrackingLocation">
/// The optional absolute URL that can be queried to track the delivery status, when the provider exposes one.
/// </param>
/// <remarks>
/// A result means the request was accepted by the provider, not that the email was delivered.
/// </remarks>
public sealed record MailSendResult(string? Reference, string? TrackingLocation = null);
