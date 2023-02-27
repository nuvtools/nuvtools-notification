using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NuvTools.Notification.Mail.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures mail section (MailConfigurationSection) to use with TOptions injection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="sectionName">Just in case to use another section instead the default one.</param>
    /// <returns></returns>
    public static IServiceCollection AddMailConfiguration(
                   this IServiceCollection services,
                   IConfiguration configuration, string sectionName = "NuvTools.Notification.Mail")
    {
        services.Configure<MailConfigurationSection>(configuration.GetSection(sectionName));
        return services;
    }
}