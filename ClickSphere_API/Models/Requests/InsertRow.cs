namespace ClickSphere_API.Models.Requests;
/// <summary>
/// Represents a request to insert a row into a database table.
/// </summary>
public class InsertRowRequest
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
    /// Gets or sets the values to be inserted into the table.
    /// </summary>
    public required string Values { get; set; }
}
