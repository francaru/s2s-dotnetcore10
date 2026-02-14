namespace Database.Entities;

/// <summary>
/// A definition for storing the details of an immutable job object.
/// </summary>
public sealed class JobEntity
{
    /// <summary>
    /// The unique ID of the job.
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// The unique ID of the worker handling the job.
    /// </summary>
    public required Guid WorkerId { get; set; }

    /// <summary>
    /// The name of the job.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The JobInfo instance that links to this job object.
    /// </summary>
    public JobInfoEntity? JobInfo { get; set; }

    /// <summary>
    /// The Worker instance that links to the this job object.
    /// </summary>
    public WorkerEntity? Worker { get; set; }
}
