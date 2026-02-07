namespace Messaging;

/// <summary>
/// A definition that describes an event's recipient.
/// </summary>
public sealed record MQEventRecipient
{
    /// <summary>
    /// The name of the service that sent out the message.
    /// </summary>
    public required string ServiceName { get; init; }
}
