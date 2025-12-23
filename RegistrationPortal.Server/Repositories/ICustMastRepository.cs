using RegistrationPortal.Server.Entities;

namespace RegistrationPortal.Server.Repositories
{
    public interface ICustMastRepository : IRepository<CustMast>
    {
        Task<CustMast?> GetByBranchAndIdAsync(string branchCode, decimal custId);
        Task<IEnumerable<CustMast>> GetByBranchAsync(string branchCode);
        Task<IEnumerable<AccountMast>> GetCustomerAccountsAsync(string branchCode, decimal custId);
        Task<decimal> GetNextCustomerIdAsync(string branchCode);
        
        // New methods for duplicate checking
        Task<CustMast?> GetByIdNumberAndBranchAsync(string idNumber, string branchCode);
        Task<CustMast?> GetByPhoneNumberAndBranchAsync(string phoneNumber, string branchCode);
        Task<bool> HasAccountTypeInBranchAsync(decimal customerId, string accountType, string branchCode);
        
        // FATCA methods
        Task AddFatcaAsync(CustomerFatca fatca);
    }
}
