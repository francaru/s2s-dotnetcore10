namespace OperationalService.Api.Schemas;

/// <summary>
/// A definition for storing information on the health of the server.
/// </summary>
public sealed class Health
{
    /// <summary>
    /// The total time the server has been up.
    /// </summary>
    public required TimeSpan UpTime { get; set; }
}
