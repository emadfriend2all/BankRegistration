using RegistrationPortal.Server.Entities;

namespace RegistrationPortal.Server.Services
{
    public interface IAccountMastService
    {
        Task<AccountMast?> GetAccountAsync(string branchCode, string actType, decimal custNo, string? currencyCode);
        Task<IEnumerable<AccountMast>> GetAllAccountsAsync();
        Task<IEnumerable<AccountMast>> GetAccountsByCustomerAsync(string branchCode, decimal custNo);
        Task<IEnumerable<AccountMast>> GetAccountsByBranchAsync(string branchCode);
        Task<AccountMast> CreateAccountAsync(AccountMast account);
        Task<AccountMast> UpdateAccountAsync(AccountMast account);
        Task<bool> DeleteAccountAsync(string branchCode, string actType, decimal custNo, string? currencyCode);
    }
}
