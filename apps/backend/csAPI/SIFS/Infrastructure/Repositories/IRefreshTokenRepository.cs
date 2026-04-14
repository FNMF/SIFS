using SIFS.Infrastructure.Persistence.Models;

namespace SIFS.Infrastructure.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<List<RefreshToken>> GetRefreshTokensByUserIdAsync(Guid userId);
        Task<string> AddWeekRefreshTokenAsync(Guid userId);
        Task UpdateRefreshTokenAsync(RefreshToken token);
    }
}
