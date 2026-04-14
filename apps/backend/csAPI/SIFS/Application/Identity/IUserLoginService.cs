using SIFS.Api.Identity;
using SIFS.Shared.Results;

namespace SIFS.Application.Identity
{
    public interface IUserLoginService
    {
        Task<Result<LoginTokenResult>> LoginAsync(UserLoginDto userLoginDto);
        Task<Result<string>> RefreshTokenAsync(string refreshToken);
    }
}
