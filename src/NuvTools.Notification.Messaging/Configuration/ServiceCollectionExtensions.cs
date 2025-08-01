using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NuvTools.Notification.Messaging.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures the messaging section (<see cref="MessagingQueueSection"/>) for use with TOptions injection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the configuration to.</param>
    /// <param name="configuration">The application <see cref="IConfiguration"/> instance containing the messaging section.</param>
    /// <param name="sectionName">The configuration section name to bind. Defaults to "NuvTools.Notification.Messaging".</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddMessagingQueueConfiguration<T>(
                   this IServiceCollection services,
                   IConfiguration configuration, string sectionName = "NuvTools.Notification.Messaging") where T : MessagingQueueSection
    {
        services.Configure<T>(configuration.GetSection(sectionName));
        return services;
    }
}