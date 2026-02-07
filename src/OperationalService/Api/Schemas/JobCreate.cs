namespace OperationalService.Api.Schemas;

/// <summary>
/// A definition for the creation of a job object.
/// </summary>
public sealed class JobCreate
{
    /// <summary>
    /// The name of the job being created.
    /// </summary>
    public required string Name { get; set; }
}
