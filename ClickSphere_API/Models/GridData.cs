namespace ClickSphere_API.Models;
/// <summary>
/// Represents the data of a grid.
/// </summary>
public class GridData
{
    /// <summary>
    /// Gets or sets the rows of the grid.
    /// </summary>
    public required IEnumerable<Dictionary<string, object>> Rows { get; set; }
    
    /// <summary>
    /// Gets or sets the columns of the grid.
    /// </summary>     
    
    public required HashSet<string> Columns { get; set; }
    
    /// <summary>
    /// Gets or sets the total row count of the grid.
    /// </summary>
    public required int TotalRowCount { get; set; }
}