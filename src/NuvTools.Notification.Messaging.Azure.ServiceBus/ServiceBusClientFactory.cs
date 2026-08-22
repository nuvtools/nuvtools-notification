using Azure.Core;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using NuvTools.Notification.Messaging.Configuration;

namespace NuvTools.Notification.Messaging.Azure.ServiceBus;

/// <summary>
/// Builds <see cref="ServiceBusClient"/> instances from a <see cref="MessagingSection"/>, choosing between
/// shared-access-key and Entra ID authentication based on which connectivity settings the section declares.
/// </summary>
public static class ServiceBusClientFactory
{
    /// <summary>
    /// Creates a client for the supplied configuration section.
    /// </summary>
    /// <param name="section">The messaging configuration section.</param>
    /// <param name="credential">
    /// Credential to use when <see cref="MessagingSection.FullyQualifiedNamespace"/> authentication applies.
    /// When null, a <see cref="DefaultAzureCredential"/> is built from
    /// <see cref="MessagingSection.ManagedIdentityClientId"/>.
    /// </param>
    /// <returns>A client authenticated by connection string or by Entra ID credential.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="section"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the section declares neither a connection string nor a fully qualified namespace.
    /// </exception>
    public static ServiceBusClient Create(MessagingSection section, TokenCredential? credential = null)
    {
        ArgumentNullException.ThrowIfNull(section);

        if (!string.IsNullOrEmpty(section.ConnectionString))
            return new ServiceBusClient(section.ConnectionString);

        if (string.IsNullOrEmpty(section.FullyQualifiedNamespace))
            throw new InvalidOperationException(
                $"Messaging section '{section.Name}' must declare either {nameof(MessagingSection.ConnectionString)} or {nameof(MessagingSection.FullyQualifiedNamespace)}.");

        return new ServiceBusClient(section.FullyQualifiedNamespace, credential ?? CreateCredential(section));
    }

    private static TokenCredential CreateCredential(MessagingSection section)
        => new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            ManagedIdentityClientId = section.ManagedIdentityClientId
        });
}
