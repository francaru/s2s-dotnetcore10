namespace Database.Entities;

/// <summary>
/// A definition for storing additional mutable details for a given job object.
/// </summary>
public sealed class JobInfoEntity
{
    /// <summary>
    /// The unique ID of the job being annotated.
    /// </summary>
    public Guid JobId { get; set; }

    /// <summary>
    /// The Job instance that links to this JobInfo object.
    /// </summary>
    public JobEntity? Job { get; set; }

    /// <summary>
    /// The status of the job (starting | running | complete).
    /// </summary>
    public string? Status { get; set; }
}
