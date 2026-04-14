using Microsoft.EntityFrameworkCore;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Results;
using System.Security.Cryptography;
using System.Text;

namespace SIFS.Infrastructure.Repositories
{
    public class RefreshTokenRepository: IRefreshTokenRepository
    {
        private readonly SIFSContext _context;
        public RefreshTokenRepository(SIFSContext context)
        {
            _context = context;
        }
        public async Task<List<RefreshToken>> GetRefreshTokensByUserIdAsync(Guid userId)
        {
            return await _context.RefreshTokens
                .Where(t => t.UserId == userId && !t.IsRevoked)
                .OrderByDescending(t =>t.ExpiresAt)
                .ToListAsync();
        }
        public async Task<string> AddWeekRefreshTokenAsync(Guid userId)
        {
            // 生成64随机字符串
            var rawToken = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64)
                );
            // hash处理
            var tokenHash = Convert.ToBase64String(
                SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))
                );
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),     // 统一UTC，这里是持续一周的刷新令牌
                IsRevoked = false,
            };
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return tokenHash;
        }
        public async Task UpdateRefreshTokenAsync(RefreshToken token)
        {
            _context.RefreshTokens.Update(token);
            await _context.SaveChangesAsync();
        }
    }
}
