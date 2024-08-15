namespace ClickSphere_API.Models.Requests;
/// <summary>
/// Request to generate a query based on a question and a table.
/// </summary>
public class GenerateQueryRequest
{
    /// <summary>
    /// The question to convert into a query.
    /// </summary>
    public string Question { get; set; }

    /// <summary>
    /// The database to execute the query on.
    /// </summary>
    public string Database { get; set; }

    /// <summary>
    /// The table/view to ask the question about.
    /// </summary>
    public string Table { get; set; }
}
