using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NuvTools.Notification.Mail.Configuration;

namespace NuvTools.Notification.Mail.Smtp.Configuration;

/// <summary>
/// Provides extension methods for configuring the SMTP mail service in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers and configures the SMTP mail service for options-based dependency injection.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to which the services will be added.
    /// </param>
    /// <param name="configuration">
    /// The application <see cref="IConfiguration"/> instance containing the SMTP configuration section.
    /// </param>
    /// <param name="sectionName">
    /// The configuration section name to bind. Defaults to <c>"MailSmtp"</c>.
    /// </param>
    /// <returns>
    /// The updated <see cref="IServiceCollection"/> instance, enabling method chaining.
    /// </returns>
    /// <remarks>
    /// This method binds the specified configuration section to <see cref="SmtpMailConfigurationSection"/>
    /// and registers <see cref="SMTPMailService"/> as <see cref="IMailService"/>, so the application layer
    /// depends only on the NuvTools.Notification.Mail abstraction.
    /// </remarks>
    public static IServiceCollection AddSmtpMail(
                   this IServiceCollection services,
                   IConfiguration configuration,
                   string sectionName = "MailSmtp")
    {
        services.AddMailConfiguration<SmtpMailConfigurationSection>(configuration, sectionName);
        services.AddSingleton<IMailService, SMTPMailService>();

        return services;
    }
}
