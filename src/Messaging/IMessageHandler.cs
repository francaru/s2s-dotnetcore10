using Database;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Messaging;

/// <summary>
/// An interface for the mapping of a disposable message handler.
/// </summary>
public interface IMessageHandler : IDisposable
{
    /// <summary>
    /// The name of the service that owns the message handler.
    /// </summary>
    string ServiceName { get; }

    /// <summary>
    /// Create a queue to be registered by consumers.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    void CreateQueue(string queueName);

    /// <summary>
    /// Send out an event to consumers that are subscribed to the specified queues.
    /// </summary>
    /// <typeparam name="T">The type of the object being transmitted from the producer.</typeparam>
    /// <param name="toQueues">A collection of queue names that are eligible to receive the event.</param>
    /// <param name="mqEventBody">The object to transmit from the producer.</param>
    void Produce<T>(string[] toQueues, T mqEventBody) where T : MQEventBody;

    /// <summary>
    /// Subscribe to a queue.
    /// </summary>
    /// <typeparam name="T">The type of the object that is expected to be received when an event enters the queue.</typeparam>
    /// <param name="onQueue">The name of the queue on which to subscribe.</param>
    /// <param name="consumerActions">A collection of consumer functions that are triggered when an event enters the queue.</param>
    void Consume<T>(string onQueue, params Action<DatabaseContext, ActivitySource, ILoggerFactory, MQEventInfo, T?>[] consumerActions) where T : MQEventBody;
}
