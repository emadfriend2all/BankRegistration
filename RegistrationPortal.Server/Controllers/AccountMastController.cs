using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistrationPortal.Server.Constants;
using RegistrationPortal.Server.Entities;
using RegistrationPortal.Server.Services;

namespace RegistrationPortal.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountMastController : ControllerBase
    {
        private readonly IAccountMastService _accountMastService;

        public AccountMastController(IAccountMastService accountMastService)
        {
            _accountMastService = accountMastService;
        }

        [HttpGet]
        [Authorize(Policy = Permissions.Accounts.List)]
        public async Task<ActionResult<IEnumerable<AccountMast>>> GetAllAccounts()
        {
            try
            {
                var accounts = await _accountMastService.GetAllAccountsAsync();
                return Ok(accounts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{branchCode}/{actType}/{custNo}/{currencyCode}")]
        [Authorize(Policy = Permissions.Accounts.List)]
        public async Task<ActionResult<AccountMast>> GetAccount(string branchCode, string actType, decimal custNo, string currencyCode)
        {
            try
            {
                var account = await _accountMastService.GetAccountAsync(branchCode, actType, custNo, currencyCode);
                if (account == null)
                    return NotFound();

                return Ok(account);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("customer/{branchCode}/{custNo}")]
        [Authorize(Policy = Permissions.Accounts.List)]
        public async Task<ActionResult<IEnumerable<AccountMast>>> GetAccountsByCustomer(string branchCode, decimal custNo)
        {
            try
            {
                var accounts = await _accountMastService.GetAccountsByCustomerAsync(branchCode, custNo);
                return Ok(accounts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("branch/{branchCode}")]
        [Authorize(Policy = Permissions.Accounts.List)]
        public async Task<ActionResult<IEnumerable<AccountMast>>> GetAccountsByBranch(string branchCode)
        {
            try
            {
                var accounts = await _accountMastService.GetAccountsByBranchAsync(branchCode);
                return Ok(accounts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Policy = Permissions.Accounts.Create)]
        public async Task<ActionResult<AccountMast>> CreateAccount(AccountMast account)
        {
            try
            {
                var createdAccount = await _accountMastService.CreateAccountAsync(account);
                return CreatedAtAction(
                    nameof(GetAccount), 
                    new { 
                        branchCode = createdAccount.BranchCCode, 
                        actType = createdAccount.ActCType, 
                        custNo = createdAccount.CustINo, 
                        currencyCode = createdAccount.CurrencyCCode 
                    }, 
                    createdAccount);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{branchCode}/{actType}/{custNo}/{currencyCode}")]
        [Authorize(Policy = Permissions.Accounts.Update)]
        public async Task<ActionResult<AccountMast>> UpdateAccount(string branchCode, string actType, decimal custNo, string currencyCode, AccountMast account)
        {
            try
            {
                if (account.BranchCCode != branchCode || 
                    account.ActCType != actType || 
                    account.CustINo != custNo || 
                    account.CurrencyCCode != currencyCode)
                    return BadRequest("Account key mismatch");

                var existingAccount = await _accountMastService.GetAccountAsync(branchCode, actType, custNo, currencyCode);
                if (existingAccount == null)
                    return NotFound();

                var updatedAccount = await _accountMastService.UpdateAccountAsync(account);
                return Ok(updatedAccount);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{branchCode}/{actType}/{custNo}/{currencyCode}")]
        [Authorize(Policy = Permissions.Accounts.Delete)]
        public async Task<ActionResult<bool>> DeleteAccount(string branchCode, string actType, decimal custNo, string currencyCode)
        {
            try
            {
                var result = await _accountMastService.DeleteAccountAsync(branchCode, actType, custNo, currencyCode);
                if (!result)
                    return NotFound();

                return Ok(true);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
