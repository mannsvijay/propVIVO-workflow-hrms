
// ================================================================
// FILE: Modules/AuthFeature/AuthFeature.Application/Repository/IAuthRepository.cs
// WHY: Repository interface following the same pattern as ITodoRepository
// ================================================================
using AuthFeature.Domain;
using HRMS.Core.Postgres.Repositories;
 
namespace AuthFeature.Application.Repository
{
    public interface IAuthRepository : IPostgresRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
        Task UpdateRefreshTokenAsync(string userId, string? refreshToken, DateTime? expiry);
        Task UpdateLastLoginAsync(string userId);
        Task<Role?> GetRoleByIdAsync(string roleId);
        Task<IEnumerable<Role>> GetAllRolesAsync();
    }
}