namespace ClickSphere_API.Models;

/// <summary>
/// Represents the columns of a view.
/// </summary>
public class ViewColumns
{
    /// <summary>
    /// Gets or sets the ViewID.
    /// </summary>
    public string? ViewID { get; set; }

    /// <summary>
    /// Gets or sets the name of the column.
    /// </summary>
    public string? ColumnName { get; set; }

    /// <summary>
    /// Gets or sets the data type of the column.
    /// </summary>
    public string? DataType { get; set; }
    
    /// <summary>
    /// Gets or sets the control type.
    /// </summary>
    public string? ControlType { get; set; }
}
