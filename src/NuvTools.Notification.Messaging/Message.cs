namespace NuvTools.Notification.Messaging;

/// <summary>
/// Represents a message envelope that wraps a strongly-typed payload and common messaging metadata.
/// </summary>
/// <typeparam name="T">The type of the message body. Must be a reference type.</typeparam>
/// <param name="body">The message payload.</param>
/// <remarks>
/// This class is intended as a lightweight envelope used by message producers and consumers within
/// the NuvTools.Notification.Messaging library. It carries the payload in <see cref="Body"/> and common
/// metadata such as <see cref="MessageId"/>, <see cref="CorrelationId"/>, <see cref="Subject"/>,
/// <see cref="TimeToLive"/>, and a flexible <see cref="Properties"/> map for custom headers.
/// 
/// The default constructor generates a new <see cref="MessageId"/>. The class itself does not impose
/// any serialization or transport requirements — those are handled by the concrete sender/consumer
/// implementations.
/// </remarks>
/// <example>
/// <code language="c#">
/// var msg = new Message&lt;Order&gt;(order)
/// {
///     Subject = "order.created",
///     CorrelationId = correlationId,
///     TimeToLive = TimeSpan.FromMinutes(30),
///     Properties = { ["priority"] = "high" }
/// };
/// </code>
/// </example>
public class Message<T>(T body) where T : class
{
    /// <summary>
    /// A unique identifier for this message. Defaults to a newly generated GUID string.
    /// </summary>
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Optional correlation identifier used to group related messages or correlate request/response flows.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Optional subject or routing key describing the purpose or category of the message.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Optional time-to-live for the message. When set, senders or transports may discard the message after this interval.
    /// </summary>
    public TimeSpan? TimeToLive { get; set; }

    /// <summary>
    /// Free-form dictionary for additional metadata/headers. Keys are strings and values can be any object.
    /// Consumers and transports may interpret these entries as needed.
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = [];

    /// <summary>
    /// The strongly-typed message payload.
    /// </summary>
    public T Body { get; set; } = body;
}
