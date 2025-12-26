using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RegistrationPortal.Server.DTOs.Dashboard;
using RegistrationPortal.Server.Services;
using RegistrationPortal.Server.Attributes;
using RegistrationPortal.Server.Constants;
using System.Security.Claims;

namespace RegistrationPortal.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("statistics")]
        [RequirePermission(Permissions.Dashboard.View)]
        public async Task<ActionResult<DashboardStatisticsDto>> GetDashboardStatistics()
        {
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var userBranch = User.FindFirst("Branch")?.Value;
                
                var statistics = await _dashboardService.GetDashboardStatisticsAsync(userRole, userBranch);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("daily-requests")]
        [RequirePermission(Permissions.Dashboard.View)]
        public async Task<ActionResult<DashboardGraphDataDto>> GetDailyRequests([FromQuery] int days = 30)
        {
            try
            {
                if (days <= 0 || days > 365)
                {
                    return BadRequest(new { error = "Days parameter must be between 1 and 365" });
                }

                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var userBranch = User.FindFirst("Branch")?.Value;
                
                var graphData = await _dashboardService.GetDailyRequestsDataAsync(days, userRole, userBranch);
                return Ok(graphData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }
}
