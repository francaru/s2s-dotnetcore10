namespace OperationalService.Api.Schemas;

/// <summary>
/// A definition for the retrieval of a single job object.
/// </summary>
public sealed class Job
{
    /// <summary>
    /// The unique identified of the job.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// The name of the retrieved job.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The status of the job.
    /// </summary>
    public required string Status { get; set; }
}
