namespace OperationalService.BusinessLogic.Objects;

public sealed class JobServiceObject
{   
    /// <summary>
    /// The unique identifier of the job.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the job.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The status of the job.
    /// </summary>
    public string? Status { get; set; }
}
