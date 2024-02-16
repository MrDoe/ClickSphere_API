using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClickSphere_API.Services;
using ClickSphere_API.Models;

namespace ClickSphere_API.Controllers
{
    /**
     * The base class for ClickSphere API controllers.
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
        public async Task<IResult> CreateView(CreateViewRequest view)
        {
            string query = $"CREATE VIEW {view.Database}.{view.Id} AS {view.Query};";
            int result = await _dbService.ExecuteNonQuery(query);

            // insert view into CV_Views table
            if (result == 0)
            {
                string insertQuery = $"INSERT INTO ClickSphere.Views (Id, Name, Description) VALUES ('{view.Id}', '{view.Name}', '{view.Description}');";
                int insertResult = await _dbService.ExecuteNonQuery(insertQuery);
                if (insertResult < 0)
                    return Results.BadRequest("Could not insert view into ClickSphere.Views table");
                else
                    return Results.Ok();
            }
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
            {
                result = await _dbService.ExecuteNonQuery($"DELETE FROM ClickSphere.Views WHERE Id = '{view}'");
                if (result == 0)
                    return Results.Ok();
                else
                    return Results.BadRequest("Could not delete view from ClickSphere.Views table");
            }
            else
                return Results.BadRequest("Could not drop view");
        }

        /**
        * Get the views of a database which are registered in ClickSphere
        * @param database The database to get the views from
        * @return The views of the database
        */
        [Authorize]
        [HttpGet]
        [Route("/getViewConfig")]
        public async Task<IEnumerable<Dictionary<string, object>>> GetViewConfig(string database)
        {
            return await _dbService.ExecuteQueryDictionary($"SELECT c.* FROM system.tables s JOIN ClickSphere.Views c ON c.Id = s.name WHERE s.database = '{database}' and s.engine = 'View'");
        }

    }
}