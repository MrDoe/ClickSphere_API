namespace ClickSphere_API.Models.Requests;
/// <summary>
/// Represents the request to store a view column as embedding.
/// </summary>
public class StoreViewColumnRequest
{
    /// <summary>
    /// The name of the database.
    /// </summary>
    public required string database { get; set; }

    /// <summary>
    /// The name of the table/view.
    /// </summary>
    public required string tableName { get; set; }

    /// <summary>
    /// The column to store as embedding.
    /// </summary>
    public required string dataColumn { get; set; }

    /// <summary>
    /// The column to use as key for the embedding.
    /// </summary>
    public required string keyColumn { get; set; }
}