namespace ClickSphere_API.Models.Requests;
public class OllamaRequest
{
    public string model { get; set; }
    public string prompt { get; set; }
    public bool stream { get; set; }
    public string system { get; set; }
}
