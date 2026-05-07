using Microsoft.EntityFrameworkCore;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Helpers;
using SIFS.Shared.Results;

namespace SIFS.Application.Rbac
{
    public class PermissionService : IPermissionService
    {
        private readonly SIFSContext _context;

        public PermissionService(SIFSContext context)
        {
            _context = context;
        }

        public async Task<Result> AssignRolesToUserAsync(Guid userId, IEnumerable<string> roleNames)
        {
            var normalizedRoleNames = roleNames
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (normalizedRoleNames.Count == 0)
                return Result.Fail(ResultCode.InvalidInput, "请至少指定一个角色");

            var roles = await _context.Roles
                .Where(r => normalizedRoleNames.Contains(r.Name))
                .ToListAsync();

            var missingRoleNames = normalizedRoleNames
                .Except(roles.Select(r => r.Name), StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (missingRoleNames.Count > 0)
                return Result.Fail(ResultCode.NotFound, $"角色不存在: {string.Join(", ", missingRoleNames)}");

            var roleIds = roles.Select(r => r.Id).ToList();
            var existingRoleIds = await _context.UserRoles
                .Where(ur => ur.UserId == userId && roleIds.Contains(ur.RoleId))
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var newUserRoles = roleIds
                .Except(existingRoleIds)
                .Select(roleId => new UserRole
                {
                    Id = UuidV7.NewUuidV7(),
                    UserId = userId,
                    RoleId = roleId
                })
                .ToList();

            if (newUserRoles.Count > 0)
            {
                await _context.UserRoles.AddRangeAsync(newUserRoles);
                await _context.SaveChangesAsync();
            }

            return Result.Success("角色绑定成功");
        }

        public async Task<Result<List<string>>> GetUserPermissionsAsync(Guid userId)
        {
            var permissions = await (
                    from permission in _context.Permissions
                    join rolePermission in _context.RolePermissions
                        on permission.Id equals rolePermission.PermissionId
                    join role in _context.Roles
                        on rolePermission.RoleId equals role.Id
                    join userRole in _context.UserRoles
                        on role.Id equals userRole.RoleId
                    where userRole.UserId == userId
                    select permission.Code
                )
                .Distinct()
                .OrderBy(code => code)
                .ToListAsync();

            return Result<List<string>>.Success(permissions);
        }

        public async Task<Result<List<string>>> GetUserRolesAsync(Guid userId)
        {
            var roles = await (
                    from role in _context.Roles
                    join userRole in _context.UserRoles
                        on role.Id equals userRole.RoleId
                    where userRole.UserId == userId
                    select role.Name
                )
                .Distinct()
                .OrderBy(name => name)
                .ToListAsync();

            return Result<List<string>>.Success(roles);
        }

        public async Task<Result<bool>> HasPermissionAsync(Guid userId, string permissionCode)
        {
            if (string.IsNullOrWhiteSpace(permissionCode))
                return Result<bool>.Fail(ResultCode.InvalidInput, "权限编码不能为空");

            var hasPermission = await (
                    from permission in _context.Permissions
                    join rolePermission in _context.RolePermissions
                        on permission.Id equals rolePermission.PermissionId
                    join userRole in _context.UserRoles
                        on rolePermission.RoleId equals userRole.RoleId
                    where userRole.UserId == userId && permission.Code == permissionCode.Trim()
                    select permission.Id
                )
                .AnyAsync();

            return Result<bool>.Success(hasPermission);
        }

        public async Task<Result<bool>> HasRoleAsync(Guid userId, string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return Result<bool>.Fail(ResultCode.InvalidInput, "角色名称不能为空");

            var normalizedRoleName = roleName.Trim();

            var hasRole = await (
                    from role in _context.Roles
                    join userRole in _context.UserRoles
                        on role.Id equals userRole.RoleId
                    where userRole.UserId == userId && role.Name == normalizedRoleName
                    select role.Id
                )
                .AnyAsync();

            return Result<bool>.Success(hasRole);
        }
    }
}
