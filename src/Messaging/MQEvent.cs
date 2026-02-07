namespace Messaging;

/// <summary>
/// A definition for transmitting an event to a recipient.
/// </summary>
/// <typeparam name="T">The type of the event being transmitted.</typeparam>
public sealed record MQEvent<T> where T : MQEventBody 
{
    /// <summary>
    /// The core content of the event.
    /// </summary>
    public required T Body { get; init; }

    /// <summary>
    /// Metadata on the sender of the event.
    /// </summary>
    public required MQEventRecipient Recipient { get; init; }
}
