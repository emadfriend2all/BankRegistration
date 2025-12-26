using Microsoft.EntityFrameworkCore;
using RegistrationPortal.Server.Data;
using RegistrationPortal.Server.Entities;

namespace RegistrationPortal.Server.Repositories
{
    public class CustomerDocumentRepository : ICustomerDocumentRepository
    {
        protected readonly RegistrationPortalDbContext _context;
        protected readonly DbSet<CustomerDocument> _dbSet;

        public CustomerDocumentRepository(RegistrationPortalDbContext context)
        {
            _context = context;
            _dbSet = context.Set<CustomerDocument>();
        }

        public async Task<CustomerDocument?> GetByIdAsync(string id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<CustomerDocument>> GetByCustomerIdAsync(string customerId)
        {
            return await _dbSet
                .Where(d => d.CustomerId == customerId)
                .OrderBy(d => d.DocumentType)
                .ToListAsync();
        }

        public async Task<IEnumerable<CustomerDocument>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<CustomerDocument> AddAsync(CustomerDocument entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task<CustomerDocument> UpdateAsync(CustomerDocument entity)
        {
            _dbSet.Update(entity);
            return entity;
        }

        public async Task<CustomerDocument> DeleteAsync(CustomerDocument entity)
        {
            _dbSet.Remove(entity);
            return entity;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
