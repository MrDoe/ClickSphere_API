using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClickSphere_API.Services;
using ClickSphere_API.Models;
namespace ClickSphere_API.Controllers;
/// <summary>
/// Controller class for managing views in the ClickSphere database system
/// </summary>
[ApiController]
public class ViewController(IApiViewServices viewServices) : ControllerBase
{
    private readonly IApiViewServices ViewServices = viewServices;

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
        return await ViewServices.CreateView(view);
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
    /// <returns>The columns of the view</returns>
    [Authorize]
    [HttpGet]
    [Route("/getViewColumns")]
    public async Task<IList<ViewColumns>> GetViewColumns(string database, string viewId)
    {
        return await ViewServices.GetViewColumns(database, viewId);
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
}
