namespace ClickSphere_API.Models.Requests;
public class OllamaResponse
{
    public required string model { get; set; }
    public required DateTime created_at { get; set; }
    public required string response { get; set; }
    public required bool done { get; set; }
}

public class Response
{
    public string query { get; set; }
    public string answer { get; set; }
    public string explanation { get; set; }
    public string sql { get; set; }
}