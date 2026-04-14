using SIFS.Api.Identity;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Infrastructure.Repositories;
using SIFS.Shared.Helpers;
using SIFS.Shared.Helpers.JWT;
using SIFS.Shared.Results;

namespace SIFS.Application.Identity
{
    public class UserRegisterService : IUserRegisterService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IJwtHelper _jwtHelper;
        public UserRegisterService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IJwtHelper jwtHelper)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtHelper = jwtHelper;
        }
        
        public async Task<Result<LoginTokenResult>> CreateUserAsync(UserRegisterDto userCreateDto)
        {
            var existingUserResult = await _userRepository.GetUserByAccountAsync(userCreateDto.Account);
            if (existingUserResult.IsSuccess)
                return Result<LoginTokenResult>.Fail(ResultCode.InfoExist, "账号已存在");
            var hashsalt = HashHelper.HashandSalt(userCreateDto.Password);
            var user = new User
            {
                Id = UuidV7.NewUuidV7(),
                Account = userCreateDto.Account,
                PasswordHashed = hashsalt.Hash,
                Salt = hashsalt.Salt
            };
            var result = await _userRepository.CreateUserAsync(user);
            var userReadDto = new UserReadDto
            {
                Id = result.Data.Id,
                Account = result.Data.Account
            };

            var accessToken = _jwtHelper.UserGenerateToken(user.Id, user.Account);
            var refreshToken = await _refreshTokenRepository.AddWeekRefreshTokenAsync(user.Id);
            var loginTokenResult = new LoginTokenResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = _jwtHelper.GetExpiresMinutes(),
                UserReadDto = userReadDto
            };
            return Result<LoginTokenResult>.Success(loginTokenResult);
        }
    }
}
