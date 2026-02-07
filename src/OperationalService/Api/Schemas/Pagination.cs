namespace OperationalService.Api.Schemas;

/// <summary>
/// Defines pagination information for when reading collections.
/// </summary>
public sealed class Pagination
{
    /// <summary>
    /// The current page number.
    /// </summary>
    public required int PageNumber { get; set; }

    /// <summary>
    /// The number of items that are allowed in each page.
    /// </summary>
    public required int PageSize { get; set; }

    /// <summary>
    /// The total number of items in the collection before applying pagination.
    /// </summary>
    public required int TotalCount { get; set; }
}
