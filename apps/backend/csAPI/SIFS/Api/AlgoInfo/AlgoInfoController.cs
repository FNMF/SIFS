using Microsoft.AspNetCore.Mvc;
using SIFS.Application.AlgoTaskApp;

namespace SIFS.Api.AlgoInfo
{
    [ApiController]
    [Route("api/info")]
    public class AlgoInfoController:ControllerBase
    {
        private readonly IAlgoInfoService _algoInfoService;
        public AlgoInfoController(IAlgoInfoService algoInfoService)
        {
            _algoInfoService = algoInfoService;
        }
        [HttpGet("algo")]
        public async Task<IActionResult> GetAlgo()
        {
            var result = await _algoInfoService.GetAllAsync();
            if (!result.IsSuccess)
            {
                return BadRequest();
            }
            return Ok(result.Data);
        }
    }
}
