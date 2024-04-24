namespace ClickSphere_API.Models.Requests;
/// <summary>
/// Represents a request to update a row in a database table.
/// </summary>
public class UpdateRowRequest
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
    /// Gets or sets the values to be updated.
    /// </summary>
    public required string Values { get; set; }
    
    /// <summary>
    /// Gets or sets the condition for updating the row.
    /// </summary>
    public required string Condition { get; set; }
}
