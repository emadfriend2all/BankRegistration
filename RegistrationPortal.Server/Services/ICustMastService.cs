using RegistrationPortal.Server.Entities;
using RegistrationPortal.Server.DTOs;
using RegistrationPortal.Server.DTOs.Pagination;

namespace RegistrationPortal.Server.Services
{
    public interface ICustMastService
    {
        Task<PaginatedResultDto<CustMastDto>> GetAllCustomersAsync(PaginationParameters parameters, string? userRole = null, string? userBranch = null);
        Task<GetCustMastByIdDto?> GetCustomerByIdAsync(string id);
        Task<CustMast?> GetCustomerAsync(string branchCode, decimal custId);
        Task<IEnumerable<CustMast>> GetCustomersByBranchAsync(string branchCode);
        Task<CustMast> CreateCustomerAsync(CreateCustomerDto customerDto);
        Task<CustMast> UpdateCustomerAsync(CustMast customer);
        Task<bool> DeleteCustomerAsync(string branchCode, decimal custId);
        Task<IEnumerable<AccountMast>> GetCustomerAccountsAsync(string branchCode, decimal custId);
        Task<bool> UpdateCustomerReviewAsync(UpdateCustomerReviewDto reviewDto, string reviewedBy);
    }
}
