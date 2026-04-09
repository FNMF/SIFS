using SIFS.Infrastructure.Persistence.Models;

namespace SIFS.Infrastructure.Repositories
{
    public interface ITaskTypeMapRepository
    {
        Task InsertAsync(TaskTypeMap taskTypeMap);
    }
}
