using Azure.Messaging.ServiceBus;
using NuvTools.Notification.Messaging.Interfaces;

namespace NuvTools.Notification.Messaging.Azure.ServiceBus;

internal class AzureMessageContext(ProcessMessageEventArgs args) : IMessageContext
{
    public Task CompleteAsync(CancellationToken cancellationToken = default)
        => args.CompleteMessageAsync(args.Message, cancellationToken);

    public Task AbandonAsync(CancellationToken cancellationToken = default)
        => args.AbandonMessageAsync(args.Message, cancellationToken: cancellationToken);

    public Task DeadLetterAsync(string reason, string? errorDescription = null, CancellationToken cancellationToken = default)
        => args.DeadLetterMessageAsync(args.Message, reason, errorDescription, cancellationToken);
}
