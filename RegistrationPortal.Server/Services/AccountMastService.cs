using RegistrationPortal.Server.Entities;
using RegistrationPortal.Server.Repositories;
using Microsoft.Extensions.Logging;

namespace RegistrationPortal.Server.Services
{
    public class AccountMastService : IAccountMastService
    {
        private readonly IAccountMastRepository _accountMastRepository;
        private readonly ICustMastRepository _custMastRepository;
        private readonly ILogger<AccountMastService> _logger;

        public AccountMastService(IAccountMastRepository accountMastRepository, 
                                 ICustMastRepository custMastRepository,
                                 ILogger<AccountMastService> logger)
        {
            _accountMastRepository = accountMastRepository;
            _custMastRepository = custMastRepository;
            _logger = logger;
        }

        public async Task<AccountMast?> GetAccountAsync(string branchCode, string actType, decimal custNo, string? currencyCode)
        {
            return await _accountMastRepository.GetByCompositeKeyAsync(branchCode, actType, custNo, currencyCode);
        }

        public async Task<IEnumerable<AccountMast>> GetAllAccountsAsync()
        {
            return await _accountMastRepository.GetAllAsync();
        }

        public async Task<IEnumerable<AccountMast>> GetAccountsByCustomerAsync(string branchCode, decimal custNo)
        {
            return await _accountMastRepository.GetByCustomerAsync(branchCode, custNo);
        }

        public async Task<IEnumerable<AccountMast>> GetAccountsByBranchAsync(string branchCode)
        {
            return await _accountMastRepository.GetByBranchAsync(branchCode);
        }

        public async Task<AccountMast> CreateAccountAsync(AccountMast account)
        {
            try
            {
                // Validate for duplicate account type in the same branch for the same customer
                await ValidateAccountDuplicatesAsync(account);

                await _accountMastRepository.AddAsync(account);
                await _accountMastRepository.SaveChangesAsync();
                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account");
                throw;
            }
        }

        private async Task ValidateAccountDuplicatesAsync(AccountMast account)
        {
            var validationErrors = new List<string>();

            // Check if the customer already has an account of the same type in the same branch
            if (account.CustINo > 0 && !string.IsNullOrEmpty(account.ActCType) && !string.IsNullOrEmpty(account.BranchCCode))
            {
                var hasDuplicateAccountType = await _custMastRepository.HasAccountTypeInBranchAsync(
                    account.CustINo, account.ActCType, account.BranchCCode);

                if (hasDuplicateAccountType)
                {
                    validationErrors.Add($"للعميل حساب بالفعل من نوع ({account.ActCType}) في هذا الفرع");
                }
            }

            if (validationErrors.Any())
            {
                throw new InvalidOperationException(string.Join("; ", validationErrors));
            }
        }

        public async Task<AccountMast> UpdateAccountAsync(AccountMast account)
        {
            try
            {
                await _accountMastRepository.UpdateAsync(account);
                await _accountMastRepository.SaveChangesAsync();
                return account;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> DeleteAccountAsync(string branchCode, string actType, decimal custNo, string? currencyCode)
        {
            try
            {
                var account = await _accountMastRepository.GetByCompositeKeyAsync(branchCode, actType, custNo, currencyCode);
                if (account == null)
                    return false;

                await _accountMastRepository.DeleteAsync(account);
                await _accountMastRepository.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
