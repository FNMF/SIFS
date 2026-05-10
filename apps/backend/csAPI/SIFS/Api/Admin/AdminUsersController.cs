using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIFS.Infrastructure.Authorization;
using SIFS.Infrastructure.Database;

namespace SIFS.Api.Admin
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize]
    [RequirePermission("admin:access")]
    public class AdminUsersController : ControllerBase
    {
        private readonly SIFSContext _context;

        public AdminUsersController(SIFSContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Query([FromQuery] AdminUserQuery query)
        {
            var page = Math.Max(query.Page, 1);
            var pageSize = query.PageSize > 0 ? query.PageSize : query.Page_Size;
            pageSize = Math.Clamp(pageSize <= 0 ? 20 : pageSize, 1, 100);
            var keyword = query.Keyword?.Trim();

            var usersQuery = _context.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                usersQuery = Guid.TryParse(keyword, out var userId)
                    ? usersQuery.Where(user => user.Id == userId || user.Account.Contains(keyword))
                    : usersQuery.Where(user => user.Account.Contains(keyword));
            }

            var total = await usersQuery.CountAsync();
            var items = await usersQuery
                .OrderBy(user => user.Account)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(user => new
                {
                    id = user.Id,
                    account = user.Account,
                    username = user.Account
                })
                .ToListAsync();

            return Ok(new
            {
                items,
                total,
                page,
                page_size = pageSize
            });
        }
    }

    public class AdminUserQuery
    {
        public string? Keyword { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        [FromQuery(Name = "page_size")]
        public int Page_Size { get; set; }
    }
}
