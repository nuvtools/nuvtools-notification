using System.Threading.Tasks;

namespace NuvTools.Notification.Mail;

public interface IMailService
{
    /// <summary>
    /// Send mail based on selected infraestructure.
    /// </summary>
    /// <param name="mailMessage">Message abstraction containg the content and its recipients.</param>
    /// <returns></returns>
    Task SendAsync(MailMessage mailMessage);
}