using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NuvTools.Notification.Messaging.Configuration;

/// <summary>
/// Provides extension methods for configuring messaging-related services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers and configures a messaging section for options-based dependency injection.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the messaging section configuration class. Must inherit from <see cref="MessagingSection"/>.
    /// </typeparam>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to which the configuration will be added.
    /// </param>
    /// <param name="configuration">
    /// The application <see cref="IConfiguration"/> instance containing the messaging section.
    /// </param>
    /// <param name="sectionName">
    /// The configuration section name to bind. Defaults to <c>"NuvTools.Notification.Messaging"</c>.
    /// </param>
    /// <returns>
    /// The updated <see cref="IServiceCollection"/> instance, enabling method chaining.
    /// </returns>
    /// <remarks>
    /// This method binds the specified configuration section to the <typeparamref name="T"/> options class,
    /// allowing it to be injected via <c>IOptions&lt;T&gt;</c> throughout the application.
    /// </remarks>
    public static IServiceCollection AddMessagingQueueConfiguration<T>(
                   this IServiceCollection services,
                   IConfiguration configuration, string sectionName = "NuvTools.Notification.Messaging") where T : MessagingSection
    {
        services.Configure<T>(configuration.GetSection(sectionName));
        return services;
    }
}