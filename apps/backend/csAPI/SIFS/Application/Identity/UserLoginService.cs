using SIFS.Api.Identity;
using SIFS.Infrastructure.Repositories;
using SIFS.Shared.Helpers;
using SIFS.Shared.Results;

namespace SIFS.Application.Identity
{
    public class UserLoginService : IUserLoginService
    {
        private readonly IUserRepository _userRepository;
        public UserLoginService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<Result<UserReadDto>> LoginAsync(UserLoginDto userLoginDto)
        {
            var userResult = await _userRepository.GetUserByAccountAsync(userLoginDto.Account);
            if (!userResult.IsSuccess)
                return Result<UserReadDto>.Fail(ResultCode.NotFound, "用户不存在");
            var user = userResult.Data;
            var isPasswordValid = HashHelper.Hashing(userLoginDto.Password, user.Salt) == user.PasswordHashed;
            if (!isPasswordValid)
                return Result<UserReadDto>.Fail(ResultCode.Unauthorized, "密码错误");
            var userReadDto = new UserReadDto
            {
                Id = user.Id,
                Account = user.Account
            };
            return Result<UserReadDto>.Success(userReadDto);
        }
    }
}
