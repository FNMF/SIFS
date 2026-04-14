using SIFS.Infrastructure.Persistence.Models;
using SIFS.Infrastructure.Repositories;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Identity
{
    public class RefreshTokenService: IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository)
        {
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<Result<List<RefreshToken>>> GetByUserIdAsync(Guid userId)
        {
            var tokens = await _refreshTokenRepository.GetRefreshTokensByUserIdAsync(userId);
            if(tokens == null)
            {
                return Result<List<RefreshToken>>.Fail(ResultCode.NotFound,"未找到相关Tokens");
            }
            return Result<List<RefreshToken>>.Success(tokens);
        }
        public async Task<Result> VerifyToken(Guid id, string refreshToken)
        {
            try
            {// 尝试获取对应的刷新令牌
                var tokensResult = await GetByUserIdAsync(id);
                // 失败则返回
                if (!tokensResult.IsSuccess)
                {
                    return Result.Fail(tokensResult.Code, tokensResult.Message);
                }

                var tokens = tokensResult.Data;
                var tokenHash = Convert.ToBase64String(
                    System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(refreshToken))
                    );
                // 尝试比对
                var matched = tokens.FirstOrDefault(t =>
                    t.Token == tokenHash &&
                    t.ExpiresAt > DateTime.UtcNow
                    );

                if (matched == null)
                {
                    return Result<RefreshToken>.Fail(
                        ResultCode.TokenInvalid, "RefreshToken 无效或已过期");
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Fail(ResultCode.ServerError, "认证Token时出错");
            }
        }
    }
}
