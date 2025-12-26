using Microsoft.EntityFrameworkCore;
using RegistrationPortal.Server.Data;

namespace RegistrationPortal.Server.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(params object[] keyValues);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IQueryable<T>> GetAllQueryableAsync();
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<T> DeleteAsync(T entity);
        Task<int> SaveChangesAsync();
    }
}
