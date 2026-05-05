using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIFS.Application.AlgoModels;
using SIFS.Infrastructure.Authorization;
using SIFS.Infrastructure.Identity;
using SIFS.Shared.Results;

namespace SIFS.Api.Admin
{
    [ApiController]
    [Route("api/admin/algos")]
    [Authorize]
    public class AlgoModelsController : ControllerBase
    {
        private readonly IAlgoModelService _algoModelService;
        private readonly ICurrentService _currentService;

        public AlgoModelsController(
            IAlgoModelService algoModelService,
            ICurrentService currentService)
        {
            _algoModelService = algoModelService;
            _currentService = currentService;
        }

        [HttpPost]
        [RequirePermission("algo:create")]
        public async Task<IActionResult> Create([FromBody] AlgoModelCreateDto dto)
        {
            var result = await _algoModelService.CreateAlgoAsync(dto, _currentService.RequiredUuid);
            return ToActionResult(result);
        }

        [HttpPut("{id:int}")]
        [RequirePermission("algo:update")]
        public async Task<IActionResult> Update(int id, [FromBody] AlgoModelUpdateDto dto)
        {
            var result = await _algoModelService.UpdateAlgoAsync(id, dto, _currentService.RequiredUuid);
            return ToActionResult(result);
        }

        [HttpPost("{id:int}/enable")]
        [RequirePermission("algo:enable")]
        public async Task<IActionResult> Enable(int id)
        {
            var result = await _algoModelService.EnableAlgoAsync(id, _currentService.RequiredUuid);
            return ToActionResult(result);
        }

        [HttpPost("{id:int}/disable")]
        [RequirePermission("algo:enable")]
        public async Task<IActionResult> Disable(int id)
        {
            var result = await _algoModelService.DisableAlgoAsync(id, _currentService.RequiredUuid);
            return ToActionResult(result);
        }

        [HttpGet]
        [RequirePermission("algo:view")]
        public async Task<IActionResult> List([FromQuery] AlgoModelQuery query)
        {
            var result = await _algoModelService.ListAlgosAsync(query);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(new
            {
                items = result.Data.Data,
                total = result.Data.Total,
                page = result.Data.PageNumber,
                page_size = result.Data.PageSize
            });
        }

        [HttpGet("{id:int}")]
        [RequirePermission("algo:view")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _algoModelService.GetAlgoAsync(id);
            return ToActionResult(result);
        }

        [HttpDelete("{id:int}")]
        [RequirePermission("algo:update")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _algoModelService.SoftDeleteAlgoAsync(id, _currentService.RequiredUuid);
            if (result.IsSuccess)
                return Ok(result.Message);

            return result.Code == ResultCode.NotFound
                ? NotFound(result.Message)
                : BadRequest(result.Message);
        }

        private IActionResult ToActionResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
                return Ok(result.Data);

            return result.Code switch
            {
                ResultCode.Forbidden => Forbid(),
                ResultCode.NotFound => NotFound(result.Message),
                _ => BadRequest(result.Message)
            };
        }
    }
}
