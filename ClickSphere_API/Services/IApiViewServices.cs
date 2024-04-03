using ClickSphere_API.Models;
namespace ClickSphere_API.Services;
public interface IApiViewServices
{
    Task<IResult> CreateView(View view);
    Task<IResult> CreateMaterializedView(MaterializedView view);
    Task<IResult> DeleteView(string database, string view);
    Task<List<View>> GetAllViews(string database);
    Task<View> GetViewConfig(string database, string viewId);
    Task<IResult> UpdateView(View view);
    Task<IList<Dictionary<string, object>>> GetViewColumns(string database, string viewId);
}