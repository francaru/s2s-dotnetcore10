namespace Database.Entities;

/// <summary>
/// A definition for storing the details of an immutable job object.
/// </summary>
public sealed class WorkerEntity
{
    /// <summary>
    /// The unique ID of the worker.
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// The name of the worker.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The WorkerInfo instance that links to this worker object.
    /// </summary>
    public WorkerInfoEntity? WorkerInfo { get; set; }

    /// <summary>
    /// Job instances that link to this worker object.
    /// </summary>
    public IEnumerable<JobEntity>? Jobs { get; set; }
}
