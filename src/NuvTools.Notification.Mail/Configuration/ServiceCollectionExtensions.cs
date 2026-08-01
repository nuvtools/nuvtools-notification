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
    /// <typeparam name="T">
    /// The configuration class to bind, either <see cref="MailConfigurationSection"/> or a provider specific
    /// class derived from it.
    /// </typeparam>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to which the configuration will be added.
    /// </param>
    /// <param name="configuration">
    /// The application <see cref="IConfiguration"/> instance containing the mail configuration section.
    /// </param>
    /// <param name="sectionName">
    /// The configuration section name to bind. Defaults to <c>"Mail"</c>.
    /// </param>
    /// <returns>
    /// The updated <see cref="IServiceCollection"/> instance, enabling method chaining.
    /// </returns>
    /// <remarks>
    /// This method binds the specified configuration section to the <typeparamref name="T"/> options class,
    /// allowing it to be injected via <c>IOptions&lt;T&gt;</c> throughout the application.
    /// </remarks>
    public static IServiceCollection AddMailConfiguration<T>(
                   this IServiceCollection services,
                   IConfiguration configuration, string sectionName = "Mail") where T : MailConfigurationSection
    {
        services.Configure<T>(configuration.GetSection(sectionName));
        return services;
    }
}
