using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClickSphere_API.Services;

namespace ClickSphere_API.Controllers
{
    /**
     * The base class for ClickSphere API controllers.
     */
    [ApiController]
    public class ClickHouseController(IDbService dbService) : ControllerBase
    {
        private readonly IDbService _dbService = dbService;

        /**
        * Get the version of the ClickHouse server
        * @return The version of the server
        */
        [Authorize]
        [HttpGet]
        [Route("/version")]
        public async Task<string> GetVersion()
        {
            var version = await _dbService.ExecuteScalar("SELECT version()");
            return $"ClickHouse version: {version}";
        }
    }
}