using SIFS.Infrastructure.Database;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Repositories
{
    public class AlgoTaskRepository: IAlgoTaskRepository
    {
        private readonly SIFSContext _context;
        public AlgoTaskRepository(SIFSContext context)
        {
            _context = context;
        }

        //public async Task<Result<>> 
    }
}
