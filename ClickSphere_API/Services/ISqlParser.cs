namespace ClickSphere_API.Services;
public interface ISqlParser
{
    ParsedQuery Parse(string sqlQuery);
}