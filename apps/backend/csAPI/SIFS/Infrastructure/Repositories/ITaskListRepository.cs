using SIFS.Infrastructure.Persistence.Models;

namespace SIFS.Infrastructure.Repositories
{
    public interface ITaskListRepository
    {
        Task InsertAsync(TaskList taskList);
    }
}
