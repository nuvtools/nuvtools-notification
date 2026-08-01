using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using NuvTools.Notification.Mail.Configuration;
using System.Net.Http.Headers;
using System.Text;

namespace NuvTools.Notification.Mail.Twilio.Configuration;

/// <summary>
/// Provides extension methods for configuring the Twilio Email services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Name of the registered <see cref="HttpClient"/>.
    /// </summary>
    public const string HttpClientName = "TwilioMail";

    /// <summary>
    /// The default resilience policy applied to the Twilio Email client:
    /// 2 retries with 1s delay, 30s per-attempt timeout, 60s circuit-breaker sampling,
    /// 90s total request timeout.
    /// </summary>
    public static readonly Action<HttpStandardResilienceOptions> DefaultResilience = options =>
    {
        options.Retry.MaxRetryAttempts = 2;
        options.Retry.Delay = TimeSpan.FromSeconds(1);
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(60);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(90);
    };

    /// <summary>
    /// Registers and configures the Twilio Email service for options-based dependency injection.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to which the services will be added.
    /// </param>
    /// <param name="configuration">
    /// The application <see cref="IConfiguration"/> instance containing the Twilio mail configuration section.
    /// </param>
    /// <param name="sectionName">
    /// The configuration section name to bind. Defaults to <c>"MailTwilio"</c>.
    /// </param>
    /// <param name="configureResilience">
    /// Optional delegate to customize the resilience pipeline. Defaults to <see cref="DefaultResilience"/>.
    /// </param>
    /// <returns>
    /// The updated <see cref="IServiceCollection"/> instance, enabling method chaining.
    /// </returns>
    /// <remarks>
    /// This method binds the specified configuration section to <see cref="TwilioMailConfigurationSection"/>
    /// and registers <see cref="TwilioMailService"/> as a typed <see cref="HttpClient"/> with the API base
    /// address, Basic authentication header and a standard resilience pipeline.
    /// The service is resolved as <see cref="IMailService"/>, so the application layer depends only on the
    /// NuvTools.Notification.Mail abstraction.
    /// </remarks>
    public static IServiceCollection AddTwilioMail(
                   this IServiceCollection services,
                   IConfiguration configuration,
                   string sectionName = "MailTwilio",
                   Action<HttpStandardResilienceOptions>? configureResilience = null)
    {
        services.AddMailConfiguration<TwilioMailConfigurationSection>(configuration, sectionName);

        services.AddHttpClient<IMailService, TwilioMailService>(HttpClientName, (serviceProvider, client) =>
        {
            var mailConfiguration = serviceProvider.GetRequiredService<IOptions<TwilioMailConfigurationSection>>().Value;

            var baseUrl = string.IsNullOrWhiteSpace(mailConfiguration.BaseUrl)
                            ? TwilioMailConfigurationSection.DefaultBaseUrl
                            : mailConfiguration.BaseUrl;

            client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + '/');

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes($"{mailConfiguration.AccountSid}:{mailConfiguration.AuthToken}")));
        })
        .AddStandardResilienceHandler(configureResilience ?? DefaultResilience);

        return services;
    }
}
