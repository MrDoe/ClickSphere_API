namespace ClickSphere_API.Services;
public interface IAiService
{
    Task<string> Ask(string question);
    Task<string> GenerateQuery(string question, string database, string table);
    Task<string> GenerateAndExecuteQuery(string question, string database, string table);
}
