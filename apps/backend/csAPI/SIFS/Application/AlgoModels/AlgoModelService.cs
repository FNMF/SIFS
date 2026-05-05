using System.Text.Json;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Infrastructure.Repositories;
using SIFS.Shared.Extensions.EventBus;
using SIFS.Shared.Results;

namespace SIFS.Application.AlgoModels
{
    public class AlgoModelService : IAlgoModelService
    {
        private readonly IAlgoModelRepository _algoModelRepository;
        private readonly IEventBus _eventBus;
        private readonly IAppEventRequestContextFactory _requestContextFactory;

        public AlgoModelService(
            IAlgoModelRepository algoModelRepository,
            IEventBus eventBus,
            IAppEventRequestContextFactory requestContextFactory)
        {
            _algoModelRepository = algoModelRepository;
            _eventBus = eventBus;
            _requestContextFactory = requestContextFactory;
        }

        public async Task<Result<AlgoModelDto>> CreateAlgoAsync(AlgoModelCreateDto dto, Guid actorId)
        {
            var validation = await ValidateForCreateAsync(dto);
            if (!validation.IsSuccess)
                return Result<AlgoModelDto>.Fail(validation.Code, validation.Message);

            var now = DateTime.UtcNow;
            var model = new AlgoModel
            {
                Name = dto.Name.Trim(),
                ApiUrl = dto.ApiUrl.Trim(),
                Description = dto.Description,
                ReservedJson = SerializeReservedJson(dto.ReservedJson),
                Enabled = false,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _algoModelRepository.CreateAsync(model);
            Publish(AppEventTypes.AlgoCreated, actorId, model, "create algorithm");
            return Result<AlgoModelDto>.Success(ToDto(model));
        }

        public async Task<Result<AlgoModelDto>> UpdateAlgoAsync(int id, AlgoModelUpdateDto dto, Guid actorId)
        {
            var model = await _algoModelRepository.FindByIdAsync(id);
            if (model == null)
                return Result<AlgoModelDto>.Fail(ResultCode.NotFound, "算法不存在");

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                var name = dto.Name.Trim();
                var existing = await _algoModelRepository.FindByNameAsync(name);
                if (existing != null && existing.Id != id)
                    return Result<AlgoModelDto>.Fail(ResultCode.InfoExist, "算法名称已存在");

                model.Name = name;
            }

            if (dto.ApiUrl != null)
            {
                if (string.IsNullOrWhiteSpace(dto.ApiUrl))
                    return Result<AlgoModelDto>.Fail(ResultCode.InvalidInput, "API URL不能为空");

                model.ApiUrl = dto.ApiUrl.Trim();
            }

            if (dto.Description != null)
                model.Description = dto.Description;

            if (dto.ReservedJson != null)
                model.ReservedJson = SerializeReservedJson(dto.ReservedJson);

            model.UpdatedAt = DateTime.UtcNow;
            await _algoModelRepository.UpdateAsync(model);
            Publish(AppEventTypes.AlgoUpdated, actorId, model, "update algorithm");
            return Result<AlgoModelDto>.Success(ToDto(model));
        }

        public async Task<Result<AlgoModelDto>> EnableAlgoAsync(int id, Guid actorId)
        {
            var model = await _algoModelRepository.FindByIdAsync(id);
            if (model == null)
                return Result<AlgoModelDto>.Fail(ResultCode.NotFound, "算法不存在");

            model.Enabled = true;
            model.UpdatedAt = DateTime.UtcNow;
            await _algoModelRepository.UpdateAsync(model);
            Publish(AppEventTypes.AlgoEnabled, actorId, model, "enable algorithm");
            return Result<AlgoModelDto>.Success(ToDto(model));
        }

        public async Task<Result<AlgoModelDto>> DisableAlgoAsync(int id, Guid actorId)
        {
            var model = await _algoModelRepository.FindByIdAsync(id);
            if (model == null)
                return Result<AlgoModelDto>.Fail(ResultCode.NotFound, "算法不存在");

            model.Enabled = false;
            model.UpdatedAt = DateTime.UtcNow;
            await _algoModelRepository.UpdateAsync(model);
            Publish(AppEventTypes.AlgoDisabled, actorId, model, "disable algorithm");
            return Result<AlgoModelDto>.Success(ToDto(model));
        }

        public async Task<Result<AlgoModelDto>> GetAlgoAsync(int id)
        {
            var model = await _algoModelRepository.FindByIdAsync(id);
            return model == null
                ? Result<AlgoModelDto>.Fail(ResultCode.NotFound, "算法不存在")
                : Result<AlgoModelDto>.Success(ToDto(model));
        }

        public async Task<Result<Paged<AlgoModelDto>>> ListAlgosAsync(AlgoModelQuery query)
        {
            var page = await _algoModelRepository.ListAsync(query);
            return Result<Paged<AlgoModelDto>>.Success(page);
        }

        public async Task<Result<AlgoModelDto>> GetEnabledAlgoByIdAsync(int id)
        {
            var model = await _algoModelRepository.FindByIdAsync(id);
            if (model == null)
                return Result<AlgoModelDto>.Fail(ResultCode.NotFound, "算法不存在");

            if (!model.Enabled)
                return Result<AlgoModelDto>.Fail(ResultCode.Forbidden, "算法未启用");

            return Result<AlgoModelDto>.Success(ToDto(model));
        }

        public async Task<Result> SoftDeleteAlgoAsync(int id, Guid actorId)
        {
            var model = await _algoModelRepository.FindByIdAsync(id);
            if (model == null)
                return Result.Fail(ResultCode.NotFound, "算法不存在");

            model.Enabled = false;
            model.DeletedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;
            await _algoModelRepository.UpdateAsync(model);
            Publish(AppEventTypes.AlgoUpdated, actorId, model, "soft delete algorithm");
            return Result.Success("删除成功");
        }

        private async Task<Result> ValidateForCreateAsync(AlgoModelCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return Result.Fail(ResultCode.InvalidInput, "算法名称不能为空");

            if (string.IsNullOrWhiteSpace(dto.ApiUrl))
                return Result.Fail(ResultCode.InvalidInput, "API URL不能为空");

            var existing = await _algoModelRepository.FindByNameAsync(dto.Name.Trim());
            if (existing != null)
                return Result.Fail(ResultCode.InfoExist, "算法名称已存在");

            _ = SerializeReservedJson(dto.ReservedJson);
            return Result.Success();
        }

        private static string? SerializeReservedJson(object? reservedJson)
        {
            if (reservedJson == null)
                return null;

            return reservedJson is JsonElement jsonElement
                ? jsonElement.GetRawText()
                : JsonSerializer.Serialize(reservedJson);
        }

        private void Publish(string eventType, Guid actorId, AlgoModel model, string summary)
        {
            _eventBus.Publish(new AppEvent
            {
                EventType = eventType,
                ActorId = actorId,
                TargetType = "algo",
                TargetId = model.Id.ToString(),
                Payload = new Dictionary<string, object?>
                {
                    ["name"] = model.Name,
                    ["enabled"] = model.Enabled,
                    ["api_url"] = model.ApiUrl
                },
                RequestContext = _requestContextFactory.Create(summary)
            });
        }

        private static AlgoModelDto ToDto(AlgoModel model) => new()
        {
            Id = model.Id,
            Name = model.Name,
            Enabled = model.Enabled,
            ApiUrl = model.ApiUrl,
            Description = model.Description,
            ReservedJson = model.ReservedJson,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt
        };
    }
}
