using Oracle.ManagedDataAccess.Client;
using RegistrationPortal.Server.DTOs.Auth;
using RegistrationPortal.Server.DTOs.Pagination;
using RegistrationPortal.Server.Data;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace RegistrationPortal.Server.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IConfiguration _configuration;
        private readonly RegistrationPortalDbContext _context; // kept for symmetry, not used directly here

        public UserRepository(RegistrationPortalDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<PaginatedResultDto<UserListDto>> GetUsersPaginatedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? sortBy = null,
            bool sortDescending = false,
            string? status = null)
        {
            using var connection = CreateNewConnection();
            await connection.OpenAsync();

            var baseSql = @"SELECT u.* FROM SSDBONLINE.USERS u";

            var where = new List<string>();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                where.Add(@"UPPER(u.USERNAME) LIKE UPPER(:searchTerm)");
                where.Add(@"UPPER(u.EMAIL) LIKE UPPER(:searchTerm)");
                where.Add(@"UPPER(u.FIRST_NAME) LIKE UPPER(:searchTerm)");
                where.Add(@"UPPER(u.LAST_NAME) LIKE UPPER(:searchTerm)");
            }

            var whereClauses = new List<string>();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                whereClauses.Add("(" + string.Join(" OR ", where) + ")");
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                // Expecting "active" or "inactive"
                var isActive = status.Trim().ToLower() == "active" ? 1 : 0;
                whereClauses.Add("u.IS_ACTIVE = :isActive");
            }

            if (whereClauses.Any())
            {
                baseSql += " WHERE " + string.Join(" AND ", whereClauses);
            }

            // Sorting
            var orderBy = "u.USERNAME";
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.Trim().ToLower())
                {
                    case "username": orderBy = "u.USERNAME"; break;
                    case "email": orderBy = "u.EMAIL"; break;
                    case "createdat": orderBy = "u.CREATED_AT"; break;
                    default: orderBy = "u.USERNAME"; break;
                }
            }
            baseSql += $" ORDER BY {orderBy} {(sortDescending ? "DESC" : "ASC")}";

            // Count total
            var countSql = "SELECT COUNT(*) FROM SSDBONLINE.USERS u" + (whereClauses.Any() ? (" WHERE " + string.Join(" AND ", whereClauses)) : "");

            // Pagination (ROWNUM)
            var offset = (pageNumber - 1) * pageSize;
            var maxRow = offset + pageSize;
            var pagedSql = $@"
                SELECT * FROM (
                    SELECT x.*, ROWNUM rn FROM (
                        {baseSql}
                    ) x WHERE ROWNUM <= {maxRow}
                ) WHERE rn > {offset}";

            // Execute count
            int totalCount = 0;
            using (var countCmd = connection.CreateCommand())
            {
                countCmd.CommandText = countSql;
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var p = new OracleParameter("searchTerm", OracleDbType.Varchar2) { Value = $"%{searchTerm}%", Direction = ParameterDirection.Input };
                    countCmd.Parameters.Add(p);
                }
                if (!string.IsNullOrWhiteSpace(status))
                {
                    var isActive = status.Trim().ToLower() == "active" ? 1 : 0;
                    countCmd.Parameters.Add(new OracleParameter("isActive", OracleDbType.Int32) { Value = isActive, Direction = ParameterDirection.Input });
                }
                var result = await countCmd.ExecuteScalarAsync();
                totalCount = result != null ? Convert.ToInt32(result) : 0;
            }

            // Execute page
            var users = new List<UserListDto>();
            var userIds = new List<int>();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = pagedSql;
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var p = new OracleParameter("searchTerm", OracleDbType.Varchar2) { Value = $"%{searchTerm}%", Direction = ParameterDirection.Input };
                    cmd.Parameters.Add(p);
                }
                if (!string.IsNullOrWhiteSpace(status))
                {
                    var isActive = status.Trim().ToLower() == "active" ? 1 : 0;
                    cmd.Parameters.Add(new OracleParameter("isActive", OracleDbType.Int32) { Value = isActive, Direction = ParameterDirection.Input });
                }

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var id = Convert.ToInt32(reader["USER_ID"]);
                    userIds.Add(id);
                    users.Add(new UserListDto
                    {
                        Id = id,
                        Username = reader["USERNAME"]?.ToString() ?? string.Empty,
                        Email = reader["EMAIL"]?.ToString() ?? string.Empty,
                        FirstName = reader["FIRST_NAME"]?.ToString() ?? string.Empty,
                        LastName = reader["LAST_NAME"]?.ToString() ?? string.Empty,
                        Branch = reader["BRANCH"]?.ToString() ?? string.Empty,
                        IsActive = Convert.ToInt32(reader["IS_ACTIVE"]) == 1,
                        CreatedAt = Convert.ToDateTime(reader["CREATED_AT"]),
                        LastLoginAt = reader["LAST_LOGIN_AT"] == DBNull.Value ? null : Convert.ToDateTime(reader["LAST_LOGIN_AT"]),
                        Roles = new List<string>()
                    });
                }
            }

            // Load roles for fetched users (batch)
            if (userIds.Any())
            {
                // Build IN clause with parameters
                var inParams = new List<string>();
                using var roleCmd = connection.CreateCommand();
                for (int i = 0; i < userIds.Count; i++)
                {
                    var name = $"uid{i}";
                    inParams.Add($":{name}");
                    roleCmd.Parameters.Add(new OracleParameter(name, OracleDbType.Int32) { Value = userIds[i], Direction = ParameterDirection.Input });
                }
                roleCmd.CommandText = $@"SELECT ur.USER_ID, r.NAME
                                          FROM SSDBONLINE.USER_ROLES ur
                                          JOIN SSDBONLINE.ROLES r ON r.ROLE_ID = ur.ROLE_ID
                                          WHERE ur.USER_ID IN ({string.Join(", ", inParams)})";

                var rolesLookup = new Dictionary<int, List<string>>();
                using var rr = await roleCmd.ExecuteReaderAsync();
                while (await rr.ReadAsync())
                {
                    var uid = Convert.ToInt32(rr["USER_ID"]);
                    var rname = rr["NAME"]?.ToString() ?? string.Empty;
                    if (!rolesLookup.TryGetValue(uid, out var list))
                    {
                        list = new List<string>();
                        rolesLookup[uid] = list;
                    }
                    list.Add(rname);
                }

                foreach (var u in users)
                {
                    if (rolesLookup.TryGetValue(u.Id, out var list))
                    {
                        u.Roles = list;
                    }
                }
            }

            return new PaginatedResultDto<UserListDto>
            {
                Data = users,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        private OracleConnection CreateNewConnection()
        {
            var connectionString = _configuration.GetConnectionString("OracleConnection");
            return new OracleConnection(connectionString);
        }
    }
}
