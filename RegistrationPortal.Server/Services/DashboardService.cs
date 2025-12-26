using Microsoft.EntityFrameworkCore;
using RegistrationPortal.Server.Data;
using RegistrationPortal.Server.DTOs.Dashboard;
using RegistrationPortal.Server.Repositories;
using RegistrationPortal.Server.Entities;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace RegistrationPortal.Server.Services
{
    public interface IDashboardService
    {
        Task<DashboardStatisticsDto> GetDashboardStatisticsAsync(string? userRole = null, string? userBranch = null);
        Task<DashboardGraphDataDto> GetDailyRequestsDataAsync(int days = 30, string? userRole = null, string? userBranch = null);
    }

    public class DashboardService : IDashboardService
    {
        private readonly ICustMastRepository _custMastRepository;
        private readonly RegistrationPortalDbContext _context;
        private readonly IConfiguration _configuration;

        public DashboardService(ICustMastRepository custMastRepository, RegistrationPortalDbContext context, IConfiguration configuration)
        {
            _custMastRepository = custMastRepository;
            _context = context;
            _configuration = configuration;
        }

        public async Task<DashboardStatisticsDto> GetDashboardStatisticsAsync(string? userRole = null, string? userBranch = null)
        {
            // Apply branch filtering for non-admin users
            IQueryable<CustMast> query = _context.CustMasts;
            
            if (!string.IsNullOrWhiteSpace(userBranch) && userRole != "Admin")
            {
                query = query.Where(c => c.BranchCCode == userBranch);
            }
            
            var allCustomers = await query.ToListAsync();
            var totalCount = allCustomers.Count;

            if (totalCount == 0)
            {
                return new DashboardStatisticsDto
                {
                    NewCustomersCount = 0,
                    NewCustomersPercentage = 0,
                    UpdateCustomersCount = 0,
                    UpdateCustomersPercentage = 0,
                    TotalCustomersCount = 0,
                    PendingReviewsCount = 0,
                    PendingReviewsPercentage = 0,
                    ApprovedReviewsCount = 0,
                    ApprovedReviewsPercentage = 0,
                    RejectedReviewsCount = 0,
                    RejectedReviewsPercentage = 0
                };
            }

            var newCustomersCount = allCustomers.Count(c => c.Status == "New");
            var updateCustomersCount = allCustomers.Count(c => c.Status == "Update");
            var pendingReviewsCount = allCustomers.Count(c => c.ReviewStatus == "Pending");
            var approvedReviewsCount = allCustomers.Count(c => c.ReviewStatus == "Approved");
            var rejectedReviewsCount = allCustomers.Count(c => c.ReviewStatus == "Rejected");

            return new DashboardStatisticsDto
            {
                NewCustomersCount = newCustomersCount,
                NewCustomersPercentage = totalCount > 0 ? (double)newCustomersCount / totalCount * 100 : 0,
                UpdateCustomersCount = updateCustomersCount,
                UpdateCustomersPercentage = totalCount > 0 ? (double)updateCustomersCount / totalCount * 100 : 0,
                TotalCustomersCount = totalCount,
                PendingReviewsCount = pendingReviewsCount,
                PendingReviewsPercentage = totalCount > 0 ? (double)pendingReviewsCount / totalCount * 100 : 0,
                ApprovedReviewsCount = approvedReviewsCount,
                ApprovedReviewsPercentage = totalCount > 0 ? (double)approvedReviewsCount / totalCount * 100 : 0,
                RejectedReviewsCount = rejectedReviewsCount,
                RejectedReviewsPercentage = totalCount > 0 ? (double)rejectedReviewsCount / totalCount * 100 : 0
            };
        }

        public async Task<DashboardGraphDataDto> GetDailyRequestsDataAsync(int days = 30, string? userRole = null, string? userBranch = null)
        {
            var startDate = DateTime.UtcNow.AddDays(-days).Date;
            
            using var connection = new OracleConnection(_configuration.GetConnectionString("OracleConnection"));
            await connection.OpenAsync();

            var sql = @"
                SELECT 
                    TO_CHAR(c.""cust_d_entrydt"", 'YYYY-MM-DD') AS date_str,
                    COUNT(*) AS count
                FROM SSDBONLINE.""cust_mast"" c
                WHERE c.""cust_d_entrydt"" >= :startDate
            ";

            // Add branch filtering for non-admin users
            if (!string.IsNullOrWhiteSpace(userBranch) && userRole != "Admin")
            {
                sql += " AND c.\"branch_c_code\" = :userBranch";
            }

            sql += @"
                GROUP BY TO_CHAR(c.""cust_d_entrydt"", 'YYYY-MM-DD')
                ORDER BY date_str
            ";

            using var command = connection.CreateCommand();
            command.CommandText = sql;

            var startDateParam = new OracleParameter("startDate", OracleDbType.Date, startDate, ParameterDirection.Input);
            command.Parameters.Add(startDateParam);

            // Add branch parameter if needed
            if (!string.IsNullOrWhiteSpace(userBranch) && userRole != "Admin")
            {
                var branchParam = new OracleParameter("userBranch", OracleDbType.Varchar2, userBranch, ParameterDirection.Input);
                command.Parameters.Add(branchParam);
            }

            var dailyRequestDtos = new List<DailyRequestDataDto>();

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                dailyRequestDtos.Add(new DailyRequestDataDto
                {
                    Date = reader.GetString(reader.GetOrdinal("date_str")),
                    Count = reader.GetInt32(reader.GetOrdinal("count"))
                });
            }

            // Fill in missing dates with zero counts
            var allDates = new List<DailyRequestDataDto>();
            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var existingData = dailyRequestDtos.FirstOrDefault(d => d.Date == date.ToString("yyyy-MM-dd"));
                
                allDates.Add(new DailyRequestDataDto
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Count = existingData?.Count ?? 0
                });
            }

            return new DashboardGraphDataDto
            {
                DailyRequests = allDates
            };
        }
    }
}
