using Oracle.ManagedDataAccess.Client;
using RegistrationPortal.Server.DTOs.Auth;
using RegistrationPortal.Server.DTOs.Pagination;
using RegistrationPortal.Server.Data;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace RegistrationPortal.Server.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly IConfiguration _configuration;
        private readonly RegistrationPortalDbContext _context;

        public RoleRepository(RegistrationPortalDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<PaginatedResultDto<RoleListDto>> GetRolesPaginatedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? sortBy = null,
            bool sortDescending = false,
            string? status = null)
        {
            using var connection = CreateNewConnection();
            await connection.OpenAsync();

            var baseSql = @"SELECT r.* FROM SSDBONLINE.ROLES r";

            var whereClauses = new List<string>();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                whereClauses.Add("(UPPER(r.NAME) LIKE UPPER(:searchTerm) OR UPPER(r.DESCRIPTION) LIKE UPPER(:searchTerm))");
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                var isActive = status.Trim().ToLower() == "active" ? 1 : 0;
                whereClauses.Add("r.IS_ACTIVE = :isActive");
            }
            if (whereClauses.Any())
            {
                baseSql += " WHERE " + string.Join(" AND ", whereClauses);
            }

            var orderBy = "r.NAME";
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.Trim().ToLower())
                {
                    case "name": orderBy = "r.NAME"; break;
                    case "createdat": orderBy = "r.CREATED_AT"; break;
                    default: orderBy = "r.NAME"; break;
                }
            }
            baseSql += $" ORDER BY {orderBy} {(sortDescending ? "DESC" : "ASC")}";

            var countSql = "SELECT COUNT(*) FROM SSDBONLINE.ROLES r" + (whereClauses.Any() ? (" WHERE " + string.Join(" AND ", whereClauses)) : "");

            var offset = (pageNumber - 1) * pageSize;
            var maxRow = offset + pageSize;
            var pagedSql = $@"
                SELECT * FROM (
                    SELECT x.*, ROWNUM rn FROM (
                        {baseSql}
                    ) x WHERE ROWNUM <= {maxRow}
                ) WHERE rn > {offset}";

            int totalCount = 0;
            using (var countCmd = connection.CreateCommand())
            {
                countCmd.CommandText = countSql;
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    countCmd.Parameters.Add(new OracleParameter("searchTerm", OracleDbType.Varchar2) { Value = $"%{searchTerm}%", Direction = ParameterDirection.Input });
                }
                if (!string.IsNullOrWhiteSpace(status))
                {
                    var isActive = status.Trim().ToLower() == "active" ? 1 : 0;
                    countCmd.Parameters.Add(new OracleParameter("isActive", OracleDbType.Int32) { Value = isActive, Direction = ParameterDirection.Input });
                }
                var result = await countCmd.ExecuteScalarAsync();
                totalCount = result != null ? Convert.ToInt32(result) : 0;
            }

            var roles = new List<RoleListDto>();
            var roleIds = new List<int>();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = pagedSql;
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    cmd.Parameters.Add(new OracleParameter("searchTerm", OracleDbType.Varchar2) { Value = $"%{searchTerm}%", Direction = ParameterDirection.Input });
                }
                if (!string.IsNullOrWhiteSpace(status))
                {
                    var isActive = status.Trim().ToLower() == "active" ? 1 : 0;
                    cmd.Parameters.Add(new OracleParameter("isActive", OracleDbType.Int32) { Value = isActive, Direction = ParameterDirection.Input });
                }

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var id = Convert.ToInt32(reader["ROLE_ID"]);
                    roleIds.Add(id);
                    roles.Add(new RoleListDto
                    {
                        Id = id,
                        Name = reader["NAME"]?.ToString() ?? string.Empty,
                        Description = reader["DESCRIPTION"]?.ToString() ?? string.Empty,
                        IsActive = Convert.ToInt32(reader["IS_ACTIVE"]) == 1,
                        CreatedAt = Convert.ToDateTime(reader["CREATED_AT"]),
                        Permissions = new List<string>()
                    });
                }
            }

            if (roleIds.Any())
            {
                var inParams = new List<string>();
                using var permCmd = connection.CreateCommand();
                for (int i = 0; i < roleIds.Count; i++)
                {
                    var name = $"rid{i}";
                    inParams.Add($":{name}");
                    permCmd.Parameters.Add(new OracleParameter(name, OracleDbType.Int32) { Value = roleIds[i], Direction = ParameterDirection.Input });
                }
                permCmd.CommandText = $@"SELECT rp.ROLE_ID, p.NAME
                                          FROM SSDBONLINE.ROLE_PERMISSIONS rp
                                          JOIN SSDBONLINE.PERMISSIONS p ON p.PERMISSION_ID = rp.PERMISSION_ID
                                          WHERE rp.ROLE_ID IN ({string.Join(", ", inParams)})";

                var permsLookup = new Dictionary<int, List<string>>();
                using var rr = await permCmd.ExecuteReaderAsync();
                while (await rr.ReadAsync())
                {
                    var rid = Convert.ToInt32(rr["ROLE_ID"]);
                    var pname = rr["NAME"]?.ToString() ?? string.Empty;
                    if (!permsLookup.TryGetValue(rid, out var list))
                    {
                        list = new List<string>();
                        permsLookup[rid] = list;
                    }
                    list.Add(pname);
                }
                foreach (var r in roles)
                {
                    if (permsLookup.TryGetValue(r.Id, out var list))
                    {
                        r.Permissions = list;
                    }
                }
            }

            return new PaginatedResultDto<RoleListDto>
            {
                Data = roles,
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
