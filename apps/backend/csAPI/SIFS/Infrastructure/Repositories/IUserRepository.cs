using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Repositories
{
    public interface IUserRepository
    {
        Task<Result<User>> GetUserByIdAsync(Guid userId);
        Task<Result<User>> GetUserByAccountAsync(string account);
        Task<Result<User>> CreateUserAsync(User user);
    }
}
