namespace Messaging.WorkEvents;

/// <summary>
/// Describes an event where a job is requested to do work.
/// </summary>
public sealed record JobDoWorkEventBody : MQEventBody
{
    /// <summary>
    /// The unique ID of the job that requires work.
    /// </summary>
    public required string JobId { get; init; }
}
