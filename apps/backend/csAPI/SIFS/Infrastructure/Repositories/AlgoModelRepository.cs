using Microsoft.EntityFrameworkCore;
using SIFS.Application.AlgoModels;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Repositories
{
    public class AlgoModelRepository : IAlgoModelRepository
    {
        private readonly SIFSContext _context;

        public AlgoModelRepository(SIFSContext context)
        {
            _context = context;
        }

        public async Task<AlgoModel?> FindByIdAsync(int id, bool includeDeleted = false)
        {
            var query = _context.AlgoModels.AsQueryable();

            if (!includeDeleted)
                query = query.Where(x => x.DeletedAt == null);

            return await query.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<AlgoModel?> FindByNameAsync(string name, bool includeDeleted = false)
        {
            var query = _context.AlgoModels.AsQueryable();

            if (!includeDeleted)
                query = query.Where(x => x.DeletedAt == null);

            return await query.FirstOrDefaultAsync(x => x.Name == name);
        }

        public async Task CreateAsync(AlgoModel model)
        {
            await _context.AlgoModels.AddAsync(model);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AlgoModel model)
        {
            _context.AlgoModels.Update(model);
            await _context.SaveChangesAsync();
        }

        public async Task<Paged<AlgoModelDto>> ListAsync(AlgoModelQuery query)
        {
            var page = Math.Max(query.Page, 1);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);

            var models = _context.AlgoModels
                .AsNoTracking()
                .Where(x => x.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(query.Name))
            {
                var name = query.Name.Trim();
                models = models.Where(x => x.Name.Contains(name));
            }

            if (query.Enabled.HasValue)
                models = models.Where(x => x.Enabled == query.Enabled.Value);

            var total = await models.CountAsync();
            var items = await models
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AlgoModelDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Enabled = x.Enabled,
                    ApiUrl = x.ApiUrl,
                    Description = x.Description,
                    ReservedJson = x.ReservedJson,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

            return new Paged<AlgoModelDto>
            {
                Data = items,
                Total = total,
                PageNumber = page,
                PageSize = pageSize
            };
        }
    }
}
