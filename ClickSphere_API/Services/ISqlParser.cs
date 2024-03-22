namespace ClickSphere_API.Services
{
    /// <summary>
    /// SQL parser for the validation of SQL queries.
    /// </summary>
    public interface ISqlParser
    {
        /// <summary>
        /// Parses a given SQL query.
        /// </summary>
        /// <param name="sqlQuery">The SQL query to parse.</param>
        /// <returns>The parsed query.</returns>
        ParsedQuery Parse(string sqlQuery);
    }
}