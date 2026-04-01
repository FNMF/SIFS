using Microsoft.EntityFrameworkCore;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Results;

namespace SIFS.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly SIFSContext _context;
        public UserRepository(SIFSContext context)
        {
            _context = context;
        }
        public async Task<Result<User>> GetUserByIdAsync(Guid userId)
        {
            IQueryable<User> query = _context.Users.Where(u => u.Id == userId);
            return await query.FirstOrDefaultAsync() is User user
                ? Result<User>.Success(user)
                : Result<User>.Fail(ResultCode.NotFound, "用户不存在");
        }

        public async Task<Result<User>> GetUserByAccountAsync(string account)
        {
            IQueryable<User> query = _context.Users.Where(u => u.Account == account);
            return await query.FirstOrDefaultAsync() is User user
                ? Result<User>.Success(user)
                : Result<User>.Fail(ResultCode.NotFound, "用户不存在");
        }

        public async Task<Result<User>> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Result<User>.Success(user, "用户创建成功");
        }
    }
}
