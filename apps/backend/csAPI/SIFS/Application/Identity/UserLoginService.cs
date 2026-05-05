using SIFS.Api.Identity;
using SIFS.Infrastructure.Identity;
using SIFS.Infrastructure.Repositories;
using SIFS.Shared.Helpers;
using SIFS.Shared.Helpers.JWT;
using SIFS.Shared.Results;
using SIFS.Shared.Extensions.EventBus;

namespace SIFS.Application.Identity
{
    public class UserLoginService : IUserLoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ICurrentService _currentService;
        private readonly IJwtHelper _jwtHelper;
        private readonly IEventBus _eventBus;
        public UserLoginService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IRefreshTokenService refreshTokenService,
            ICurrentService currentService,
            IJwtHelper jwtHelper,
            IEventBus eventBus)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _refreshTokenService = refreshTokenService;
            _currentService = currentService;
            _jwtHelper = jwtHelper;
            _eventBus = eventBus;
        }
        public async Task<Result<LoginTokenResult>> LoginAsync(UserLoginDto userLoginDto)
        {
            var userResult = await _userRepository.GetUserByAccountAsync(userLoginDto.Account);
            if (!userResult.IsSuccess)
                return Result<LoginTokenResult>.Fail(ResultCode.NotFound, "用户不存在");
            var user = userResult.Data;
            var isPasswordValid = HashHelper.Hashing(userLoginDto.Password, user.Salt) == user.PasswordHashed;
            if (!isPasswordValid)
                return Result<LoginTokenResult>.Fail(ResultCode.Unauthorized, "密码错误");
            var userReadDto = new UserReadDto
            {
                Id = user.Id,
                Account = user.Account
            };
            var token = _jwtHelper.UserGenerateToken(user.Id, user.Account);
            var refreshToken = await _refreshTokenRepository.AddWeekRefreshTokenAsync(user.Id);
            var loginTokenResult = new LoginTokenResult
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresIn = _jwtHelper.GetExpiresMinutes(),
                UserReadDto = userReadDto,
            };
            _eventBus.Publish(new AppEvent
            {
                EventType = AppEventTypes.UserLogin,
                ActorId = user.Id,
                TargetType = "user",
                TargetId = user.Id.ToString(),
                Payload = new Dictionary<string, object?>
                {
                    ["account"] = user.Account
                }
            });
            return Result<LoginTokenResult>.Success(loginTokenResult);
        }
        public async Task<Result<string>> RefreshTokenAsync(string refreshToken)
        {
            var id = _currentService.RequiredUuid;
            var refreshTokenResult = await _refreshTokenService.VerifyToken(id, refreshToken);
            if(!refreshTokenResult.IsSuccess)
                return Result<string>.Fail(refreshTokenResult.Code, refreshTokenResult.Message);

            var userResult = await _userRepository.GetUserByIdAsync(id);
            if(!userResult.IsSuccess)
                return Result<string>.Fail(userResult.Code, userResult.Message);

            var user = userResult.Data;

            var newAccessToken = _jwtHelper.UserGenerateToken(user.Id, user.Account);

            return Result<string>.Success(newAccessToken);
        }
    }
}
