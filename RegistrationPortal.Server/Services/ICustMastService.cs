using RegistrationPortal.Server.Entities;
using RegistrationPortal.Server.DTOs;

namespace RegistrationPortal.Server.Services
{
    public interface ICustMastService
    {
        Task<CustMast?> GetCustomerAsync(string branchCode, decimal custId);
        Task<IEnumerable<CustMast>> GetAllCustomersAsync();
        Task<IEnumerable<CustMast>> GetCustomersByBranchAsync(string branchCode);
        Task<CustMast> CreateCustomerAsync(CreateCustomerDto customerDto);
        Task<CustMast> UpdateCustomerAsync(CustMast customer);
        Task<bool> DeleteCustomerAsync(string branchCode, decimal custId);
        Task<IEnumerable<AccountMast>> GetCustomerAccountsAsync(string branchCode, decimal custId);
    }
}
