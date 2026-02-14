namespace OperationalService.Api.Schemas;

/// <summary>
/// A definition for the retrieval of a single worker object.
/// </summary>
public sealed class Worker
{
    /// <summary>
    /// The unique identified of the worker.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// The name of the retrieved worker.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The status of the worker.
    /// </summary>
    public required string Status { get; set; }
}
