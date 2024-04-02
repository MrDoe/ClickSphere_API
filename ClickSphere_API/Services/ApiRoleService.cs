using ClickSphere_API.Tools;
using ClickSphere_API.Models;
using ClickSphere_API.Models.Requests;
using System.Text.RegularExpressions;

namespace ClickSphere_API.Services
{
    /// <summary>
    /// This class provides methods for managing roles in the ClickHouse database system.
    /// </summary>
    public partial class ApiRoleService(IDbService dbService) : IApiRoleService
    {
        private readonly IDbService dbService = dbService;

        /// <summary>
        /// Creates a new role in the ClickHouse database system.
        /// </summary>
        /// <param name="roleName">The name of the role to create.</param>
        /// <returns>The created role.</returns>
        public async Task<Result> CreateRole(string roleName)
        {
            string query = $"CREATE ROLE `{roleName}`";
            try
            {
                await dbService.ExecuteNonQuery(query);
            }
            catch (Exception)
            {
                return Result.BadRequest("Could not create role");
            }
            return Result.Ok();
        }

        /// <summary>
        /// Deletes a role from the ClickHouse database system.
        /// </summary>
        /// <param name="roleName">The name of the role to delete.</param>
        /// <returns>True if the role was deleted, otherwise false.</returns>
        public async Task<Result> DeleteRole(string roleName)
        {
            string query = $"DROP ROLE `{roleName}`";
            try
            {
                await dbService.ExecuteNonQuery(query);
            }
            catch (Exception)
            {
                return Result.BadRequest("Could not delete role");
            }
            return Result.Ok();
        }

        /// <summary>
        /// Retrieves all roles from the database.
        /// </summary>
        /// <returns>A list of strings representing all roles.</returns>
        public async Task<List<UserRole>> GetRoles()
        {
            string query = $"SELECT id, name FROM system.roles";
            var result = await dbService.ExecuteQueryDictionary(query);

            List<UserRole> roles = result.Select(row => new UserRole
            {
                Id = Guid.Parse(row["id"].ToString()!),
                Name = row["name"].ToString()!
            }).ToList();

            return roles;
        }

        /// <summary>
        /// Get role by its id.
        /// </summary>
        /// <param name="roleId">The id of the role.</param>
        /// <returns>The role with the given id.</returns>
        public async Task<UserRole?> GetRoleById(Guid roleId)
        {
            string query = $"SELECT name FROM system.roles WHERE id = '{roleId}'";
            var result = await dbService.ExecuteScalar(query);
            if (result is DBNull)
                return null;

            return new UserRole
            {
                Id = roleId,
                Name = result?.ToString()!
            };
        }

        /// <summary>
        /// Retrieves the roles associated with a user.
        /// </summary>
        /// <param name="userName">The user name of the user.</param>
        /// <returns>A UserRole object representing the roles associated with the user.</returns>
        public async Task<UserRole?> GetRoleFromUser(string userName)
        {
            string query = $"SELECT granted_role_name, granted_role_id from system.role_grants where user_name = '{userName}'";
            var result = await dbService.ExecuteQueryDictionary(query);
            var role = result.Select(row => new UserRole
            {
                Id = Guid.Parse(row["granted_role_id"].ToString()!),
                Name = row["granted_role_name"].ToString()!
            }).FirstOrDefault();

            return role;
        }

        /// <summary>
        /// Assigns a role to a user.
        /// </summary>
        /// <param name="userName">The user name of the user.</param>
        /// <param name="roleName">The name of the role to assign.</param>
        /// <returns>True if the role was assigned, otherwise false.</returns>
        public async Task<Result> AssignRoleToUser(string userName, string roleName)
        {
            string query = $"GRANT ROLE `{roleName}` TO USER `{userName}`";
            try
            {
                await dbService.ExecuteNonQuery(query);
            }
            catch (Exception)
            {
                return Result.BadRequest("Could not assign role");
            }
            return Result.Ok();
        }

        /// <summary>
        /// Removes a role from a user.
        /// </summary>
        /// <param name="userName">The user name of the user.</param>
        /// <param name="roleName">The name of the role to remove.</param>
        /// <returns>True if the role was removed, otherwise false.</returns>
        public async Task<Result> RemoveRoleFromUser(string userName, string roleName)
        {
            string query = $"REVOKE ROLE `{roleName}` FROM USER `{userName}`";
            try
            {
                await dbService.ExecuteNonQuery(query);
            }
            catch (Exception)
            {
                return Result.BadRequest("Could not remove role");
            }
            return Result.Ok();
        }

        /// <summary>
        /// Grant select permission of a view to a role.
        /// <summary>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="viewId">The id of the view.</param>
        /// <param name="database">The name of the view's database.</param>
        /// <returns>True if the view was assigned, otherwise false.</returns>
        public async Task<Result> AddViewToRole(string roleName, string viewId, string database)
        {
            string query = $"GRANT SELECT ON `{database}`.`{viewId}` TO ROLE `{roleName}`";
            try
            {
                await dbService.ExecuteNonQuery(query);
            }
            catch (Exception)
            {
                return Result.BadRequest("Could not assign view to role");
            }
            return Result.Ok();
        }

        /// <summary>
        /// Get all granted views for role.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        /// <returns>The views for the role.</returns>
        public async Task<List<GetViewsForRoleRequest>> GetViewsForRole(string roleName)
        {
            string query = $"SHOW GRANTS FOR `{roleName}`";
            List<string> result = await dbService.ExecuteQuery(query);

            // parse with Regex to get Database and View
            List<GetViewsForRoleRequest> views = [];
            foreach (var row in result)
            {
                Match match = RegExParseViews().Match(row);
                if (match.Success)
                {
                    views.Add(new GetViewsForRoleRequest
                    {
                        Database = match.Groups["database"].Value,
                        ViewID = match.Groups["view"].Value
                    });
                }
            }
            return views;
        }

        [GeneratedRegex(@"^GRANT SELECT ON (?<database>\w+)\.(?<view>\w+).*$")]
        private static partial Regex RegExParseViews();
    }
}
