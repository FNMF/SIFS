using Microsoft.AspNetCore.Mvc;
using SIFS.Application.Identity;
using SIFS.Infrastructure.Identity;

namespace SIFS.Api.Identity
{
    [ApiController]
    [Route("api/identity")]
    public class IdentityController:ControllerBase
    {
        private readonly IUserLoginService _userLoginService;
        private readonly IUserRegisterService _userRegisterService;
        public IdentityController(IUserLoginService userLoginService, IUserRegisterService userRegisterService)
        {
            _userLoginService = userLoginService;
            _userRegisterService = userRegisterService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            var result = await _userLoginService.LoginAsync(dto);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            
            return Ok(result.Data);
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            var result = await _userRegisterService.CreateUserAsync(dto);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"]
                    .FirstOrDefault()?
                    .Replace("Bearer ", "");
            if (accessToken == null)
            {
                return BadRequest("无效的请求数据");
            }

            var result = await _userLoginService.RefreshTokenAsync(refreshToken);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return Unauthorized(result);
            }
        }
    }
}
