using Azure.Messaging.ServiceBus;
using NuvTools.Notification.Messaging.Interfaces;

namespace NuvTools.Notification.Messaging.Azure.ServiceBus;

internal class AzureMessageContext(ProcessMessageEventArgs args) : IMessageContext
{
    public bool IsMessageCompleted { get; private set; }

    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        if (IsMessageCompleted) return;
        await args.CompleteMessageAsync(args.Message, cancellationToken);
        IsMessageCompleted = true;
    }

    public async Task AbandonAsync(CancellationToken cancellationToken = default)
    {
        if (IsMessageCompleted) return;
        await args.AbandonMessageAsync(args.Message, cancellationToken: cancellationToken);
        IsMessageCompleted = true;
    }

    public async Task DeadLetterAsync(string reason, string? errorDescription = null, CancellationToken cancellationToken = default)
    {
        if (IsMessageCompleted) return;
        await args.DeadLetterMessageAsync(args.Message, reason, errorDescription, cancellationToken);
        IsMessageCompleted = true;
    }
}
