using System.Text;
using ClickSphere_API.Models;
namespace ClickSphere_API.Services;

/// <summary>
/// Service class for handling view operations.
/// </summary>
/// <param name="dbService"></param>
public class ApiViewServices(IDbService dbService) : IApiViewService
{
    private readonly IDbService _dbService = dbService;

    /// <summary>
    /// Create a new view in the specified database
    /// </summary>
    /// <param name="view">The view to create</param>
    /// <returns>The result of the view creation</returns>
    public async Task<IResult> CreateView(View view)
    {
        string query = $"CREATE VIEW {view.Database}.{view.Id} AS {view.Query};";
        int result;
        try
        {
            result = await _dbService.ExecuteNonQuery(query);
        }
        catch (Exception e)
        {
            return Results.BadRequest(e.Message);
        }

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

    /// <summary>
    /// Create materialized view in the specified database
    /// </summary>
    /// <param name="view">The materialized view to create</param>
    /// <returns>The result of the materialized view creation</returns>
    public async Task<IResult> CreateMaterializedView(MaterializedView view)
    {
        // create materialized view
        string query = $"CREATE MATERIALIZED VIEW {view.Database}.{view.Id} " +
                       $"ENGINE = {view.Engine} ";

        if (view.PartitionBy != null)
            query += $"PARTITION BY {view.PartitionBy} ";

        if (view.OrderBy != null)
            query += $"ORDER BY {view.OrderBy} ";

        if (view.Populate == true)
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

    /// <summary>
    /// Delete a view from a database
    /// </summary>
    /// <param name="database">The database where the view is located</param>
    /// <param name="view">The view to delete</param>
    /// <returns>The result of the view deletion</returns>
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

    /// <summary>
    /// Get the views of a database which are registered in ClickSphere
    /// </summary>
    /// <param name="database">The database to get the views from</param>
    /// <returns>A list of views of the database</returns>
    public async Task<List<View>> GetAllViews(string database)
    {
        return await _dbService.ExecuteQueryList<View>("SELECT c.*, s.database as Database, s.as_select as Query " +
                                                       "FROM system.tables s JOIN ClickSphere.Views c ON c.Id = s.name " +
                                                       $"WHERE s.database = '{database}' and s.engine = 'View'");
    }

    /// <summary>
    /// Get configuration of specific view from database
    /// </summary>
    /// <param name="database">The database to get the view from</param>
    /// <param name="viewId">The viewId to get the configuration from</param>
    /// <returns>The configuration of the view</returns>
    public async Task<View> GetViewConfig(string database, string viewId)
    {
        return await _dbService.ExecuteQueryObject<View>("SELECT c.Id, c.Name, c.Description, s.database as Database, s.as_select as Query, c.Questions " +
                                                         "FROM system.tables s JOIN ClickSphere.Views c ON c.Id = s.name " +
                                                         $"WHERE s.database = '{database}' and s.engine = 'View' and c.Id = '{viewId}'");
    }

    /// <summary>
    /// Update a view in the specified database
    /// </summary>
    /// <param name="view">The view to update</param>
    /// <returns>The result of the view update</returns>
    public async Task<IResult> UpdateView(View view)
    {
        string query = $"CREATE OR REPLACE VIEW {view.Database}.{view.Id} AS {view.Query};";
        int result = await _dbService.ExecuteNonQuery(query);

        // update view in CV_Views table
        if (result == 0)
        {
            string updateQuery = $"ALTER TABLE ClickSphere.Views " + 
                                 $"UPDATE Name = '{view.Name}', " + 
                                 $"Description = '{view.Description}', " + 
                                 $"Questions = '{view.Questions}' " + 
                                 $"WHERE Id = '{view.Id}';";
            int updateResult = await _dbService.ExecuteNonQuery(updateQuery);
            if (updateResult < 0)
                return Results.BadRequest("Could not update view in ClickSphere.Views table");
            else
                return Results.Ok();
        }
        else
            return Results.BadRequest("Could not update view");
    }

    /// <summary>
    /// Get view columns and type for QBE search
    /// </summary>
    /// <param name="database">The database to get the view from</param>
    /// <param name="viewId">The viewId to get the columns from</param>
    /// <param name="forceUpdate">Force update of the view columns</param>
    /// <returns>The columns of the view</returns>
    public async Task<IList<ViewColumns>> GetViewColumns(string database, string viewId, bool forceUpdate = false)
    {
        List<ViewColumns> columns;

        if (forceUpdate)
        {
            // delete data from ViewColumns table
            await _dbService.ExecuteNonQuery($"DELETE FROM ClickSphere.ViewColumns WHERE Database = '{database}' AND ViewId = '{viewId}'");

            // update data from ViewColumns table
            bool success = await UpdateViewColumns(database, viewId);

            if (!success)
                return new List<ViewColumns>();

            // load data from ViewColumns table
            return await LoadViewColumns(database, viewId);
        }
        else
        {
            // try to get data from ViewColumns table
            columns = await LoadViewColumns(database, viewId);

            if (columns.Count == 0)
            {
                bool success = await UpdateViewColumns(database, viewId);

                if (!success)
                    return new List<ViewColumns>();

                // load data from ViewColumns table again
                columns = await LoadViewColumns(database, viewId);
            }
            return columns;
        }
    }

    private async Task<List<ViewColumns>> LoadViewColumns(string database, string viewId)
    {
        return await _dbService.ExecuteQueryList<ViewColumns>($"SELECT * FROM ClickSphere.ViewColumns WHERE Database = '{database}' AND ViewId = '{viewId}' order by Sorter");
    }

    private async Task<bool> UpdateViewColumns(string database, string viewId)
    {
        // insert data into viewColumns table
        var viewCols = await _dbService.ExecuteQueryDictionary($"SELECT name as `Column Name`, type as `Data Type` FROM system.columns WHERE table = '{viewId}' and database = '{database}'");

        if (viewCols == null)
            return false;

        int sorter = 0;
        foreach (var col in viewCols)
        {
            string? controlType = col["Data Type"] switch
            {
                "String" => "TextBox",
                "UInt8" or "UInt16" or "UInt32" or "UInt64" or "Int8" or "Int16" or "Int32" or "Int64" => "Numeric",
                "Float32" or "Float64" => "Numeric",
                "DateTime" or "DateTime64(3)" => "DateTime",
                _ => "TextBox",
            };

            // escape single quotes from data type
            string dataType = col["Data Type"].ToString()?.Replace("'", "''") ?? "";
            
            string insertQuery = "INSERT INTO ClickSphere.ViewColumns (Id, Database, ViewId, ColumnName, DataType, ControlType, Sorter) " +
                                 $"VALUES ('{Guid.NewGuid()}','{database}','{viewId}','{col["Column Name"]}','{dataType}','{controlType}',{sorter});";

            await _dbService.ExecuteNonQuery(insertQuery);
            ++sorter;
        }
        return true;
    }

    /// <summary>
    /// Get columns, data types and descriptions of a view
    /// </summary>
    /// <param name="database">The database to get the view from</param>
    /// <param name="viewId">The viewId to get the columns from</param>
    /// <returns>View definition as string</returns>
    public async Task<string> GetViewDefinition(string database, string viewId)
    {
        string query = $"SELECT ColumnName, DataType, Description " +
                       $"FROM ClickSphere.ViewColumns " +
                       $"WHERE Database = '{database}' AND ViewId = '{viewId}' order by Sorter";

        var output = await _dbService.ExecuteQueryDictionary(query);
        if (output == null)
            return "";

        // create view definition as CSV
        StringBuilder csv = new();
        csv.AppendLine("Column Name,Data Type,Description");
        foreach (var row in output)
        {
            csv.AppendLine($"{row["ColumnName"]},{row["DataType"]},{row["Description"]}");
        }

        return csv.ToString();
    }

    /// <summary>
    /// Update view column configuration
    /// </summary>
    /// <param name="column">The column to update</param>
    /// <returns>The result of the column update</returns>
    public async Task<IResult> UpdateViewColumn(ViewColumns column)
    {
        if(column == null)
            return Results.BadRequest("Column is null");
        
        // escape single quotes
        string placeholder = column.Placeholder.Replace("'", "''");
        string description = column.Description.Replace("'", "''");

        string query = $"ALTER TABLE ClickSphere.ViewColumns " +
                       $"UPDATE ControlType = '{column.ControlType}', " +
                       $"Placeholder = '{placeholder}', " +
                       $"Sorter = {column.Sorter}, " +
                       $"Description = '{description}' " +
                       $"WHERE Id = '{column.Id}';";

        int result = await _dbService.ExecuteNonQuery(query);
        if (result == 0)
            return Results.Ok();
        else
            return Results.BadRequest("Could not update column");
    }

    /// <summary>
    /// Get the distinct values of a column
    /// </summary>
    /// <param name="database">The database to get the data from</param>
    /// <param name="viewId">The viewId to get the data from</param>
    /// <param name="column">The column to get the distinct values from</param>
    /// <returns>The distinct values of the column</returns>
    public async Task<IList<string>> GetDistinctValues(string database, string viewId, string column)
    {
        return await _dbService.ExecuteQuery($"SELECT DISTINCT {column} FROM {database}.{viewId} LIMIT 50") ?? [];
    }
}