namespace ClickSphere_API.Models.Requests;
/// <summary>
/// Represents a request to create a table in a database.
/// </summary>
public class CreateTableRequest
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
    /// Gets or sets the columns of the table.
    /// </summary>
    public required string Columns { get; set; }
    
    /// <summary>
    /// Gets or sets the engine used for the table.
    /// </summary>
    public string? Engine { get; set; }
    
    /// <summary>
    /// Gets or sets the column used for ordering the table.
    /// </summary>
    public string? OrderBy { get; set; }
}
