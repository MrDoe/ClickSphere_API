using ClickSphere_API.Models;
namespace ClickSphere_API.Services;
/// <summary>
/// Represents the interface for interacting with API views.
/// </summary>
public interface IApiViewServices
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
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<IList<Dictionary<string, object>>> GetViewColumns(string database, string viewId);
}