using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.Persistence.Models;

namespace SIFS.Infrastructure.Repositories
{
    public class TaskTypeMapRepository: ITaskTypeMapRepository
    {
        private readonly SIFSContext _context;
        public TaskTypeMapRepository(SIFSContext context)
        {
            _context = context;
        }
        public async Task InsertAsync(TaskTypeMap taskTypeMap)
        {
            _context.TaskTypeMaps.Add(taskTypeMap);
            await _context.SaveChangesAsync();
        }
    }
}
