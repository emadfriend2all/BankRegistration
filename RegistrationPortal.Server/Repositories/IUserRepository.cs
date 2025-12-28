using RegistrationPortal.Server.DTOs.Pagination;
using RegistrationPortal.Server.DTOs.Auth;

namespace RegistrationPortal.Server.Repositories
{
    public interface IUserRepository
    {
        Task<PaginatedResultDto<UserListDto>> GetUsersPaginatedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? sortBy = null,
            bool sortDescending = false,
            string? status = null);
    }
}
