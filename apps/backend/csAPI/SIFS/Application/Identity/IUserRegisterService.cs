using SIFS.Api.Identity;
using SIFS.Shared.Results;

namespace SIFS.Application.Identity
{
    public interface IUserRegisterService
    {
        Task<Result<LoginTokenResult>> CreateUserAsync(UserRegisterDto userCreateDto);
    }
}
