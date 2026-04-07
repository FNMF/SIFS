using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.Persistence.Models;

namespace SIFS.Infrastructure.Repositories
{
    public class TaskListRepository: ITaskListRepository
    {
        private readonly SIFSContext _context;
        public TaskListRepository(SIFSContext context)
        {
            _context = context;
        }
        public async Task InsertAsync(TaskList taskList)
        {
            await _context.TaskLists.AddAsync(taskList);
            await _context.SaveChangesAsync();
        }
    }
}
