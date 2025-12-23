using Microsoft.AspNetCore.Mvc;
using RegistrationPortal.Server.Entities;
using RegistrationPortal.Server.Services;
using RegistrationPortal.Server.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;

namespace RegistrationPortal.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustMastController : ControllerBase
    {
        private readonly ICustMastService _custMastService;

        public CustMastController(ICustMastService custMastService)
        {
            _custMastService = custMastService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustMast>>> GetAllCustomers()
        {
            try
            {
                var customers = await _custMastService.GetAllCustomersAsync();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("{branchCode}/{custId}")]
        public async Task<ActionResult<CustMast>> GetCustomer(string branchCode, decimal custId)
        {
            try
            {
                var customer = await _custMastService.GetCustomerAsync(branchCode, custId);
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
    }
}
