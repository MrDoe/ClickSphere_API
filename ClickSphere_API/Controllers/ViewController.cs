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
        public async Task<IResult> CreateView(View view)
        {
            string query = $"CREATE VIEW {view.Database}.{view.Id} AS {view.Query};";
            int result = await _dbService.ExecuteNonQuery(query);

            // insert view into CV_Views table
            if (result == 0)
            {
                string insertQuery = $"INSERT INTO ClickSphere.Views (Id, Name, Description, Type) VALUES ('{view.Id}','{view.Name}','{view.Description}','V');";
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
        * Create materialized view in the specified database
        * @param database The database where the materialized view should be created
        * @param viewName The name of the materialized view to create
        * @param query The query that defines the materialized view
        * @return The result of the materialized view creation
        */
        [Authorize]
        [HttpPost]
        [Route("/createMaterializedView")]
        public async Task<IResult> CreateMaterializedView(MaterializedView view)
        {
            // create materialized view
            string query = $"CREATE MATERIALIZED VIEW {view.Database}.{view.Id} " + 
                           $"ENGINE = {view.Engine} ";
            
            if(view.PartitionBy != null)
                query += $"PARTITION BY {view.PartitionBy} ";
            
            if(view.OrderBy != null)
                query += $"ORDER BY {view.OrderBy} ";

            if(view.Populate == true)
                query += "POPULATE ";
            
            query += $"AS {view.Query};";
            
            int result = await _dbService.ExecuteNonQuery(query);

            // insert view into ClickSphere.Views metadata table
            if (result == 0)
            {
                string insertQuery = $"INSERT INTO ClickSphere.Views (Id, Name, Description, Type) VALUES ('{view.Id}','{view.Name}','{view.Description}','M');";
                int insertResult = await _dbService.ExecuteNonQuery(insertQuery);
                if (insertResult < 0)
                    return Results.BadRequest("Could not insert view into ClickSphere.Views table");
                else
                    return Results.Ok();
            }
            else
                return Results.BadRequest("Could not create materialized view");
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
        [Route("/getAllViews")]
        public async Task<IEnumerable<Dictionary<string, object>>> GetAllViews(string database)
        {
            return await _dbService.ExecuteQueryDictionary("SELECT c.*, s.database as Database, s.as_select as Query " +
                                                           "FROM system.tables s JOIN ClickSphere.Views c ON c.Id = s.name " + 
                                                          $"WHERE s.database = '{database}' and s.engine = 'View'");
        }

        /**
        * Get configuration of specific view from database
        * @param database The database to get the view from
        * @param viewId The viewId to get the configuration from
        * @return The configuration of the view
        */
        [Authorize]
        [HttpGet]
        [Route("/getViewConfig")]
        public async Task<View> GetViewConfig(string database, string viewId)
        {
            return await _dbService.ExecuteQueryObject<View>("SELECT c.Id, c.Name, c.Description, s.database as Database, s.as_select as Query " + 
                                                             "FROM system.tables s JOIN ClickSphere.Views c ON c.Id = s.name " + 
                                                             $"WHERE s.database = '{database}' and s.engine = 'View' and c.Id = '{viewId}'");
        }

        /**
        * Update a view in the specified database
        * @param database The database where the view should be updated
        * @param view The view to update
        * @return The result of the view update
        */
        [Authorize]
        [HttpPost]
        [Route("/updateView")]
        public async Task<IResult> UpdateView(View view)
        {          
            string query = $"CREATE OR REPLACE VIEW {view.Database}.{view.Id} AS {view.Query};";
            int result = await _dbService.ExecuteNonQuery(query);

            // update view in CV_Views table
            if (result == 0)
            {
                string updateQuery = $"UPDATE ClickSphere.Views SET Name = '{view.Name}', Description = '{view.Description}' WHERE Id = '{view.Id}';";
                int updateResult = await _dbService.ExecuteNonQuery(updateQuery);
                if (updateResult < 0)
                    return Results.BadRequest("Could not update view in ClickSphere.Views table");
                else
                    return Results.Ok();
            }
            else
                return Results.BadRequest("Could not update view");
        }

        /**
        * Get view columns and type for QBE search
        * @param database The database to get the view from
        * @param viewId The viewId to get the columns from
        * @return The columns of the view
        */
        [Authorize]
        [HttpGet]
        [Route("/getViewColumns")]
        public async Task<IEnumerable<Dictionary<string, object>>> GetViewColumns(string database, string viewId)
        {
            return await _dbService.ExecuteQueryDictionary($"SELECT name, type FROM system.columns WHERE table = '{database}.{viewId}'");
        }
    }
}