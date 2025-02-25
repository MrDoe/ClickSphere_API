using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClickSphere_API.Services;
using ClickSphere_API.Models;
using System.Text;
namespace ClickSphere_API.Controllers;
/// <summary>
/// Controller class for managing views in the ClickSphere database system
/// </summary>
[ApiController]
public class ViewController(IApiViewService viewServices) : ControllerBase
{
    private readonly IApiViewService ViewServices = viewServices;

    /// <summary>
    /// Create a new view in the specified database
    /// </summary>
    /// <param name="view">The view to create</param>
    /// <returns>The result of the view creation</returns>
    [Authorize]
    [HttpPost]
    [Route("/createView")]
    public async Task<IResult> CreateView(View view)
    {
        HttpResponseMessage result = await ViewServices.CreateView(view);
        if (result.IsSuccessStatusCode)
        {
            return Results.Ok("View created successfully");
        }
        else
        {
            return Results.BadRequest(result.ReasonPhrase);
        }
    }

    /// <summary>
    /// Create materialized view in the specified database
    /// </summary>
    /// <param name="view">The materialized view to create</param>
    /// <returns>The result of the materialized view creation</returns>
    [Authorize]
    [HttpPost]
    [Route("/createMaterializedView")]
    public async Task<IResult> CreateMaterializedView(MaterializedView view)
    {
        return await ViewServices.CreateMaterializedView(view);
    }

    /// <summary>
    /// Delete a view from a database
    /// </summary>
    /// <param name="database">The database where the view is located</param>
    /// <param name="viewId">The view to delete</param>
    /// <returns>The result of the view deletion</returns>
    [Authorize]
    [HttpDelete]
    [Route("/deleteView")]
    public async Task<IResult> DeleteView(string database, string viewId)
    {
       return await ViewServices.DeleteView(database, viewId);
    }

    /// <summary>
    /// Get all views of a database which are registered in ClickSphere
    /// </summary>
    /// <param name="database">The database to get the views from</param>
    /// <returns>The views of the database</returns>
    [Authorize]
    [HttpGet]
    [Route("/getAllViews")]
    public async Task<IList<View>> GetAllViews(string database)
    {
        return await ViewServices.GetAllViews(database);
    }

    /// <summary>
    /// Get configuration of a view in a database
    /// </summary>
    /// <param name="database">The database to get the view from</param>
    /// <param name="viewId">The viewId to get the configuration from</param>
    /// <returns>The configuration of the view</returns>
    [Authorize]
    [HttpGet]
    [Route("/getViewConfig")]
    public async Task<View> GetViewConfig(string database, string viewId)
    {
        return await ViewServices.GetViewConfig(database, viewId);
    }

    /// <summary>
    /// Update view configuration
    /// </summary>
    /// <param name="view">The view to update</param>
    /// <returns>The result of the view update</returns>
    [Authorize]
    [HttpPost]
    [Route("/updateView")]
    public async Task<IResult> UpdateView(View view)
    {
        return await ViewServices.UpdateView(view);
    }

    /// <summary>
    /// Get view columns and type for QBE search
    /// </summary>
    /// <param name="database">The database to get the view from</param>
    /// <param name="viewId">The viewId to get the columns from</param>
    /// <param name="forceUpdate">Force update of the columns</param>
    /// <returns>The columns of the view</returns>
    [Authorize]
    [HttpGet]
    [Route("/getViewColumns")]
    public async Task<IList<ViewColumns>> GetViewColumns(string database, string viewId, bool forceUpdate)
    {
        return await ViewServices.GetViewColumns(database, viewId, forceUpdate);
    }

    /// <summary>
    /// Get view definition for AI search
    /// </summary>
    /// <param name="database">The database to get the view from</param>
    /// <param name="viewId">The viewId to get the definition from</param>
    /// <returns>The view definition</returns>
    [Authorize]
    [HttpGet]
    [Route("/getViewDefinition")]
    public async Task<string> GetViewDefinition(string database, string viewId)
    {
        return await ViewServices.GetViewDefinition(database, viewId);
    }
    
    
    /// <summary>
    /// Update configuration of a view column 
    /// </summary>
    /// <param name="columns">The columns to update</param>
    /// <returns>The result of the column update</returns>
    [Authorize]
    [HttpPost]
    [Route("/updateViewColumn")]
    public async Task<IResult> UpdateViewColumn(ViewColumns columns)
    {
        return await ViewServices.UpdateViewColumn(columns);
    }

    /// <summary>
    /// Get the distinct values of a column
    /// </summary>
    /// <param name="database">The database to get the data from</param>
    /// <param name="viewId">The viewId to get the data from</param>
    /// <param name="columnName">The column to get the distinct values from</param>
    /// <returns>The distinct values of the column</returns>
    [Authorize]
    [HttpGet]
    [Route("/getDistinctValues")]
    public async Task<IList<string>> GetDistinctValues(string database, string viewId, string columnName)
    {
        if(database == null || viewId == null || columnName == null)
        {
            throw new ArgumentNullException("database, viewId or columnName is null");
        }
        
        return await ViewServices.GetDistinctValues(database, viewId, columnName);
    }

    /// <summary>
    /// Export view to Excel file
    /// </summary>
    /// <param name="b64query">The view query to export</param>
    /// <param name="viewId">The viewId to get the data from</param>
    /// <param name="fileName">The name of the file</param>
    /// <returns>The Excel file</returns>
    //[Authorize]
    [HttpGet]
    [Route("/exportToExcel")]
    public async Task<FileResult?> ExportToExcel(string b64query, string viewId, string fileName)
    {
        string query = Encoding.UTF8.GetString(Convert.FromBase64String(b64query));
        return await ViewServices.ExportToExcel(query, viewId, fileName);
    }
    
}
