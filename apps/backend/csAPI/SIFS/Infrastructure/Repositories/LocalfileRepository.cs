using Microsoft.EntityFrameworkCore;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Repositories
{
    public class LocalfileRepository : ILocalfileRepository
    {
        private readonly SIFSContext _context;
        public LocalfileRepository(SIFSContext context)
        {
            _context = context;
        }

        public async Task<Result<Localfile>> GetLocalfileByIdAsync(Guid id)
        {
            var localfile = await _context.Localfiles.FindAsync(id);
            return localfile != null
                ? Result<Localfile>.Success(localfile)
                : Result<Localfile>.Fail(ResultCode.NotFound, "本地文件不存在");
        }
        public async Task<Result<List<Localfile>>> GetLocalfilesByAlgoTaskIdAsync(Guid algoId)
        {
            var localfiles = await _context.Localfiles.Where(lf => lf.AlgoTaskId == algoId).ToListAsync();
            return Result<List<Localfile>>.Success(localfiles);
        }
        public async Task<Result<Localfile>> CreateLocalfileAsync(Localfile localfile)
        {
            _context.Localfiles.Add(localfile);
            await _context.SaveChangesAsync();
            return Result<Localfile>.Success(localfile, "本地文件创建成功");
        }
        public async Task<Result<List<Localfile>>> CreateLocalfilesAsync(List<Localfile> localfiles)
        {
            _context.Localfiles.AddRange(localfiles);
            await _context.SaveChangesAsync();
            return Result<List<Localfile>>.Success(localfiles, "本地文件批量创建成功");
        }

    }
}
