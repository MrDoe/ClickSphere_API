namespace ClickSphere_API.Services;
public interface IAiService
{
    Task<string> Ask(string question);
    Task<string> GenerateQuery(string question, string table);
}
