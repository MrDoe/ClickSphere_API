namespace ClickSphere_API.Models.Requests;
public class InsertRowRequest
{
    public required string Database { get; set; }
    public required string Table { get; set; }
    public required string Values { get; set; }
}
