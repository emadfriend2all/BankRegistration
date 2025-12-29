using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RegistrationPortal.Server.DTOs;
using RegistrationPortal.Server.Services;
using RegistrationPortal.Server.Constants;
using RegistrationPortal.Server.Attributes;
using RegistrationPortal.Server.DTOs.Pagination;
using RegistrationPortal.Server.Entities;
using System.Security.Claims;

namespace RegistrationPortal.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustMastController : ControllerBase
    {
        private readonly ICustMastService _custMastService;
        private readonly IAuthService _authService;
        private readonly IAppLoggerService _logger;

        public CustMastController(ICustMastService custMastService, IAuthService authService, IAppLoggerService logger)
        {
            _custMastService = custMastService;
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        [RequirePermission(Permissions.Customers.List)]
        public async Task<ActionResult<PaginatedResultDto<CustMastDto>>> GetAllCustomers([FromQuery] PaginationParameters parameters)
        {
            try
            {
                await _logger.LogInformationAsync("Getting all customers with parameters: Page={Page}, PageSize={PageSize}", 
                    parameters.PageNumber, parameters.PageSize);

                // Get user role from JWT token
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                
                // Get current user information including branch
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                string? userBranch = null;
                
                if (userIdClaim != null)
                {
                    var currentUser = await _authService.GetUserByIdAsync(int.Parse(userIdClaim.Value));
                    userBranch = currentUser?.Branch;
                }
                
                var customers = await _custMastService.GetAllCustomersAsync(parameters, userRole, userBranch);
                
                await _logger.LogInformationAsync("Successfully retrieved {Count} customers", customers.Data.Count());
                return Ok(customers);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Error occurred while getting all customers");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [RequirePermission(Permissions.Customers.ViewDetails)]
        public async Task<ActionResult<GetCustMastByIdDto>> GetCustomerById(string id)
        {
            try
            {
                var customer = await _custMastService.GetCustomerByIdAsync(id);
                if (customer == null)
                    return NotFound();

                return Ok(customer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("branch/{branchCode}")]
        [RequirePermission(Permissions.Customers.List)]
        public async Task<ActionResult<IEnumerable<CustMast>>> GetCustomersByBranch(string branchCode)
        {
            try
            {
                var customers = await _custMastService.GetCustomersByBranchAsync(branchCode);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("{branchCode}/{custId}/accounts")]
        [RequirePermission(Permissions.Customers.ViewDetails)]
        public async Task<ActionResult<IEnumerable<AccountMast>>> GetCustomerAccounts(string branchCode, decimal custId)
        {
            try
            {
                var accounts = await _custMastService.GetCustomerAccountsAsync(branchCode, custId);
                return Ok(accounts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<CustMast>> CreateCustomer([FromForm] CreateCustomerDto customerDto)
        {
            try
            {
                foreach (var item in customerDto.AccountMasts ?? [])
                {
                    item.ActCActsts = "New";
                }
                var createdCustomer = await _custMastService.CreateCustomerAsync(customerDto);
                return Ok(createdCustomer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPut("{branchCode}/{custId}")]
        [RequirePermission(Permissions.Customers.Update)]
        public async Task<ActionResult<CustMast>> UpdateCustomer(string branchCode, decimal custId, CustMast customer)
        {
            try
            {
                if (customer.BranchCCode != branchCode || customer.CustIId != custId)
                    return BadRequest("Customer ID mismatch");

                var existingCustomer = await _custMastService.GetCustomerAsync(branchCode, custId);
                if (existingCustomer == null)
                    return NotFound();

                var updatedCustomer = await _custMastService.UpdateCustomerAsync(customer);
                return Ok(updatedCustomer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpDelete("{branchCode}/{custId}")]
        [RequirePermission(Permissions.Customers.Delete)]
        public async Task<ActionResult<bool>> DeleteCustomer(string branchCode, decimal custId)
        {
            try
            {
                var result = await _custMastService.DeleteCustomerAsync(branchCode, custId);
                if (!result)
                    return NotFound();

                return Ok(true);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPut("review")]
        [RequirePermission(Permissions.Customers.Review)]
        public async Task<ActionResult<bool>> UpdateCustomerReview([FromBody] UpdateCustomerReviewDto reviewDto)
        {
            try
            {
                // Get current user from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                var currentUser = await _authService.GetUserByIdAsync(int.Parse(userIdClaim.Value));
                if (currentUser == null)
                {
                    return Unauthorized(new { error = "User not found" });
                }

                // Get reviewedBy from current user username
                var reviewedBy = currentUser.Username;

                var result = await _custMastService.UpdateCustomerReviewAsync(reviewDto, reviewedBy);
                if (!result)
                    return NotFound();

                return Ok(true);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }
}
