using System.Text.RegularExpressions;
namespace ClickSphere_API.Services
{
    /// <summary>
    /// A service class for parsing and validating SQL queries to prevent SQL injection attacks.
    /// </summary>
    public partial class SqlParser : ISqlParser
    {
        private static readonly char[] separator = [' '];

        /// <summary>
        /// Parses the SQL query and returns a ParsedQuery object.
        /// </summary>
        /// <param name="query">The SQL query to parse.</param>
        /// <returns>A ParsedQuery object containing the parsed query information.</returns>
        public ParsedQuery Parse(string query)
        {
            // cut everthing before the first SELECT
            var indexOfSelect = query.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase);
            if (indexOfSelect != -1)
            {
                query = query[indexOfSelect..];
            }
            else
            {
                return new ParsedQuery { IsValid = false };
            }

            // trim query
            query = query.Trim();

            // cut semi-colon from the end of the query
            if (query.EndsWith(';'))
            {
                query = query[..(query.Length - 1)];
            }

            // Convert the query to uppercase
            var upperQuery = query.ToUpperInvariant();

            // Check if the query contains a semicolon that is not inside a string literal
            bool insideStringLiteral = false;
            int charIndex = 0;
            foreach (char c in query)
            {
                if (c == '\'' || c == '"')
                {
                    insideStringLiteral = !insideStringLiteral;
                }
                else if (c == ';' && !insideStringLiteral && charIndex != query.Length - 1)
                {
                    throw new ArgumentException("Multiple statements are not allowed");
                }
                charIndex++;
            }

            // Check if the query starts with a data modifying statement
            if (upperQuery.StartsWith("INSERT INTO") || upperQuery.StartsWith("UPDATE") || upperQuery.StartsWith("DELETE") ||
                upperQuery.StartsWith("DROP") || upperQuery.StartsWith("GRANT") || upperQuery.StartsWith("REVOKE") ||
                upperQuery.StartsWith("CREATE") || upperQuery.StartsWith("ALTER") || upperQuery.StartsWith("TRUNCATE") ||
                upperQuery.StartsWith("RENAME"))
            {
                throw new ArgumentException("Data modifying statements are not allowed");
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
            // filter only the keywords that are used in the application
            var keywords = new[] { "SELECT", "FROM", "WHERE", "AND", "OR", "NOT", "IN", "BETWEEN", "LIKE", "IS", "NULL", "ORDER", "ASC", "DESC", "LIMIT", "OFFSET", "BY", "ORDER"};
            return keywords.Contains(word.ToUpperInvariant());
        }

        private static bool IsValidName(string name)
        {
            // Only allow alphanumeric characters and some special chars used in SQL
            return MyRegex().IsMatch(name);
        }

        [GeneratedRegex(@"^[\*a-zA-ZäöüßÄÖÜ0-9_\s`'""@!=<>\(\)%\.\,:\-\/\+\%\[\]]+$")]
        private static partial Regex MyRegex();
    }

    /// <summary>
    /// Represents a parsed SQL query.
    /// </summary>
    public class ParsedQuery
    {
        /// <summary>
        /// Gets or sets a value indicating whether the query is valid.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the sanitized query.
        /// </summary>
        public string? SanitizedQuery { get; set; }
    }
}
