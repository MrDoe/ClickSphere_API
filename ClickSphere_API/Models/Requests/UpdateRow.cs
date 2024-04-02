namespace ClickSphere_API.Models.Requests;
public class UpdateRowRequest
{
    public required string Database { get; set; }
    public required string Table { get; set; }
    public required string Values { get; set; }
    public required string Condition { get; set; }
}
