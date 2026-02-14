namespace OperationalService.BusinessLogic.Objects;

/// <summary>
/// A service object for storing information on a worker.
/// </summary>
public sealed class WorkerServiceObject
{
    /// <summary>
    /// The unique identifier of the worker.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the worker.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The status of the worker.
    /// </summary>
    public string? Status { get; set; }
}
