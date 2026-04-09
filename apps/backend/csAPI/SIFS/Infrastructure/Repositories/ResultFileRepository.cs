using Microsoft.EntityFrameworkCore;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Repositories
{
    public class ResultFileRepository: IResultFileRepository
    {
        private readonly SIFSContext _context;
        public ResultFileRepository(SIFSContext context)
        {
            _context = context;
        }
        public async Task InsertAsync(ResultFile resultFile)
        {
            _context.ResultFiles.Add(resultFile);
            await _context.SaveChangesAsync();
        }
        public async Task<Result<ResultFile>> GetByIdAsync(Guid id)
        {
            var resultFile = await _context.ResultFiles.FirstOrDefaultAsync(f => f.Id == id);
            return resultFile != null
                ? Result<ResultFile>.Success(resultFile)
                : Result<ResultFile>.Fail(ResultCode.NotFound, "结果文件记录不存在");
        }
    }
}
