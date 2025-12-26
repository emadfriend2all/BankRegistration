using RegistrationPortal.Server.Entities;

namespace RegistrationPortal.Server.Repositories
{
    public interface ICustomerDocumentRepository
    {
        Task<CustomerDocument?> GetByIdAsync(string id);
        Task<IEnumerable<CustomerDocument>> GetByCustomerIdAsync(string customerId);
        Task<IEnumerable<CustomerDocument>> GetAllAsync();
        Task<CustomerDocument> AddAsync(CustomerDocument entity);
        Task<CustomerDocument> UpdateAsync(CustomerDocument entity);
        Task<CustomerDocument> DeleteAsync(CustomerDocument entity);
        Task<int> SaveChangesAsync();
    }
}
