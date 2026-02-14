namespace Messaging.JobLifecyleEvents;

/// <summary>
/// Describes an event where the status of a job has changed.
/// </summary>
public sealed record JobStatusChangeEventBody : MQEventBody
{
    /// <summary>
    /// The unique ID of the job for which the status has changed.
    /// </summary>
    public required string JobId {  get; init; }
    
    /// <summary>
    /// The previous status of the job.
    /// </summary>
    public required string PreviousStatus { get; init; }

    /// <summary>
    /// The new status of the job.
    /// </summary>
    public required string NewStatus { get; init; }
}
