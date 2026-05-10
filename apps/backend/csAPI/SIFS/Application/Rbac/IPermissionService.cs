using SIFS.Shared.Results;

namespace SIFS.Application.Rbac
{
    public interface IPermissionService
    {
        Task<Result> AssignRolesToUserAsync(Guid userId, IEnumerable<string> roleNames);

        Task<Result> SetUserRolesAsync(Guid userId, IEnumerable<string> roleNames);

        Task<Result<List<RoleReadDto>>> GetRolesAsync();

        Task<Result<List<string>>> GetUserPermissionsAsync(Guid userId);

        Task<Result<List<string>>> GetUserRolesAsync(Guid userId);

        Task<Result<bool>> HasPermissionAsync(Guid userId, string permissionCode);

        Task<Result<bool>> HasRoleAsync(Guid userId, string roleName);
    }
}
