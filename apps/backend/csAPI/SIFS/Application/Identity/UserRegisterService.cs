using SIFS.Api.Identity;
using SIFS.Infrastructure.Repositories;
using SIFS.Shared.Helpers;
using SIFS.Shared.Results;

namespace SIFS.Application.Identity
{
    public class UserRegisterService : IUserRegisterService
    {
        private readonly IUserRepository _userRepository;
        public UserRegisterService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        /*
        public async Task<Result<UserReadDto>> CreateUserAsync(UserRegisterDto userCreateDto)
        {
            var existingUserResult = await _userRepository.GetUserByAccountAsync(userCreateDto.Account);
            if (existingUserResult.IsSuccess)
                return Result<UserReadDto>.Fail(ResultCode.InfoExist, "账号已存在");
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
            return Result<UserReadDto>.Success(userReadDto);
        }*/
    }
}
