using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Identity
{
    public interface IRefreshTokenService
    {
        Task<Result<List<RefreshToken>>> GetByUserIdAsync(Guid userId);
        Task<Result> VerifyToken(Guid id, string refreshToken);
    }
}
