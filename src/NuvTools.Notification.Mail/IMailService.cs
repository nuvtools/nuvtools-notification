namespace NuvTools.Notification.Mail;

/// <summary>
/// Provides email sending, regardless of the underlying infrastructure.
/// </summary>
/// <remarks>
/// This is the only contract the application layer needs to depend on. Provider specific packages
/// contain nothing but the implementation and its configuration.
/// </remarks>
public interface IMailService
{
    /// <summary>
    /// Send mail based on selected infraestructure.
    /// </summary>
    /// <param name="mailMessage">Message abstraction containg the content and its recipients.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The provider reference of the accepted send request.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the message uses a feature the provider cannot honor, such as
    /// <see cref="MailMessage.ScheduledFor"/>, <see cref="MailMessage.Tags"/> or
    /// <see cref="MailAddress.Variables"/>.
    /// </exception>
    Task<MailSendResult> SendAsync(MailMessage mailMessage, CancellationToken cancellationToken = default);
}
