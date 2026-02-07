namespace Messaging;

/// <summary>
/// A definition on the metadata of an event.
/// </summary>
public sealed class MQEventInfo
{
    /// <summary>
    /// The name of the queue that received the event.
    /// </summary>
    public required string QueueName {  get; init; }
    
    /// <summary>
    /// The sender recipient that sent the event.
    /// </summary>
    public required MQEventRecipient Sender { get; init; }
}
