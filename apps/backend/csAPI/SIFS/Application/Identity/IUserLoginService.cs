using SIFS.Api.Identity;
using SIFS.Shared.Results;

namespace SIFS.Application.Identity
{
    public interface IUserLoginService
    {
        Task<Result<UserReadDto>> LoginAsync(UserLoginDto userLoginDto);
    }
}
