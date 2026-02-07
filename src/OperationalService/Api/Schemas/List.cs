namespace OperationalService.Api.Schemas;

/// <summary>
/// An abstract definition for containing a collection of retrieved items with the applied pagination.
/// </summary>
/// <typeparam name="T">The generic type of the collection.</typeparam>
public abstract class List<T> where T : class
{
    /// <summary>
    /// The collection of items.
    /// </summary>
    public required IEnumerable<T> Data { get; set; }

    /// <summary>
    /// Pagination that was applied on the retrieved items.
    /// </summary>
    public required Pagination Pagination { get; set; }
}
