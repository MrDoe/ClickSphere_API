using Microsoft.AspNetCore.Mvc;
using ClickSphere_API.Services;
namespace ClickSphere_API.Controllers;

/// <summary>
/// The base class for ClickSphere API controllers.
/// </summary>
[ApiController]
public class ClickHouseController(IDbService dbService) : ControllerBase
{
    private readonly IDbService _dbService = dbService;

    /// <summary>
    /// Get the version of the ClickHouse server.
    /// </summary>
    /// <returns>The version of the server.</returns>
    [HttpGet]
    [Route("/version")]
    public async Task<string> GetVersion()
    {
        var version = await _dbService.ExecuteScalar("SELECT version()");
        return $"ClickHouse version: {version}";
    }
}
