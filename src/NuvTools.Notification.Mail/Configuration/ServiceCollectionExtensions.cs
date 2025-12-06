using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NuvTools.Notification.Mail.Configuration;

/// <summary>
/// Provides extension methods for configuring mail-related services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers and configures a mail configuration section for options-based dependency injection.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to which the configuration will be added.
    /// </param>
    /// <param name="configuration">
    /// The application <see cref="IConfiguration"/> instance containing the mail configuration section.
    /// </param>
    /// <param name="sectionName">
    /// The configuration section name to bind. Defaults to <c>"NuvTools.Notification.Mail"</c>.
    /// </param>
    /// <returns>
    /// The updated <see cref="IServiceCollection"/> instance, enabling method chaining.
    /// </returns>
    /// <remarks>
    /// This method binds the specified configuration section to the <see cref="MailConfigurationSection"/> options class,
    /// allowing it to be injected via <c>IOptions&lt;MailConfigurationSection&gt;</c> throughout the application.
    /// </remarks>
    public static IServiceCollection AddMailConfiguration(
                   this IServiceCollection services,
                   IConfiguration configuration, string sectionName = "NuvTools.Notification.Mail")
    {
        services.Configure<MailConfigurationSection>(configuration.GetSection(sectionName));
        return services;
    }
}