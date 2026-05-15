using CoreKit.IAM.Entities;

namespace CoreKit.IAM.Interfaces;

public interface IRoleManagementService
{
    Task<List<Role>> GetRolesAsync(Guid? tenantId);

    Task<Role?> GetRoleByIdAsync(Guid roleId, Guid? tenantId);

    Task<Role> CreateRoleAsync(string name, Guid? tenantId, string? description = null);

    Task UpdateRoleAsync(Guid roleId, string? newName, string? newDescription, Guid? tenantId);

    Task DeleteRoleAsync(Guid roleId, Guid? tenantId);

    Task AssignPermissionAsync(Guid roleId, Guid permissionId);

    Task RemovePermissionAsync(Guid roleId, Guid permissionId);

    Task<List<Permission>> GetPermissionsForRoleAsync(Guid roleId);
}