using Microsoft.AspNetCore.Mvc;
using ClickSphere_API.Services;
namespace ClickSphere_API.Controllers;

/// <summary>
/// The base class for ODBC connections from ClickHouse to Microsoft SQL Server.
/// </summary>
[ApiController]
public class OdbcController(IDbService dbService, IApiViewService viewService) : ControllerBase
{
    private readonly IDbService _dbService = dbService;
    private readonly IApiViewService _viewService = viewService;

    /// <summary>
    /// Get the version of the SQL Server.
    /// </summary>
    /// <returns>The version of the server.</returns>
    [HttpGet]
    [Route("/odbc/version")]
    public async Task<string> GetVersion()
    {
        var version = await _dbService.ExecuteScalar("SELECT @@VERSION");
        return $"SQL Server version: {version}";
    }

    /// <summary>
    /// Get columns from a table in the ODBC connection.
    /// </summary>
    /// <param name="table">The table to get.</param>
    /// <param name="column">The column to get.</param>
    /// <returns>The list of views.</returns>
    [HttpGet]
    [Route("/odbc/views")]
    public async Task<IList<string>> GetColumnFromODBC(string table, string column)
    {
        if(string.IsNullOrEmpty(table) || string.IsNullOrEmpty(column))
        {
            return [];
        }
        return await _dbService.GetColumnFromODBC(table, column);
    }

    /// <summary>
    /// Get a SQL Server view definition from the ODBC connection.
    /// </summary>
    /// <param name="view">The view to import from the ODBC database.</param>
    /// <param name="dropExisting">Whether to drop the existing view.</param>
    /// <returns>The view definition.</returns>
    [HttpGet]
    [Route("/odbc/importView")]
    public async Task<string> ImportViewFromODBC(string view, bool dropExisting = false)
    {
        if(string.IsNullOrEmpty(view))
        {
            return "Invalid request";
        }
        return await _viewService.ImportViewFromODBC(view, dropExisting);
    }
}