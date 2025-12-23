using RegistrationPortal.Server.Entities;

namespace RegistrationPortal.Server.Repositories
{
    public interface IAccountMastRepository : IRepository<AccountMast>
    {
        Task<AccountMast?> GetByCompositeKeyAsync(string branchCode, string actType, decimal custNo, string? currencyCode);
        Task<IEnumerable<AccountMast>> GetByCustomerAsync(string branchCode, decimal custNo);
        Task<IEnumerable<AccountMast>> GetByBranchAsync(string branchCode);
    }
}
