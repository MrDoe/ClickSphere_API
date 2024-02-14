namespace ClickViews_API.Services
{
    public interface IDbService
    {
        Task<List<string>> ExecuteQuery(string query);
        Task<List<Dictionary<string, object>>> ExecuteQueryDictionary(string query);
        Task<int> ExecuteNonQuery(string query);
        Task<object?> ExecuteScalar(string query);
        Task<bool> CheckLogin(string user, string password);
    }
}