using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClickViews_API.Services;
using ClickViews_API.Models;

namespace ClickViews_API.Controllers
{
    /**
     * The base class for ClickViews API controllers.
     */
    [ApiController]
    public class ViewController(IDbService dbService) : ControllerBase
    {
        private readonly IDbService _dbService = dbService;
       
        /**
        * Create a new view in the specified database
        * @param database The database where the view should be created
        * @param viewName The name of the view to create
        * @param query The query that defines the view
        * @return The result of the view creation
        */
        [Authorize]
        [HttpPost]
        [Route("/createView")]
        public async Task<IResult> CreateView(CreateViewRequest request)
        {
            string query = $"CREATE VIEW {request.Database}.{request.View} AS {request.Query};";
            int result = await _dbService.ExecuteNonQuery(query);
            
            if (result == 0)
                return Results.Ok();
            else
                return Results.BadRequest("Could not create view");
        }

        /**
        * Delete a view from a database
        * @param database The database where the view is located
        * @param view The view to delete
        * @return The result of the view deletion
        */
        [Authorize]
        [HttpDelete]
        [Route("/deleteView")]
        public async Task<IResult> DeleteView(string database, string view)
        {
            int result = await _dbService.ExecuteNonQuery($"DROP VIEW {database}.{view}");
            if (result == 0)
                return Results.Ok();
            else
                return Results.BadRequest("Could not drop view");
        }
    }
}