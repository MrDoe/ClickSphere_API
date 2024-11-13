using ClickSphere_API.Models;
namespace ClickSphere_API.Services;
/// <summary>
/// Represents the interface for interacting with API views.
/// </summary>
public interface IApiViewService
{
    /// <summary>
    /// Creates a new view.
    /// </summary>
    /// <param name="view">The view to create.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<IResult> CreateView(View view);

    /// <summary>
    /// Creates a new materialized view.
    /// </summary>
    /// <param name="view">The materialized view to create.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<IResult> CreateMaterializedView(MaterializedView view);

    /// <summary>
    /// Deletes a view.
    /// </summary>
    /// <param name="database">The database name.</param>
    /// <param name="view">The view name.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<IResult> DeleteView(string database, string view);

    /// <summary>
    /// Gets all views in a database.
    /// </summary>
    /// <param name="database">The database name.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<List<View>> GetAllViews(string database);

    /// <summary>
    /// Gets the configuration of a specific view.
    /// </summary>
    /// <param name="database">The database name.</param>
    /// <param name="viewId">The view ID.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<View> GetViewConfig(string database, string viewId);

    /// <summary>
    /// Gets the definition of a specific view.
    /// </summary>
    /// <param name="database">The database name.</param>
    /// <param name="viewId">The view ID.</param>
    /// <returns>A task representing the asynchronous operation.</returns> 
    Task<string> GetViewDefinition(string database, string viewId);

    /// <summary>
    /// Updates a view.
    /// </summary>
    /// <param name="view">The view to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<IResult> UpdateView(View view);

    /// <summary>
    /// Gets the columns of a specific view.
    /// </summary>
    /// <param name="database">The database name.</param>
    /// <param name="viewId">The view ID.</param>
    /// <param name="forceUpdate">Whether to force an update of the columns.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<IList<ViewColumns>> GetViewColumns(string database, string viewId, bool forceUpdate);

    /// <summary>
    /// Update a view column configuration.
    /// </summary>
    /// <param name="column">The column to update.</param>
    /// <returns>The result of the column update.</returns>
    Task<IResult> UpdateViewColumn(ViewColumns column);

    /// <summary>
    /// Gets the distinct values of a column.
    /// </summary>
    /// <param name="database">The database name.</param>
    /// <param name="viewId">The view ID.</param>
    /// <param name="columnName">The column name.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<IList<string>> GetDistinctValues(string database, string viewId, string columnName);


    /// <summary>
    /// Import view from ODBC into ClickHouse
    /// </summary>
    /// <param name="view">The view name</param>
    /// <param name="dropExisting">Whether to drop the existing view</param>
    /// <returns></returns>
    Task<string> ImportViewFromODBC(string view, bool dropExisting);
}