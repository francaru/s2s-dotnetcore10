namespace OperationalService.Api.Schemas;

/// <summary>
/// A definition for the creation of a worker object.
/// </summary>
public sealed class WorkerCreate
{
    /// <summary>
    /// The name of the worker being created.
    /// </summary>
    public required string Name { get; set; }
}
