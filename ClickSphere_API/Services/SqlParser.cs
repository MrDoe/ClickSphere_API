using System.Text.RegularExpressions;
namespace ClickSphere_API.Services;

/*
* A service class for parsing and validating SQL queries to prevent SQL injection attacks.
*/
public class SqlParser : ISqlParser
{
    private static readonly char[] separator = [' ', '\t', '\n', '\r', ',', '(', ')', ';', '=', '<', '>', '+', '-', '*', '/'];

    public ParsedQuery Parse(string query)
    {
        // Convert the query to uppercase and trim leading and trailing whitespace
        var upperQuery = query.Trim().ToUpperInvariant();

        // Check if the query contains a semicolon that is not inside a string literal
        bool insideStringLiteral = false;
        foreach (char c in query)
        {
            if (c == '\'')
            {
                insideStringLiteral = !insideStringLiteral;
            }
            else if (c == ';' && !insideStringLiteral)
            {
                throw new ArgumentException("Multiple statements are not allowed");
            }
        }

        // Check if the query starts with a data modifying statement
        if (upperQuery.StartsWith("INSERT INTO") || upperQuery.StartsWith("UPDATE") || upperQuery.StartsWith("DELETE") ||
            upperQuery.StartsWith("DROP") || upperQuery.StartsWith("GRANT") || upperQuery.StartsWith("REVOKE") ||
            upperQuery.StartsWith("CREATE") || upperQuery.StartsWith("ALTER") || upperQuery.StartsWith("TRUNCATE") ||
            upperQuery.StartsWith("RENAME"))
        {
            throw new ArgumentException("Data modifying statements are not allowed");
        }

        // Check if the query is a SELECT statement
        if (!upperQuery.StartsWith("SELECT"))
        {
            return new ParsedQuery { IsValid = false };
        }

        // Split the query into words
        var words = query.Split(separator, StringSplitOptions.RemoveEmptyEntries);

        // Check each word
        foreach (var word in words)
        {
            // If the word is a SQL keyword, skip it
            if (IsSqlKeyword(word))
            {
                continue;
            }

            // If the word is not a valid table or column name, return an invalid query
            if (!IsValidName(word))
            {
                return new ParsedQuery { IsValid = false };
            }
        }

        // If all words are valid, return the sanitized query
        return new ParsedQuery { IsValid = true, SanitizedQuery = query };
    }

    private static bool IsSqlKeyword(string word)
    {
        var keywords = new[] { "SELECT", "FROM", "WHERE", "AND", "OR", "NOT", "IN", "BETWEEN", "LIKE", "IS", "NULL", "AS", "JOIN", "ON", "INNER", "OUTER", "LEFT", "RIGHT", "FULL", "CROSS", "UNION", "ALL", "EXCEPT", "INTERSECT", "GROUP", "BY", "HAVING", "ORDER", "ASC", "DESC", "LIMIT", "OFFSET" };
        return keywords.Contains(word.ToUpperInvariant());
    }

    private static bool IsValidName(string name)
    {
        // Only allow alphanumeric characters, underscores and spaces in table and column names
        return Regex.IsMatch(name, @"^[*a-zA-Z0-9_\s`']+$");
    }
}

public class ParsedQuery
{
    public bool IsValid { get; set; }
    public string? SanitizedQuery { get; set; }
}