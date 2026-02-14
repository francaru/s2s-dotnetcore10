namespace Messaging.WorkerLifecycleEvents;

/// <summary>
/// Describes an event where a worker is started.
/// </summary>
public sealed record WorkerStartEventBody : MQEventBody
{
    /// <summary>
    /// The name of the worker that has just started.
    /// </summary>
    public required string WorkerName { get; init; }
}
