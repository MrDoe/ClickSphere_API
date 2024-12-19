using ClickSphere_API.Models;
namespace ClickSphere_API.Services;

/// <summary>
/// Represents the interface for interacting with API views
/// </summary>
public interface IApiViewService
{
    /// <summary>
    /// Creates a new view
    /// </summary>
    /// <param name="view">The view to create</param>
    /// <returns>Result object</returns>
    Task<IResult> CreateView(View view);

    /// <summary>
    /// Creates a new materialized view
    /// </summary>
    /// <param name="view">The materialized view to create</param>
    /// <returns>Result object</returns>
    Task<IResult> CreateMaterializedView(MaterializedView view);

    /// <summary>
    /// Deletes a view
    /// </summary>
    /// <param name="database">The database name</param>
    /// <param name="view">The view name</param>
    /// <returns>Result object</returns>
    Task<IResult> DeleteView(string database, string view);

    /// <summary>
    /// Gets all views in a database
    /// </summary>
    /// <param name="database">The database name</param>
    /// <returns>List of View objects</returns>
    Task<List<View>> GetAllViews(string database);

    /// <summary>
    /// Gets the configuration of a specific view
    /// </summary>
    /// <param name="database">The database name</param>
    /// <param name="viewId">The view ID</param>
    /// <returns>Specified view object</returns>
    Task<View> GetViewConfig(string database, string viewId);

    /// <summary>
    /// Gets the definition of a specific view
    /// </summary>
    /// <param name="database">The database name</param>
    /// <param name="viewId">The view ID</param>
    /// <returns>View defition as string</returns> 
    Task<string> GetViewDefinition(string database, string viewId);

    /// <summary>
    /// Updates a view
    /// </summary>
    /// <param name="view">The view to update</param>
    /// <returns>Result object</returns>
    Task<IResult> UpdateView(View view);

    /// <summary>
    /// Gets the columns of a specific view
    /// </summary>
    /// <param name="database">The database name</param>
    /// <param name="viewId">The view ID</param>
    /// <param name="forceUpdate">Whether to force an update of the columns</param>
    /// <returns>List of ViewColumn objects</returns>
    Task<IList<ViewColumns>> GetViewColumns(string database, string viewId, bool forceUpdate);

    /// <summary>
    /// Update a view column configuration
    /// </summary>
    /// <param name="column">The column to update</param>
    /// <returns>The result of the column update</returns>
    Task<IResult> UpdateViewColumn(ViewColumns column);

    /// <summary>
    /// Gets the distinct values of a column
    /// </summary>
    /// <param name="database">The database name</param>
    /// <param name="viewId">The view ID</param>
    /// <param name="columnName">The column name</param>
    /// <returns>List of values as strings</returns>
    Task<IList<string>> GetDistinctValues(string database, string viewId, string columnName);

    /// <summary>
    /// Import view from ODBC into ClickHouse
    /// </summary>
    /// <param name="view">The view name</param>
    /// <param name="dropExisting">Whether to drop the existing view</param>
    /// <returns>string</returns>
    Task<string> ImportViewFromODBC(string view, bool dropExisting);

    /// <summary>
    /// Get columns from ODBC view
    /// </summary>
    /// <param name="view">The view name</param>
    /// <param name="columnName">The column name</param>
    /// <returns>List of columns as strings</returns>
    Task<IList<string>> GetColumnsFromODBC(string view, string columnName);

    /// <summary>
    /// Get views from ODBC
    /// </summary>
    /// <returns>List of views available for import as strings</returns>
    Task<IList<string>> GetViewsFromODBC();
}