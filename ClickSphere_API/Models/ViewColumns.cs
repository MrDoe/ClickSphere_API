namespace ClickSphere_API.Models;
/// <summary>
/// Represents the columns of a view.
/// </summary>
public class ViewColumns
{
    /// <summary>
    /// Gets or sets the ID of the view column.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the database.
    /// </summary>
    public string? Database { get; set; }

    /// <summary>
    /// Gets or sets the ViewId.
    /// </summary>
    public string? ViewId { get; set; }

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

    /// <summary>
    /// Gets or sets the default value.
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the sorter.
    /// </summary>
    public uint? Sorter { get; set; }

    /// <summary>
    /// Gets or sets the operator. This field is not mapped to the db.
    /// </summary>
    public string? Operator { get; set; } = "=";

    /// <summary>
    /// Gets or sets the search value. This field is not mapped to the db.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the description of the column.
    /// </summary>
    public string? Description { get; set; }
}
