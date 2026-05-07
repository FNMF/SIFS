using Microsoft.EntityFrameworkCore;
using SIFS.Application.ModelHealthChecks;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Repositories
{
    public class ModelHealthCheckRepository : IModelHealthCheckRepository
    {
        private readonly SIFSContext _context;

        public ModelHealthCheckRepository(SIFSContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(ModelHealthCheck modelHealthCheck)
        {
            await _context.ModelHealthChecks.AddAsync(modelHealthCheck);
            await _context.SaveChangesAsync();
        }

        public async Task<ModelHealthCheck?> GetLatestByAlgoModelIdAsync(int algoModelId)
        {
            return await _context.ModelHealthChecks
                .AsNoTracking()
                .Where(x => x.AlgoModelId == algoModelId)
                .OrderByDescending(x => x.CheckedAt)
                .ThenByDescending(x => x.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<Dictionary<int, ModelHealthCheck>> GetLatestByAlgoModelIdsAsync(IEnumerable<int> algoModelIds)
        {
            var ids = algoModelIds.Distinct().ToList();
            if (!ids.Any())
                return new Dictionary<int, ModelHealthCheck>();

            var checks = await _context.ModelHealthChecks
                .AsNoTracking()
                .Where(x => ids.Contains(x.AlgoModelId))
                .OrderByDescending(x => x.CheckedAt)
                .ThenByDescending(x => x.Id)
                .ToListAsync();

            return checks
                .GroupBy(x => x.AlgoModelId)
                .ToDictionary(x => x.Key, x => x.First());
        }

        public async Task<Paged<ModelHealthStatusDto>> ListLatestStatusesAsync(ModelHealthStatusQuery query)
        {
            var page = Math.Max(query.Page, 1);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);

            var algos = _context.AlgoModels
                .AsNoTracking()
                .Where(x => x.DeletedAt == null);

            if (query.Enabled.HasValue)
                algos = algos.Where(x => x.Enabled == query.Enabled.Value);

            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                var keyword = query.Keyword.Trim();
                algos = algos.Where(x => x.Name.Contains(keyword) || x.ApiUrl.Contains(keyword));
            }

            var allAlgos = await algos.ToListAsync();
            var latest = await GetLatestByAlgoModelIdsAsync(allAlgos.Select(x => x.Id));

            var items = allAlgos.Select(algo =>
            {
                latest.TryGetValue(algo.Id, out var health);
                var status = algo.Enabled
                    ? health?.Status ?? ModelHealthStatus.Offline
                    : "disabled";

                return new ModelHealthStatusDto
                {
                    AlgoModelId = algo.Id,
                    Name = algo.Name,
                    Enabled = algo.Enabled,
                    ApiUrl = algo.ApiUrl,
                    HealthStatus = status,
                    ResponseTimeMs = health?.ResponseTimeMs,
                    CheckedAt = health?.CheckedAt,
                    FailureReason = health?.FailureReason,
                    Description = algo.Description
                };
            });

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                var status = query.Status.Trim().ToLowerInvariant();
                items = items.Where(x => x.HealthStatus == status);
            }

            var ordered = items
                .OrderBy(x => x.HealthStatus == ModelHealthStatus.Offline || x.HealthStatus == ModelHealthStatus.Timeout ? 0 : 1)
                .ThenByDescending(x => x.CheckedAt ?? DateTime.MinValue)
                .ThenBy(x => x.Name)
                .ToList();

            return new Paged<ModelHealthStatusDto>
            {
                Data = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                Total = ordered.Count,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<ModelHealthSummaryDto> CountByLatestStatusAsync()
        {
            var enabledAlgos = await _context.AlgoModels
                .AsNoTracking()
                .Where(x => x.DeletedAt == null && x.Enabled)
                .Select(x => x.Id)
                .ToListAsync();

            var latest = await GetLatestByAlgoModelIdsAsync(enabledAlgos);

            return new ModelHealthSummaryDto
            {
                Online = enabledAlgos.Count(id => latest.TryGetValue(id, out var health) && health.Status == ModelHealthStatus.Online),
                Timeout = enabledAlgos.Count(id => latest.TryGetValue(id, out var health) && health.Status == ModelHealthStatus.Timeout),
                Offline = enabledAlgos.Count(id => !latest.TryGetValue(id, out var health) || health.Status == ModelHealthStatus.Offline)
            };
        }
    }
}
