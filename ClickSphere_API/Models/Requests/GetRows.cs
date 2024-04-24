namespace ClickSphere_API.Models.Requests;
/// <summary>
/// Represents a request to retrieve rows from a database table.
/// </summary>
public class GetRowsRequest
{
    /// <summary>
    /// Gets or sets the name of the database.
    /// </summary>
    public required string Database { get; set; }

    /// <summary>
    /// Gets or sets the name of the table.
    /// </summary>
    public required string Table { get; set; }

    /// <summary>
    /// Gets or sets the columns to retrieve.
    /// </summary>
    public required string Columns { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of rows to retrieve.
    /// </summary>
    public string? Limit { get; set; }

    /// <summary>
    /// Gets or sets the column to use for ordering the rows.
    /// </summary>
    public string? OrderBy { get; set; }
}
