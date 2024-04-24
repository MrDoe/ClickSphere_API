namespace ClickSphere_API.Models.Requests;
/// <summary>
/// Represents a request to delete a row from a database table.
/// </summary>
public class DeleteRowRequest
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
    /// Gets or sets the condition for deleting the row.
    /// </summary>
    public required string Condition { get; set; }
}
