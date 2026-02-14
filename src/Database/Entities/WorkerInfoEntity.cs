namespace Database.Entities;

/// <summary>
/// A definition for storing additional mutable details for a given worker object.
/// </summary>
public sealed class WorkerInfoEntity
{
    /// <summary>
    /// The unique ID of the worker being annotated.
    /// </summary>
    public Guid WorkerId { get; set; }

    /// <summary>
    /// The worker instance that links to this WorkerInfo object.
    /// </summary>
    public WorkerEntity? Worker { get; set; }

    /// <summary>
    /// The status of the worker (starting | idle  | staged | busy | unreachable).
    /// </summary>
    public string? Status { get; set; }
}
